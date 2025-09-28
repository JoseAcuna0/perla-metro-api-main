using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MainApi.src.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public RoutesController(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        // GET api/routes
        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.GetAsync($"{baseUrl}/api/routes");

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

        // GET api/routes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteById(string id)
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.GetAsync($"{baseUrl}/api/routes/{id}");

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

        // POST api/routes
        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] object routeDto)
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/routes", routeDto);

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

        // PUT api/routes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(string id, [FromBody] object routeDto)
        {
            var baseUrl = _config["Services:Routes"];
            var response = await _httpClient.PutAsJsonAsync($"{baseUrl}/api/routes/{id}", routeDto);

            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

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