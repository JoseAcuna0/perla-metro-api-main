var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Registro de servicios en el contenedor de dependencias.
/// </summary>
builder.Services.AddControllers();            // Soporte para controladores de la API
builder.Services.AddEndpointsApiExplorer();   // Explorador de endpoints (Swagger/OpenAPI)
builder.Services.AddSwaggerGen();             // Generador de documentación Swagger

/// <summary>
/// Registro de HttpClient.
/// Este cliente se inyecta en los controladores y se utiliza para 
/// reenviar las solicitudes hacia los microservicios (ej: Routes Service en Render).
/// </summary>
builder.Services.AddHttpClient();

builder.Services.AddHttpClient("TicketService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5050"); 
});

var app = builder.Build();

/// <summary>
/// Configuración del pipeline HTTP.
/// Solo en entorno de desarrollo se habilita Swagger.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();  // Redirige todas las solicitudes HTTP a HTTPS
app.MapControllers();       // Mapea automáticamente los controladores de la API
app.Run();                  // Arranca la aplicación
