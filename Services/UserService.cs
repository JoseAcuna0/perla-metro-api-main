using MainApi.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MainApi.Services;

/// <summary>
/// Interfaz para el servicio de usuarios que define las operaciones de autenticación
/// y gestión de usuarios contra el User Service (FastAPI)
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Autentica un usuario contra el User Service
    /// </summary>
    /// <param name="request">Credenciales de login</param>
    /// <returns>Respuesta de login con token JWT, null si falla</returns>
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Registra un nuevo usuario en el User Service
    /// </summary>
    /// <param name="request">Datos del usuario a registrar</param>
    /// <returns>Usuario creado, null si falla</returns>
    Task<User?> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// Obtiene información de la sesión actual del usuario
    /// </summary>
    /// <param name="token">Token JWT del usuario</param>
    /// <returns>Información de sesión, null si el token es inválido</returns>
    Task<SessionInfo?> GetSessionAsync(string token);
    
    /// <summary>
    /// Obtiene información detallada de un usuario por su ID
    /// </summary>
    /// <param name="userId">UUID del usuario</param>
    /// <param name="token">Token JWT para autenticación</param>
    /// <returns>Datos del usuario, null si no se encuentra</returns>
    Task<User?> GetUserByIdAsync(string userId, string token);
}

/// <summary>
/// Implementación del servicio de usuarios que actúa como cliente HTTP
/// hacia el User Service (FastAPI) desplegado en Render
/// </summary>
/// <remarks>
/// Este servicio encapsula todas las llamadas HTTP al User Service,
/// manejando la serialización/deserialización JSON y la autenticación.
/// 
/// URL del servicio: https://perla-metro-users-service-j9el.onrender.com
/// </remarks>
public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly JsonSerializerOptions _responseOptions;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de usuarios
    /// </summary>
    /// <param name="httpClient">Cliente HTTP para realizar peticiones al User Service</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <exception cref="ArgumentNullException">Se lanza si httpClient o configuration son null</exception>
    public UserService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        // Obtener URL del User Service desde configuración con fallback
        _baseUrl = _configuration["UserService:BaseUrl"] ?? "https://perla-metro-users-service-j9el.onrender.com";
        
        // Configurar opciones de serialización JSON para peticiones salientes
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true // Para mejor legibilidad en logs
        };
        
        // Configurar opciones de deserialización para respuestas del User Service
        // El User Service (FastAPI) envía respuestas en snake_case, pero con JsonPropertyName
        // en los modelos, podemos usar CamelCase y ser case-insensitive
        _responseOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true // Crucial para mapear snake_case a PascalCase
        };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var loginData = new
            {
                email = request.Email,
                password = request.Password
            };

            var json = JsonSerializer.Serialize(loginData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseJson, _responseOptions);
                return loginResponse;
            }

            return null;
        }
        catch (Exception ex)
        {
            // Log exception
            Console.WriteLine($"Error en login: {ex.Message}");
            return null;
        }
    }

    public async Task<User?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var registerData = new
            {
                full_name = request.FullName,
                email = request.Email,
                password = request.Password
            };

            var json = JsonSerializer.Serialize(registerData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/users/", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(responseJson, _responseOptions);
                return user;
            }

            return null;
        }
        catch (Exception ex)
        {
            // Log exception
            Console.WriteLine($"Error en registro: {ex.Message}");
            return null;
        }
    }

    public async Task<SessionInfo?> GetSessionAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/auth/session");
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var sessionInfo = JsonSerializer.Deserialize<SessionInfo>(responseJson, _responseOptions);
                return sessionInfo;
            }

            return null;
        }
        catch (Exception ex)
        {
            // Log exception
            Console.WriteLine($"Error obteniendo sesión: {ex.Message}");
            return null;
        }
        finally
        {
            // Limpiar el header de autorización
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<User?> GetUserByIdAsync(string userId, string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/users/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(responseJson, _responseOptions);
                return user;
            }

            return null;
        }
        catch (Exception ex)
        {
            // Log exception
            Console.WriteLine($"Error obteniendo usuario: {ex.Message}");
            return null;
        }
        finally
        {
            // Limpiar el header de autorización
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}