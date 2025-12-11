using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Domain.Entities;

public class Document
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string FileName { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public required string Url { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string MimeType { get; set; }
    
  [Required]
    public required string UploaderId { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
