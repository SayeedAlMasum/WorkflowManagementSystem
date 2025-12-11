using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Domain.Entities;

public class LeaveRequest
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public required string EmployeeId { get; set; }
    
    public DateTime StartDate { get; set; }
    
 public DateTime EndDate { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public required string Reason { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string LeaveType { get; set; } // e.g., "Annual", "Sick"
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
