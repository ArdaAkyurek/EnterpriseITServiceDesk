using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.Application.DTOs.Ticket
{
    namespace EnterpriseIT.Application.DTOs.Ticket
    {
        public class CreateTicketDto
        {
            [Required, StringLength(150)]
            public string Title { get; set; } = string.Empty;

            [Required, StringLength(10000)]
            public string Description { get; set; } = string.Empty;

            [Range(1, int.MaxValue)]
            public int CategoryId { get; set; }

            [Range(1, int.MaxValue)]
            public int PriorityId { get; set; }

            [Range(1, int.MaxValue)]
            public int? AssetId { get; set; }
        }
    }
}
