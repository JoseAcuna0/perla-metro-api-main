using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MainApi.src.Dtos;
using MainApi.src.Enums;
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


        [HttpPost("Add")]
        public async Task<IActionResult> AddTicket([FromForm] AddTicketDto addTicketDto)
        {
            var client = _httpClientFactory.CreateClient("TicketService");

            var response = await client.PostAsJsonAsync("/Add", addTicketDto);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var ticketResponse = await response.Content.ReadFromJsonAsync<object>();
            return Ok(ticketResponse);
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

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllTickets([FromQuery] string? userId, [FromQuery] DateTime? fecha, [FromQuery] TicketState? state)
        {
            var client = _httpClientFactory.CreateClient("TicketService");


            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(userId)) queryParams.Add($"userId={userId}");
            if (fecha.HasValue) queryParams.Add($"fecha={fecha.Value:yyyy-MM-dd}");
            if (state.HasValue) queryParams.Add($"state={state.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            var response = await client.GetAsync($"/GetAllTickets{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var tickets = await response.Content.ReadFromJsonAsync<List<TicektDto>>();
            return Ok(tickets);
        }
        

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateTicket(string id, [FromForm] UpdateDto updateTicketDto)
        {
            var client = _httpClientFactory.CreateClient("TicketService");

            // llamamos al endpoint del microservicio TicketService
            var response = await client.PutAsJsonAsync($"/Update/{id}", updateTicketDto);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteTicket(string id)
        {
            var client = _httpClientFactory.CreateClient("TicketService");

            var response = await client.DeleteAsync($"/Delete/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }


        
    }
}