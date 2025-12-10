using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflow.Application.DTOs;

namespace Workflow.Application.Interfaces
{

    public interface IWorkflowTemplateService
    {
        Task<int> CreateTemplateAsync(CreateWorkflowDto dto);
        Task<WorkflowTemplateDto> GetByIdAsync(int id);
        Task<List<WorkflowTemplateDto>> GetAllAsync();
    }
}