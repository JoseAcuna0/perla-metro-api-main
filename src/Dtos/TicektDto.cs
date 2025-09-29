using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MainApi.src.Dtos
{
    public class TicektDto
    {
            public string? Id { get; set; }
            public string? IdUser { get; set; }
            public string? Type { get; set; }
            public string? State { get; set; }
            public DateTime IssueDate { get; set; }
            public decimal Price { get; set; }
    }
}