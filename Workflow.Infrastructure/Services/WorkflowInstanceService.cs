using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Domain.Entities;
using Workflow.Infrastructure.Persistence;

namespace Workflow.Infrastructure.Services;

public class WorkflowInstanceService : IWorkflowInstanceService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;

    public WorkflowInstanceService(AppDbContext db, IMapper mapper, UserManager<AppUser> userManager)
    {
        _db = db;
    _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<int> StartInstanceAsync(StartInstanceDto dto)
    {
        // Validate workflow template exists
        var workflow = await _db.Workflows
            .Include(w => w.Steps.OrderBy(s => s.Order))
       .FirstOrDefaultAsync(w => w.Id == dto.WorkflowId);

        if (workflow == null)
            throw new ArgumentException("Workflow template not found");

        if (!workflow.Steps.Any())
         throw new InvalidOperationException("Workflow has no steps defined");

      using var transaction = await _db.Database.BeginTransactionAsync();
 try
        {
            // Create workflow instance
            var instance = new WorkflowInstance
    {
 WorkflowId = workflow.Id,
   CreatedById = dto.CreatedById,
      CreatedDate = DateTime.UtcNow,
        Status = InstanceStatus.InProgress,
    CurrentStepId = "0", // Will update after creating steps
 Workflow = workflow
            };

    _db.WorkflowInstances.Add(instance);
  await _db.SaveChangesAsync();

            // Create instance steps from template
            var firstStep = workflow.Steps.OrderBy(s => s.Order).First();
     
          foreach (var templateStep in workflow.Steps)
            {
       var instanceStep = new WorkflowInstanceStep
         {
      WorkflowInstanceId = instance.Id,
              WorkflowStepId = templateStep.Id,
         StepName = templateStep.StepName,
Order = templateStep.Order,
        Status = templateStep.Order == firstStep.Order ? StepStatus.InProgress : StepStatus.Pending,
    WorkflowInstance = instance,
      WorkflowStep = templateStep
        };

   // Auto-assign first step to creator if role matches
              if (templateStep.Order == firstStep.Order)
          {
 var userRoles = await _userManager.GetRolesAsync(
         await _userManager.FindByIdAsync(dto.CreatedById) 
   ?? throw new InvalidOperationException("User not found"));
         
  if (string.IsNullOrEmpty(templateStep.RoleRequired) || 
             userRoles.Contains(templateStep.RoleRequired))
           {
   instanceStep.AssignedToUserId = dto.CreatedById;
     instanceStep.StartedDate = DateTime.UtcNow;
  }
     }

  _db.WorkflowInstanceSteps.Add(instanceStep);
            }

         await _db.SaveChangesAsync();

          // Update current step ID
         var firstInstanceStep = await _db.WorkflowInstanceSteps
              .Where(s => s.WorkflowInstanceId == instance.Id)
           .OrderBy(s => s.Order)
                .FirstAsync();
 
            instance.CurrentStepId = firstInstanceStep.Id.ToString();
   await _db.SaveChangesAsync();

       await transaction.CommitAsync();
    return instance.Id;
        }
        catch
        {
      await transaction.RollbackAsync();
   throw;
     }
    }

    public async Task<WorkflowInstanceDto> GetInstanceAsync(int id)
    {
  var instance = await _db.WorkflowInstances
            .Include(i => i.Workflow)
            .Include(i => i.Steps.OrderBy(s => s.Order))
           .ThenInclude(s => s.AssignedToUser)
 .Include(i => i.Steps)
         .ThenInclude(s => s.CompletedByUser)
         .FirstOrDefaultAsync(i => i.Id == id);

   if (instance == null)
            throw new KeyNotFoundException($"Workflow instance with ID {id} not found");

        var dto = new WorkflowInstanceDto
        {
   Id = instance.Id,
            WorkflowId = instance.WorkflowId,
   WorkflowName = instance.Workflow.Name,
  CurrentStepId = instance.CurrentStepId,
  CurrentStepName = instance.Steps.FirstOrDefault(s => s.Id.ToString() == instance.CurrentStepId)?.StepName ?? "N/A",
            CreatedById = instance.CreatedById,
            CreatedDate = instance.CreatedDate,
      Status = instance.Status,
            Steps = instance.Steps.Select(s => new WorkflowInstanceStepDto
   {
             Id = s.Id,
             StepName = s.StepName,
                Order = s.Order,
    Status = s.Status,
           AssignedToUserId = s.AssignedToUserId,
                AssignedToUserName = s.AssignedToUser?.FullName,
             StartedDate = s.StartedDate,
   CompletedDate = s.CompletedDate,
         CompletedByUserId = s.CompletedByUserId,
 CompletedByUserName = s.CompletedByUser?.FullName,
       Comments = s.Comments
            }).ToList()
        };

        return dto;
    }

    public async Task<ActionResultDto> ActOnInstanceAsync(int instanceId, InstanceActionDto action, string actingUserId)
    {
   // Load instance with workflow template and steps
        var instance = await _db.WorkflowInstances
            .Include(i => i.Workflow)
        .ThenInclude(w => w.Steps)
            .Include(i => i.Steps.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
  return new ActionResultDto { Success = false, Message = "Instance not found" };

  if (instance.Status != InstanceStatus.InProgress)
 return new ActionResultDto { Success = false, Message = "Instance is not active" };

        var currentStep = instance.Steps.FirstOrDefault(s => s.Id.ToString() == instance.CurrentStepId);
        if (currentStep == null)
       return new ActionResultDto { Success = false, Message = "Current step not found" };

        var templateStep = await _db.WorkflowSteps.FindAsync(currentStep.WorkflowStepId);
      if (templateStep == null)
            return new ActionResultDto { Success = false, Message = "Template step not found" };

        // Permission check
        if (!await CanUserActOnStep(actingUserId, templateStep))
      return new ActionResultDto { Success = false, Message = "User does not have permission for this step" };

      using var transaction = await _db.Database.BeginTransactionAsync();
        try
   {
     var actionLower = action.Action.ToLower();

            if (actionLower == "approve" || actionLower == "complete")
         {
                // Mark current step as completed
   currentStep.Status = StepStatus.Completed;
                currentStep.CompletedDate = DateTime.UtcNow;
         currentStep.CompletedByUserId = actingUserId;
             currentStep.Comments = action.Comments;

   // Find next step
    var orderedSteps = instance.Steps.OrderBy(s => s.Order).ToList();
  var currentIndex = orderedSteps.FindIndex(s => s.Id == currentStep.Id);

   if (currentIndex == orderedSteps.Count - 1)
        {
         // Last step -> complete workflow
        instance.Status = InstanceStatus.Completed;
                 instance.CurrentStepId = "0";
   
       await _db.SaveChangesAsync();
  await transaction.CommitAsync();

         return new ActionResultDto 
   { 
     Success = true, 
             Message = "Workflow completed successfully" 
            };
}
     else
       {
   // Move to next step
          var nextStep = orderedSteps[currentIndex + 1];
    nextStep.Status = StepStatus.InProgress;
        nextStep.StartedDate = DateTime.UtcNow;
     
         instance.CurrentStepId = nextStep.Id.ToString();

                    // Auto-assign if possible
           var nextTemplateStep = await _db.WorkflowSteps.FindAsync(nextStep.WorkflowStepId);
        // For now, leave unassigned - can add auto-assignment logic here

        await _db.SaveChangesAsync();
       await transaction.CommitAsync();

  return new ActionResultDto 
       { 
      Success = true, 
    Message = "Step completed, moved to next step",
        NextStepName = nextStep.StepName,
     AssignedToUserId = nextStep.AssignedToUserId
       };
     }
     }
            else if (actionLower == "reject")
{
        // Mark step as skipped and workflow as cancelled
     currentStep.Status = StepStatus.Skipped;
    currentStep.CompletedDate = DateTime.UtcNow;
   currentStep.CompletedByUserId = actingUserId;
      currentStep.Comments = action.Comments;

     instance.Status = InstanceStatus.Cancelled;
   instance.CurrentStepId = "0";

     await _db.SaveChangesAsync();
        await transaction.CommitAsync();

     return new ActionResultDto 
     { 
          Success = true, 
             Message = "Workflow rejected and cancelled" 
           };
          }
          else
    {
             return new ActionResultDto 
      { 
        Success = false, 
   Message = "Invalid action. Use 'approve', 'complete', or 'reject'" 
 };
     }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
  return new ActionResultDto 
 { 
             Success = false, 
    Message = $"Error processing action: {ex.Message}" 
     };
        }
    }

    public async Task<List<WorkflowInstanceDto>> GetMyInstancesAsync(string userId)
    {
        var instances = await _db.WorkflowInstances
            .Include(i => i.Workflow)
  .Include(i => i.Steps)
   .Where(i => i.CreatedById == userId || 
     i.Steps.Any(s => s.AssignedToUserId == userId))
   .OrderByDescending(i => i.CreatedDate)
      .ToListAsync();

        var dtos = new List<WorkflowInstanceDto>();
    
        foreach (var instance in instances)
        {
            var dto = new WorkflowInstanceDto
         {
       Id = instance.Id,
       WorkflowId = instance.WorkflowId,
    WorkflowName = instance.Workflow.Name,
    CurrentStepId = instance.CurrentStepId,
       CurrentStepName = instance.Steps.FirstOrDefault(s => s.Id.ToString() == instance.CurrentStepId)?.StepName ?? "N/A",
        CreatedById = instance.CreatedById,
                CreatedDate = instance.CreatedDate,
      Status = instance.Status
};
 
            dtos.Add(dto);
        }

        return dtos;
    }

    private async Task<bool> CanUserActOnStep(string userId, WorkflowStep step)
    {
      // If no role required, anyone can act
     if (string.IsNullOrEmpty(step.RoleRequired))
            return true;

   var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
     return false;

        var roles = await _userManager.GetRolesAsync(user);
        return roles.Contains(step.RoleRequired);
}
}
