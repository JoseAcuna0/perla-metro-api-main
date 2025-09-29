#!/bin/bash

# =====================================================
# Script de Pruebas Automáticas - Gateway API
# =====================================================
# Prueba todos los endpoints implementados para verificar
# la integración con el User Service de FastAPI
#
# Fecha: $(date)
# =====================================================

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuración
BASE_URL="http://localhost:5000"
USER_SERVICE_URL="https://perla-metro-users-service-j9el.onrender.com"

# Variables globales para tests
TOKEN=""
USER_ID=""
TEST_EMAIL="test.$(date +%s)@perlametro.cl"
TEST_PASSWORD="TestPass123!"

# =====================================================
# FUNCIONES AUXILIARES
# =====================================================

print_header() {
    echo -e "${BLUE}=====================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}=====================================${NC}"
}

print_step() {
    echo -e "${YELLOW}[STEP]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

check_dependency() {
    if ! command -v $1 &> /dev/null; then
        print_error "$1 no está instalado. Instálalo con: sudo apt-get install $1"
        exit 1
    fi
}

wait_for_service() {
    local url=$1
    local max_attempts=30
    local attempt=0
    
    print_step "Esperando a que el servicio esté disponible en $url..."
    
    while [ $attempt -lt $max_attempts ]; do
        if curl -s -o /dev/null -w "%{http_code}" "$url" | grep -q "200\|404\|401"; then
            print_success "Servicio disponible"
            return 0
        fi
        
        attempt=$((attempt + 1))
        echo -n "."
        sleep 2
    done
    
    print_error "Servicio no disponible después de $max_attempts intentos"
    return 1
}

# =====================================================
# FUNCIONES DE PRUEBA
# =====================================================

test_health_check() {
    print_step "Verificando conectividad con User Service..."
    
    local response=$(curl -s -o /dev/null -w "%{http_code}" "$USER_SERVICE_URL/docs")
    
    if [[ $response == "200" ]]; then
        print_success "User Service está funcionando"
    else
        print_error "User Service no responde (HTTP $response)"
        return 1
    fi
}

test_register() {
    print_step "Probando endpoint de registro..."
    
    local payload=$(cat <<EOF
{
    "fullName": "Usuario de Prueba",
    "email": "$TEST_EMAIL",
    "password": "$TEST_PASSWORD"
}
EOF
)
    
    local response=$(curl -s -X POST "$BASE_URL/api/auth/register" \
        -H "Content-Type: application/json" \
        -d "$payload")
    
    # Verificar que la respuesta contiene éxito
    if echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Registro exitoso"
        
        # Extraer ID del usuario para pruebas posteriores
        USER_ID=$(echo "$response" | jq -r '.data.id')
        print_info "Usuario creado con ID: $USER_ID"
        
        return 0
    else
        print_error "Fallo en registro"
        echo "Respuesta: $response"
        return 1
    fi
}

test_login() {
    print_step "Probando endpoint de login..."
    
    local payload=$(cat <<EOF
{
    "email": "$TEST_EMAIL",
    "password": "$TEST_PASSWORD"
}
EOF
)
    
    local response=$(curl -s -X POST "$BASE_URL/api/auth/login" \
        -H "Content-Type: application/json" \
        -d "$payload")
    
    # Verificar que la respuesta contiene éxito
    if echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Login exitoso"
        
        # Extraer token para pruebas posteriores
        TOKEN=$(echo "$response" | jq -r '.data.access_token')
        print_info "Token obtenido (primeros 20 chars): ${TOKEN:0:20}..."
        
        # Verificar que isAdmin está presente
        local is_admin=$(echo "$response" | jq -r '.data.is_admin')
        print_info "Usuario es admin: $is_admin"
        
        return 0
    else
        print_error "Fallo en login"
        echo "Respuesta: $response"
        return 1
    fi
}

test_session() {
    print_step "Probando endpoint de sesión..."
    
    if [[ -z "$TOKEN" ]]; then
        print_error "No hay token disponible para la prueba"
        return 1
    fi
    
    local response=$(curl -s -X GET "$BASE_URL/api/auth/session" \
        -H "Authorization: Bearer $TOKEN")
    
    # Verificar que la respuesta contiene éxito
    if echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Información de sesión obtenida"
        
        # Verificar campos importantes
        local user_id=$(echo "$response" | jq -r '.data.user_id')
        local is_admin=$(echo "$response" | jq -r '.data.is_admin')
        local email=$(echo "$response" | jq -r '.data.email')
        
        print_info "User ID: $user_id"
        print_info "Is Admin: $is_admin"
        print_info "Email: $email"
        
        # Verificar que el campo isAdmin está presente (crucial para otros devs)
        if [[ "$is_admin" != "null" ]]; then
            print_success "Campo isAdmin presente - otros desarrolladores pueden verificar roles"
        else
            print_error "Campo isAdmin faltante"
            return 1
        fi
        
        return 0
    else
        print_error "Fallo en obtener sesión"
        echo "Respuesta: $response"
        return 1
    fi
}

test_get_user_by_id() {
    print_step "Probando endpoint de obtener usuario por ID..."
    
    if [[ -z "$TOKEN" || -z "$USER_ID" ]]; then
        print_error "No hay token o user_id disponible para la prueba"
        return 1
    fi
    
    local response=$(curl -s -X GET "$BASE_URL/api/auth/users/$USER_ID" \
        -H "Authorization: Bearer $TOKEN")
    
    # Verificar que la respuesta contiene éxito
    if echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Usuario obtenido por ID"
        
        # Verificar que isAdmin está presente
        local is_admin=$(echo "$response" | jq -r '.data.is_admin')
        print_info "Usuario es admin: $is_admin"
        
        return 0
    else
        print_error "Fallo en obtener usuario por ID"
        echo "Respuesta: $response"
        return 1
    fi
}

test_logout() {
    print_step "Probando endpoint de logout..."
    
    local response=$(curl -s -X POST "$BASE_URL/api/auth/logout")
    
    # Verificar que la respuesta contiene éxito
    if echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Logout exitoso"
        
        # Verificar que contiene timestamp
        local logged_out=$(echo "$response" | jq -r '.data.loggedOut')
        print_info "Logout confirmado: $logged_out"
        
        return 0
    else
        print_error "Fallo en logout"
        echo "Respuesta: $response"
        return 1
    fi
}

test_unauthorized_access() {
    print_step "Probando acceso no autorizado..."
    
    local response=$(curl -s -X GET "$BASE_URL/api/auth/session" \
        -H "Authorization: Bearer token-invalido")
    
    # Verificar que la respuesta indica fallo
    if echo "$response" | jq -e '.success == false' > /dev/null 2>&1; then
        print_success "Acceso no autorizado correctamente rechazado"
        return 0
    else
        print_error "El sistema no rechazó correctamente el acceso no autorizado"
        echo "Respuesta: $response"
        return 1
    fi
}

# =====================================================
# FUNCIÓN PRINCIPAL
# =====================================================

run_all_tests() {
    print_header "INICIANDO PRUEBAS DEL GATEWAY API"
    
    local total_tests=0
    local passed_tests=0
    local failed_tests=0
    
    # Verificar dependencias
    check_dependency "curl"
    check_dependency "jq"
    
    # Verificar que el servicio local esté funcionando
    if ! wait_for_service "$BASE_URL/swagger/index.html"; then
        print_error "El Gateway API no está funcionando en $BASE_URL"
        print_info "Ejecuta 'dotnet run --urls=\"http://localhost:5000\"' primero"
        exit 1
    fi
    
    # Array de funciones de prueba
    local tests=(
        "test_health_check"
        "test_register" 
        "test_login"
        "test_session"
        "test_get_user_by_id"
        "test_logout"
        "test_unauthorized_access"
    )
    
    # Ejecutar todas las pruebas
    for test_func in "${tests[@]}"; do
        total_tests=$((total_tests + 1))
        echo ""
        
        if $test_func; then
            passed_tests=$((passed_tests + 1))
        else
            failed_tests=$((failed_tests + 1))
        fi
        
        sleep 1  # Pausa entre pruebas
    done
    
    # Resumen final
    echo ""
    print_header "RESUMEN DE PRUEBAS"
    echo -e "Total de pruebas: ${BLUE}$total_tests${NC}"
    echo -e "Pruebas exitosas: ${GREEN}$passed_tests${NC}"
    echo -e "Pruebas fallidas: ${RED}$failed_tests${NC}"
    
    if [[ $failed_tests -eq 0 ]]; then
        echo ""
        print_success "¡TODAS LAS PRUEBAS PASARON! ✅"
        print_success "El Gateway API está funcionando correctamente"
        print_info "Los otros desarrolladores pueden usar:"
        print_info "- GET /api/auth/session para verificar si isAdmin = true"
        print_info "- Todos los endpoints de autenticación funcionan"
        echo ""
        return 0
    else
        echo ""
        print_error "ALGUNAS PRUEBAS FALLARON ❌"
        print_error "Revisa los errores anteriores antes de hacer commit"
        echo ""
        return 1
    fi
}

# =====================================================
# VERIFICACIÓN DE ARGUMENTOS Y EJECUCIÓN
# =====================================================

show_help() {
    echo "Script de Pruebas Automáticas - Gateway API"
    echo ""
    echo "Uso: $0 [opciones]"
    echo ""
    echo "Opciones:"
    echo "  -h, --help     Muestra esta ayuda"
    echo "  -v, --verbose  Modo verbose (más detalles)"
    echo "  -q, --quiet    Modo silencioso (solo errores)"
    echo ""
    echo "Ejemplos:"
    echo "  $0              # Ejecutar todas las pruebas"
    echo "  $0 --verbose    # Ejecutar con más detalles"
    echo ""
    echo "Prerrequisitos:"
    echo "  - El Gateway API debe estar ejecutándose en http://localhost:5000"
    echo "  - Conexión a internet (para acceder al User Service)"
    echo "  - curl y jq instalados"
}

# Procesar argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_help
            exit 0
            ;;
        -v|--verbose)
            set -x  # Habilitar modo verbose
            shift
            ;;
        -q|--quiet)
            exec > /dev/null 2>&1  # Silenciar output
            shift
            ;;
        *)
            print_error "Opción desconocida: $1"
            show_help
            exit 1
            ;;
    esac
done

# Ejecutar pruebas
run_all_tests
exit $?