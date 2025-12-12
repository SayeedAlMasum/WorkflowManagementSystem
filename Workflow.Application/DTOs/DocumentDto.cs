using System.ComponentModel.DataAnnotations;

namespace Workflow.Application.DTOs
{
        public class DocumentDto
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

            public DateTime CreatedDate { get; set; }
        }
}
