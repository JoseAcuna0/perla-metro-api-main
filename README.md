# 🚀 Perla Metro – Main API (Gateway de Servicios)

La **Main API** actúa como una fachada o gateway para el sistema **Perla Metro**.  
Su función principal es centralizar la comunicación entre los distintos servicios distribuidos (Rutas, Estaciones, Usuarios, Tickets), de manera que los clientes tengan un único punto de acceso.

---

## 🏗️ Arquitectura

El sistema está basado en una **arquitectura SOA (Service-Oriented Architecture)** con servicios distribuidos.

- Cada dominio principal (Rutas, Usuarios, Tickets) se desarrolla como un servicio independiente en .NET y se despliega en **Render**.
- Integración heterogénea: el **servicio de Estaciones** se desarrolla en Node.js/Express y se despliega en **Railway**.
- La Main API recibe las solicitudes de los clientes y las reenvía al servicio correspondiente mediante `HttpClient`.
- Los clientes interactúan únicamente con la Main API, sin conocer las URLs ni las tecnologías de los servicios internos.

### Diagrama simplificado

Cliente → Main API → [Auth/User Service] → User Service (FastAPI)
→ [Routes Service] → Routes Service (.NET)
→ [Stations Service] → Stations Service (Node.js/Railway)
→ [Tickets Service] → Tickets Service (.NET)

yaml
Copiar código

---

## 🛠️ Tecnologías utilizadas

- .NET 6/8 (ASP.NET Core Web API)  
- C# como lenguaje principal  
- Swagger / OpenAPI para documentación y pruebas  
- HttpClient para comunicación con microservicios  
- JWT (JSON Web Tokens) para autenticación stateless  
- MongoDB Atlas (solo para el Ticket Service)  
- Render y Railway como plataformas de despliegue en la nube  

---

## 🔐 Sistema de Autenticación (Auth/User Service)

La Main API incluye un **sistema de autenticación JWT** que actúa como gateway hacia el User Service (FastAPI).

### Características principales

- Autenticación JWT stateless: los tokens contienen toda la información necesaria  
- Integración con User Service: https://perla-metro-users-service-j9el.onrender.com  
- Verificación de roles administrativos: soporte para usuarios admin y regulares  
- Emails institucionales: validación de formato `@perlametro.cl`

### Endpoints de autenticación disponibles

| Endpoint | Método | Descripción |
|----------|-------|------------|
| `/api/auth/login` | POST | Inicia sesión, retorna token JWT, ID de usuario y rol admin |
| `/api/auth/register` | POST | Registra nuevos usuarios (requiere email institucional) |
| `/api/auth/session` | GET | Obtiene info del usuario autenticado (verificación de permisos) |
| `/api/auth/logout` | POST | Cierra sesión (el cliente descarta el token JWT) |
| `/api/auth/users/{userId}` | GET | Obtiene información detallada de un usuario específico |

---

## 🚊 Servicio de Rutas (Routes Service)

- **GET** `/api/routes` → Lista todas las rutas (valida estaciones en Railway)  
- **GET** `/api/routes/{id}` → Obtiene ruta específica  
- **POST** `/api/routes` → Crea nueva ruta (valida existencia y estado de estaciones)  
- **PUT** `/api/routes/{id}` → Actualiza ruta  
- **DELETE** `/api/routes/{id}` → Elimina (soft delete) ruta  

> ⚠️ Los endpoints de rutas dependen del **Stations Service** para validar estaciones (`is_active`).

---

## 🏢 Servicio de Estaciones (Stations Service)

- Servicio desarrollado en **Node.js/Express** y desplegado en Railway.  
- Proporciona información de estaciones y valida disponibilidad para Rutas.  
- Endpoints principales: `/api/stations`, `/api/stations/{id}` (GET), `/api/stations/validate` (POST).

---

## 🎟️ Servicio de Tickets (Tickets Service)

Repositorio: [https://github.com/Yinkov/perla-metro-ticket-service](https://github.com/Yinkov/perla-metro-ticket-service)  

El servicio de tickets gestiona los boletos de pasajeros: creación, consulta, actualización y eliminación. Funciona con **MongoDB Atlas**.

### Endpoints

| Endpoint | Método | Descripción |
|----------|-------|------------|
| `/api/tickets/add` | POST | Crear un nuevo ticket |
| `/api/tickets/{id}` | GET | Obtener ticket por ID |
| `/api/tickets` | GET | Listar tickets (filtros opcionales: `userId`, `fecha`, `state`) |
| `/api/tickets/update/{id}` | PUT | Actualizar ticket existente |
| `/api/tickets/delete/{id}` | DELETE | Eliminar ticket (soft delete) |

### Validaciones del Ticket Service

- `Price` debe ser mayor a 0  
- `Type` solo acepta `"Ida"` o `"Vuelta"`  
- `State` solo acepta `"Activo"`, `"Usado"` o `"Caducado"`  
- No se puede reactivar un ticket **Caducado**  
- Cada usuario no puede tener dos tickets con la misma `IssueDate` (índice único MongoDB)  
- Borrado lógico (`isActive = false`) para mantener historial  

---

## 🚀 Instalación y configuración

### 1. Clonar el repositorio

`bash
git clone https://github.com/JoseAcuna0/perla-metro-api-main.git
cd perla-metro-api-main
2. Configurar servicios en appsettings.json
json
Copiar código
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "UserService": {
    "BaseUrl": "https://perla-metro-users-service-j9el.onrender.com"
  },
  "Services": {
    "Routes": "https://perla-metro-routes-service.onrender.com",
    "Stations": "https://perla-metro-stations-service-XXXX.up.railway.app",
    "Tickets": "https://localhost:5050"
  }
}
⚠️ Reemplaza la URL del Stations Service por la real.
⚠️ Ticket Service escucha en https://localhost:5050/swagger.

3. Restaurar dependencias y ejecutar
bash
Copiar código
dotnet restore
dotnet run
La API arrancará en: https://localhost:5001

Swagger disponible en: https://localhost:5001/swagger

✅ Ejemplo de uso con Postman
Tickets
Crear ticket

http
Copiar código
POST https://localhost:5001/api/tickets/add
Content-Type: application/json

{
  "idUser": "12345",
  "issueDate": "2025-09-28T08:00:00",
  "price": 1200,
  "type": "Ida",
  "state": "Activo"
}
Obtener tickets por usuario

http
Copiar código
GET https://localhost:5001/api/tickets?userId=12345
