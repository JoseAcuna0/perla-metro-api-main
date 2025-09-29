using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MainApi.src.Enums;

namespace MainApi.src.Dtos
{
    public class UpdateDto
    {
        public DateTime? issueDate { get; set; } 
        [EnumDataType(typeof(TicketType))]
        public TicketType? Type { get; set; }
        [EnumDataType(typeof(TicketState))]
        public TicketState? State { get; set; } 
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal? Price { get; set; }
    }
}