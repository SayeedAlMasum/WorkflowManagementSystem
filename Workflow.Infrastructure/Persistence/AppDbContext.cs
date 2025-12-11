using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Workflow.Domain.Entities;

namespace Workflow.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Workflow> Workflows { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<WorkflowInstanceStep> WorkflowInstanceSteps { get; set; }
    public DbSet<WorkflowInstanceHistory> WorkflowInstanceHistories { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Workflow configurations
        builder.Entity<Domain.Entities.Workflow>(entity =>
        {

            entity.HasMany(e => e.Steps)
                .WithOne(s => s.Workflow)
                .HasForeignKey(s => s.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkflowInstance configurations
        builder.Entity<WorkflowInstance>(entity =>
        {
            entity.HasOne(e => e.Workflow)
                .WithMany()
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Steps)
                .WithOne(s => s.WorkflowInstance)
                .HasForeignKey(s => s.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkflowInstanceStep configurations
        builder.Entity<WorkflowInstanceStep>(entity =>
        {
            entity.HasOne(e => e.WorkflowStep)
                .WithMany()
                .HasForeignKey(e => e.WorkflowStepId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CompletedByUser)
                .WithMany()
                .HasForeignKey(e => e.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // WorkflowInstanceHistory configurations
        builder.Entity<WorkflowInstanceHistory>(entity =>
        {
            entity.HasOne(h => h.WorkflowInstance)
                .WithMany()
                .HasForeignKey(h => h.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.WorkflowStep)
                .WithMany()
                .HasForeignKey(h => h.WorkflowStepId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
