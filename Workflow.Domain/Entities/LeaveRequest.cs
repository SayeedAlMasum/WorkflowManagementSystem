using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Domain.Entities
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        public string Reason { get; set; }
        [Required]
        public string LeaveType { get; set; } // e.g., "Annual", "Sick"
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
