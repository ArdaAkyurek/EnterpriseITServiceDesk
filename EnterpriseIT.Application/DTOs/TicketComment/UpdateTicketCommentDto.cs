using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.Application.DTOs.TicketComment
{
    public class UpdateTicketCommentDto
    {
        [Required, StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}
