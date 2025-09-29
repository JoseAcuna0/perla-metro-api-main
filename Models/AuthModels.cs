using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MainApi.Models;

/// <summary>
/// Modelo para solicitudes de inicio de sesión
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Dirección de correo electrónico del usuario (debe ser institucional @perlametro.cl)
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = null!;
}

/// <summary>
/// Modelo para solicitudes de registro de nuevo usuario
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Nombre completo del usuario (opcional)
    /// </summary>
    public string? FullName { get; set; }
    
    /// <summary>
    /// Dirección de correo electrónico institucional (@perlametro.cl)
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// Contraseña del usuario (mínimo 8 caracteres)
    /// </summary>
    /// <remarks>
    /// La contraseña debe cumplir con los requisitos de seguridad del User Service:
    /// - Mínimo 8 caracteres
    /// - Al menos una mayúscula, minúscula, número y carácter especial
    /// </remarks>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; set; } = null!;
}

/// <summary>
/// Respuesta del User Service tras un login exitoso
/// </summary>
/// <remarks>
/// Este modelo mapea la respuesta JSON del User Service que utiliza snake_case
/// a propiedades C# en PascalCase mediante JsonPropertyName attributes
/// </remarks>
public class LoginResponse
{
    /// <summary>
    /// Token JWT para autenticación en subsecuentes peticiones
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;
    
    /// <summary>
    /// Tipo de token (siempre "bearer")
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "bearer";
    
    /// <summary>
    /// UUID único del usuario autenticado
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = null!;
    
    /// <summary>
    /// Dirección de correo electrónico del usuario
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// Indica si el usuario tiene privilegios administrativos
    /// </summary>
    /// <remarks>
    /// Este campo es crucial para que otros desarrolladores puedan
    /// verificar permisos administrativos del usuario autenticado
    /// </remarks>
    [JsonPropertyName("is_admin")]
    public bool IsAdmin { get; set; }
}

public class SessionInfo
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = null!;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
    
    [JsonPropertyName("is_admin")]
    public bool IsAdmin { get; set; }
    
    [JsonPropertyName("expires_at")]
    public string ExpiresAt { get; set; } = null!;
}

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("is_active")]
    public bool? IsActive { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("is_admin")]
    public bool IsAdmin { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; } = null!;
    public string? Details { get; set; }
}