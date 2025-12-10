

using System.ComponentModel.DataAnnotations;

namespace Workflow.Application.DTOs
{
    public class DocumentDto
    {
        [Required]    
        public string FileName { get; set; }
        [Required]
        public string MimeType { get; set; }
        [Required]
        public Stream Content { get; set; } // file content }
        }
}
