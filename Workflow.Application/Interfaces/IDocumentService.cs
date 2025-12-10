using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflow.Application.DTOs;

namespace Workflow.Application.Interfaces
{
    public interface IDocumentService 
    {
       Task<DocumentDto> UploadAsync(CreateDocumentDto dto, string uploaderId);
       Task<List<DocumentDto>> ListAsync(string? UploaderId = null);
    }

}
