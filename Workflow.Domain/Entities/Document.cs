using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public string MimeType { get; set; }
        [Required]
        public string UploaderId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
