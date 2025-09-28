using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MainApi.src.Controllers
{
    /// <summary>
    /// Controlador de la Main API que expone los endpoints relacionados con Rutas.
    /// Esta clase funciona como un "gateway", ya que reenvía las solicitudes al microservicio
    /// de rutas desplegado externamente (ej: en Render).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// Recibe un <see cref="HttpClient"/> para hacer llamadas HTTP
        /// y la configuración <see cref="IConfiguration"/> para obtener la URL base del microservicio.
        /// </summary>
        public RoutesController(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        /// <summary>
        /// Obtiene todas las rutas desde el microservicio de rutas.
        /// </summary>
        /// <returns>Lista de rutas en formato JSON.</returns>
        // GET api/routes
        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.GetAsync($"{baseUrl}/api/routes");

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

        /// <summary>
        /// Obtiene una ruta específica según su identificador.
        /// </summary>
        /// <param name="id">ID de la ruta.</param>
        /// <returns>La ruta encontrada en formato JSON o un 404 si no existe.</returns>
        // GET api/routes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteById(string id)
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.GetAsync($"{baseUrl}/api/routes/{id}");

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

        /// <summary>
        /// Crea una nueva ruta en el microservicio de rutas.
        /// </summary>
        /// <param name="routeDto">Objeto JSON con los datos de la ruta.</param>
        /// <returns>Ruta creada en formato JSON.</returns>
        // POST api/routes
        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] object routeDto)
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/routes", routeDto);

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

        /// <summary>
        /// Actualiza una ruta existente en el microservicio de rutas.
        /// </summary>
        /// <param name="id">ID de la ruta a modificar.</param>
        /// <param name="routeDto">Objeto JSON con los datos actualizados.</param>
        /// <returns>Ruta modificada en formato JSON.</returns>
        // PUT api/routes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(string id, [FromBody] object routeDto)
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.PutAsJsonAsync($"{baseUrl}/api/routes/{id}", routeDto);

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

        /// <summary>
        /// Elimina una ruta (soft delete) en el microservicio de rutas.
        /// </summary>
        /// <param name="id">ID de la ruta a eliminar.</param>
        /// <returns>Confirmación de eliminación.</returns>
        // DELETE api/routes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(string id)
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.DeleteAsync($"{baseUrl}/api/routes/{id}");

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }
    }
}
