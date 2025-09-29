using MainApi.Models;
using MainApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainApi.Controllers;

/// <summary>
/// Controlador de autenticación que actúa como gateway hacia el User Service.
/// Proporciona endpoints para login, logout, registro y gestión de sesiones.
/// Integra con el User Service (FastAPI) desplegado en https://perla-metro-users-service-j9el.onrender.com
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de autenticación
    /// </summary>
    /// <param name="userService">Servicio para interactuar con el User Service</param>
    public AuthController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <summary>
    /// Autentica un usuario y retorna un token JWT
    /// </summary>
    /// <param name="request">Credenciales de login (email institucional y contraseña)</param>
    /// <returns>Respuesta con token JWT y datos del usuario</returns>
    /// <response code="200">Login exitoso, retorna token JWT y datos del usuario</response>
    /// <response code="400">Datos de entrada inválidos (validación fallida)</response>
    /// <response code="401">Credenciales incorrectas</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "email": "usuario@perlametro.cl",
    ///         "password": "MiPassword123!"
    ///     }
    ///
    /// El email debe ser institucional (@perlametro.cl)
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 400)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 401)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 500)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var loginResponse = await _userService.LoginAsync(request);
            
            if (loginResponse != null)
            {
                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login exitoso",
                    Data = loginResponse
                });
            }

            return Unauthorized(new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "Credenciales incorrectas"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="request">Datos del usuario a registrar</param>
    /// <returns>Usuario creado exitosamente</returns>
    /// <response code="201">Usuario registrado exitosamente</response>
    /// <response code="400">Datos inválidos (email no institucional, contraseña débil, etc.)</response>
    /// <response code="409">El email ya está registrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/auth/register
    ///     {
    ///         "fullName": "Juan Pérez",
    ///         "email": "juan.perez@perlametro.cl",
    ///         "password": "Password123!"
    ///     }
    ///
    /// Validaciones aplicadas:
    /// - Email debe ser institucional (@perlametro.cl)
    /// - Contraseña mínimo 8 caracteres
    /// - FullName es opcional
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<User>), 201)]
    [ProducesResponseType(typeof(ApiResponse<User>), 400)]
    [ProducesResponseType(typeof(ApiResponse<User>), 409)]
    [ProducesResponseType(typeof(ApiResponse<User>), 500)]
    public async Task<ActionResult<ApiResponse<User>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _userService.RegisterAsync(request);
            
            if (user != null)
            {
                return Created($"/api/users/{user.Id}", new ApiResponse<User>
                {
                    Success = true,
                    Message = "Usuario registrado exitosamente",
                    Data = user
                });
            }

            return BadRequest(new ApiResponse<User>
            {
                Success = false,
                Message = "Error al registrar usuario. Verifique que el email sea institucional (@perlametro.cl) y la contraseña tenga al menos 8 caracteres."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<User>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Obtiene información de la sesión actual del usuario autenticado
    /// </summary>
    /// <returns>Información de la sesión incluyendo estado de administrador</returns>
    /// <response code="200">Información de sesión obtenida exitosamente</response>
    /// <response code="401">Token inválido, expirado o no proporcionado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **IMPORTANTE PARA COLABORADORES:**
    /// 
    /// Este endpoint es esencial para verificar si el usuario actual es administrador.
    /// Otros desarrolladores en sus ramas respectivas pueden usar este endpoint para:
    /// 
    /// 1. Verificar permisos administrativos: `data.isAdmin`
    /// 2. Obtener ID del usuario: `data.userId`
    /// 3. Verificar expiración del token: `data.expiresAt`
    /// 
    /// Ejemplo de uso en cliente JavaScript:
    /// 
    ///     const response = await fetch('/api/auth/session', {
    ///         headers: { 'Authorization': `Bearer ${token}` }
    ///     });
    ///     const { data } = await response.json();
    ///     
    ///     if (data.isAdmin) {
    ///         // Usuario tiene permisos administrativos
    ///         enableAdminFeatures();
    ///     }
    /// 
    /// Requiere header: `Authorization: Bearer {token}`
    /// </remarks>
    [HttpGet("session")]
    [ProducesResponseType(typeof(ApiResponse<SessionInfo>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SessionInfo>), 401)]
    [ProducesResponseType(typeof(ApiResponse<SessionInfo>), 500)]
    public async Task<ActionResult<ApiResponse<SessionInfo>>> GetSession()
    {
        try
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new ApiResponse<SessionInfo>
                {
                    Success = false,
                    Message = "Token de autorización requerido"
                });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var sessionInfo = await _userService.GetSessionAsync(token);
            
            if (sessionInfo != null)
            {
                return Ok(new ApiResponse<SessionInfo>
                {
                    Success = true,
                    Message = "Información de sesión obtenida exitosamente",
                    Data = sessionInfo
                });
            }

            return Unauthorized(new ApiResponse<SessionInfo>
            {
                Success = false,
                Message = "Token inválido o expirado"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<SessionInfo>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario (logout)
    /// </summary>
    /// <returns>Confirmación de cierre de sesión</returns>
    /// <response code="200">Logout exitoso</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **Nota sobre JWT Stateless:**
    /// 
    /// Debido a que el sistema utiliza tokens JWT stateless, el logout es una operación
    /// principalmente del lado del cliente. Este endpoint:
    /// 
    /// 1. Confirma la intención de logout
    /// 2. Proporciona un timestamp para auditoria
    /// 3. **EL CLIENTE DEBE descartar el token JWT**
    /// 
    /// Para un logout efectivo, el cliente debe:
    /// - Eliminar el token del localStorage/sessionStorage
    /// - Limpiar el estado de autenticación
    /// - Redirigir al usuario a la página de login
    /// </remarks>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public ActionResult<ApiResponse<object>> Logout()
    {
        try
        {
            // En un sistema JWT stateless, el logout se maneja en el cliente
            // descartando el token. Aquí solo confirmamos la acción.
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Sesión cerrada exitosamente. Descarte el token del cliente.",
                Data = new { loggedOut = true, timestamp = DateTime.UtcNow }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Obtiene información detallada de un usuario específico
    /// </summary>
    /// <param name="userId">UUID del usuario a consultar</param>
    /// <returns>Información completa del usuario incluyendo rol administrativo</returns>
    /// <response code="200">Usuario encontrado y retornado exitosamente</response>
    /// <response code="401">Token inválido o no proporcionado</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **Endpoint auxiliar para verificación de roles:**
    /// 
    /// Este endpoint proporciona una alternativa al endpoint `/session` para
    /// obtener información detallada de un usuario, incluyendo su rol administrativo.
    /// 
    /// Casos de uso:
    /// - Verificar si un usuario específico es administrador
    /// - Obtener detalles completos de perfil
    /// - Validaciones de permisos basadas en ID
    /// 
    /// Ejemplo de uso:
    /// 
    ///     GET /api/auth/users/c572e11f-d16a-4eb2-834b-e5cc31be8708
    ///     Authorization: Bearer {token}
    /// 
    /// La respuesta incluye `isAdmin: boolean` para verificación de roles.
    /// </remarks>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<User>), 200)]
    [ProducesResponseType(typeof(ApiResponse<User>), 401)]
    [ProducesResponseType(typeof(ApiResponse<User>), 404)]
    [ProducesResponseType(typeof(ApiResponse<User>), 500)]
    public async Task<ActionResult<ApiResponse<User>>> GetUserById(string userId)
    {
        try
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new ApiResponse<User>
                {
                    Success = false,
                    Message = "Token de autorización requerido"
                });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var user = await _userService.GetUserByIdAsync(userId, token);
            
            if (user != null)
            {
                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Message = "Usuario obtenido exitosamente",
                    Data = user
                });
            }

            return NotFound(new ApiResponse<User>
            {
                Success = false,
                Message = "Usuario no encontrado"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<User>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }
}