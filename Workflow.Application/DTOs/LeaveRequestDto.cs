using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Application.DTOs
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        [Required]
        public string EmployeeId { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        public string Reason { get; set; }
        [Required]
        public string LeaveType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
