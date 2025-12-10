using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Application.DTOs
{
    public class WorkflowHistoryDto 
    {
        public int Id { get; set; }
        [Required]
        public string Action { get; set; }
        [Required]
        public string ActionTakenById { get; set; } 
        public string? Comment { get; set; } 
        public DateTime Timestamp { get; set; } 
        public int? StepId { get; set; } 
        public string? StepName { get; set; }
    }

}
