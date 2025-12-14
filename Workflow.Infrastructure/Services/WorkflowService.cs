using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Infrastructure.Persistence;

namespace Workflow.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public WorkflowService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<int> CreateWorkflowAsync(CreateWorkflowDto dto)
    {
        var workflow = _mapper.Map<Domain.Entities.Workflow>(dto);
   
        _db.Workflows.Add(workflow);
        await _db.SaveChangesAsync();
        
        return workflow.Id;
    }

    public async Task<List<WorkflowDto>> GetAllAsync()
    {
        var workflows = await _db.Workflows
          .Include(w => w.Steps)
            .ToListAsync();
    
        return _mapper.Map<List<WorkflowDto>>(workflows);
    }

    public async Task<WorkflowDto> GetByIdAsync(int id)
    {
 var workflow = await _db.Workflows
         .Include(w => w.Steps)
         .FirstOrDefaultAsync(w => w.Id == id);
  
   if (workflow == null)
      throw new KeyNotFoundException($"Workflow with ID {id} not found");
      
      return _mapper.Map<WorkflowDto>(workflow);
    }

    public async Task UpdateAsync(int id, CreateWorkflowDto dto)
    {
        var workflow = await _db.Workflows
       .Include(w => w.Steps)
          .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
      throw new KeyNotFoundException($"Workflow with ID {id} not found");

      // Update basic properties
   workflow.Name = dto.Name;
        workflow.Description = dto.Description;

        // Remove existing steps
        _db.WorkflowSteps.RemoveRange(workflow.Steps);

        // Add new steps
 workflow.Steps = _mapper.Map<List<Domain.Entities.WorkflowStep>>(dto.Steps);
      foreach (var step in workflow.Steps)
        {
    step.WorkflowId = workflow.Id;
        }

   await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var workflow = await _db.Workflows
            .Include(w => w.Steps)
    .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
          throw new KeyNotFoundException($"Workflow with ID {id} not found");

        // Check if workflow has any instances
        var hasInstances = await _db.WorkflowInstances
         .AnyAsync(i => i.WorkflowId == id);

   if (hasInstances)
            throw new InvalidOperationException("Cannot delete workflow with existing instances");

        _db.Workflows.Remove(workflow);
        await _db.SaveChangesAsync();
    }
}
