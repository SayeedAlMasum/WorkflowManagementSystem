using Microsoft.EntityFrameworkCore;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Infrastructure.Persistence;

namespace Workflow.Infrastructure.Services
{
    public class WorkflowHistoryService : IWorkflowHistoryService
    {
        private readonly AppDbContext _db;

        public WorkflowHistoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<WorkflowHistoryDto>> GetHistoryAsync(int instanceId)
        {
            var items = await _db.WorkflowInstanceHistories
       .Include(h => h.WorkflowStep)
       .Where(h => h.WorkflowInstanceId == instanceId)
    .OrderBy(h => h.Timestamp)
     .ToListAsync();

            return items.Select(h => new WorkflowHistoryDto
            {
                Id = h.Id,
                Action = h.Action,
                ActionTakenById = h.ActionTakenById,
                Comment = h.Comment,
                Timestamp = h.Timestamp,
                StepId = h.WorkflowStepId,
                StepName = h.WorkflowStep?.StepName
            }).ToList();
        }
    }
}