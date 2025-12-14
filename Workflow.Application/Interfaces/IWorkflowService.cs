using Workflow.Application.DTOs;


namespace Workflow.Application.Interfaces
{

    public interface IWorkflowService
    {
        Task<int> CreateWorkflowAsync(CreateWorkflowDto dto);
        Task<List<WorkflowDto>> GetAllAsync();
        Task<WorkflowDto> GetByIdAsync(int id);
        Task UpdateAsync(int id, CreateWorkflowDto dto);
        Task DeleteAsync(int id);
    }
}