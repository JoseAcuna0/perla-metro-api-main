using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MainApi.src.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MainApi.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TicketController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(string id)
        {
            var client = _httpClientFactory.CreateClient("TicketService");
            var response = await client.GetAsync($"Get/{id}");
         
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var content = await response.Content.ReadFromJsonAsync<TicektDto>();
            return Ok(content);
        }

        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody] TicektDto ticket)
        {
            var client = _httpClientFactory.CreateClient("TicketService");
            var response = await client.PostAsJsonAsync("", ticket);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            return Ok("Ticket creado exitosamente desde Main API");
        }
    }
}