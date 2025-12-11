# Workflow Management System - ???????? ??????

## ? ?? ??????? ??????

### 1. Domain Layer (?? Entities ????)
- ? AppUser - Identity user
- ? Workflow - Template
- ? WorkflowStep - Template ?? steps
- ? WorkflowInstance - ????? workflow
- ? WorkflowInstanceStep - Instance ?? steps ??? status
- ? WorkflowInstanceHistory - Audit log (?? ??? ?? ?????)
- ? Document - File upload ?? ????
- ? LeaveRequest - Leave request ?? ????
- ? Enums: InstanceStatus, StepStatus

### 2. Application Layer (Business Logic)
- ? ?? DTOs ???? (30+ DTOs)
- ? ?? Service Interfaces:
  - IWorkflowService - Template management
  - IWorkflowInstanceService - Instance management + history
  - ITaskService - User tasks (filtering ? pagination ??)
  - IDocumentService - Document upload/list
  - ILeaveService - Leave request submit/get
  - IWorkflowHistoryService - Audit history
- ? AutoMapper configuration ?? mappings ??

### 3. Infrastructure Layer (Database ? Implementation)
- ? AppDbContext - ?? tables configure ???
  - Identity tables
  - Workflow tables
  - Document ? LeaveRequest tables
  - History table
  - ?? relationships ??? constraints
- ? ?? Services implement ???:
  - **WorkflowService** - Template CRUD
  - **WorkflowInstanceService** - ???? workflow engine:
    - ? StartInstanceAsync() - Instance ????, steps clone, first step assign
    - ? ActOnInstanceAsync() - Approve/Reject/Complete
    - ? ApproveAsync() - Explicit approve
    - ? RejectAsync() - Explicit reject
    - ? GetInstanceAsync() - Instance details
    - ? GetMyInstancesAsync() - User ?? instances
    - ? CanUserActOnStep() - Role-based permission check
    - ? **History tracking** - ?? actions ?? ???? history entry
  - **TaskService** - Task management:
    - ? Status filtering (Pending/InProgress/Completed/Rejected)
    - ? Pagination support
  - **DocumentService** - File upload ??? document management
  - **LeaveService** - Leave request management
  - **WorkflowHistoryService** - History retrieval
  - **StorageService** - Local file storage (IStorageService interface)

### 4. Key Features Implemented
- ? **Transaction Support** - ?? operations transaction ? wrapped
- ? **History/Audit Log** - ??????? action track ???
- ? **Role-based Security** - Step level permission check
- ? **Auto-assignment** - First step creator ?? assign (role match ????)
- ? **State Management** - Instance ? step status tracking
- ? **Error Handling** - Try-catch with rollback
- ? **Pagination** - Task list ? pagination
- ? **Filtering** - Status ???????? task filter

---

## ?? ?? ???? ??? - ???? API Layer

### ?????? ?? ???? ???:

#### 1. API Project Setup (??? ?? ????)
```bash
dotnet new webapi -n Workflow.Api
cd Workflow.Api
dotnet add reference ../Workflow.Infrastructure/Workflow.Infrastructure.csproj
dotnet add reference ../Workflow.Application/Workflow.Application.csproj
```

#### 2. Packages Add ???
```bash
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.11
dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.11
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

#### 3. Program.cs ? Dependency Injection Configure ???
```csharp
// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Services
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IWorkflowInstanceService, WorkflowInstanceService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IWorkflowHistoryService, WorkflowHistoryService>();

// Storage (upload folder path ???? ???)
var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
builder.Services.AddSingleton<IStorageService>(new LocalStorageService(uploadPath));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
```

#### 4. Controllers ???? ???

**?? WorkflowsController.cs** - Workflow Template Management
```csharp
[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _service;
    
    // POST /api/workflows - Create template
    // GET /api/workflows - List templates
    // GET /api/workflows/{id} - Get template
}
```

**?? WorkflowInstancesController.cs** - Instance Management
```csharp
[ApiController]
[Route("api/workflow-instances")]
public class WorkflowInstancesController : ControllerBase
{
    private readonly IWorkflowInstanceService _service;
    private readonly IWorkflowHistoryService _historyService;
    
    // POST /api/workflow-instances/start - Start instance
    // GET /api/workflow-instances/{id} - Instance details
    // POST /api/workflow-instances/{id}/act - General action
    // POST /api/workflow-instances/{id}/approve - Approve
    // POST /api/workflow-instances/{id}/reject - Reject
    // GET /api/workflow-instances/my - My instances
    // GET /api/workflow-instances/{id}/history - History
}
```

**?? TasksController.cs** - User Tasks
```csharp
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;
    
    // GET /api/tasks/my?status=InProgress&page=1&pageSize=20
    // POST /api/tasks/{taskId}/complete
}
```

**?? DocumentsController.cs** - Document Management
```csharp
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _docService;
    private readonly IWorkflowInstanceService _instanceService;
    
  // POST /api/documents/upload - Upload ??? workflow start
    // GET /api/documents - List documents
}
```

**?? LeaveController.cs** - Leave Requests
```csharp
[ApiController]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;
 private readonly IWorkflowInstanceService _instanceService;
    
    // POST /api/leave/requests - Submit ??? workflow start
    // GET /api/leave/requests/{id} - Leave details
}
```

#### 5. Database Migration Run ???
```bash
# Package Manager Console ?
Add-Migration InitialCreate
Update-Database

# ???? Terminal ?
dotnet ef migrations add InitialCreate --project Workflow.Infrastructure --startup-project Workflow.Api
dotnet ef database update --project Workflow.Infrastructure --startup-project Workflow.Api
```

#### 6. Roles Seed ???
???? `SeedData.cs` class ???? ???:
```csharp
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        string[] roles = { "Admin", "Manager", "HR", "Reviewer", "Employee" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
        {
         await roleManager.CreateAsync(new IdentityRole(role));
            }
 }
    }
}
```

Program.cs ? call ???:
```csharp
using (var scope = app.Services.CreateScope())
{
await SeedData.Initialize(scope.ServiceProvider);
}
```

---

## ?? Test ???? ???? Scenarios

### ?? Document Approval Test ???:

1. **Workflow Template Create ???:**
   ```json
   POST /api/workflows
   {
     "name": "Document Approval",
     "description": "3-step document approval",
     "createdById": "user-id",
     "steps": [
       { "stepName": "Reviewer 1", "order": 1, "roleRequired": "Reviewer" },
     { "stepName": "Reviewer 2", "order": 2, "roleRequired": "Reviewer" },
   { "stepName": "Final Approval", "order": 3, "roleRequired": "Manager" }
     ]
   }
   ```

2. **Document Upload ???:**
   ```
   POST /api/documents/upload
   (file upload + workflowId)
   ? Automatically starts instance
   ```

3. **Tasks ????:**
   ```
   GET /api/tasks/my
   ? Shows Reviewer 1 step
   ```

4. **Approve ???:**
   ```
   POST /api/tasks/{taskId}/complete
   { "action": "Approve", "comments": "Looks good" }
   ? Moves to Reviewer 2
   ```

5. **Continue ???** ?????? ?? Completed ???

### ??? Leave Request Test ???:

1. **Leave Workflow Template Create:**
   ```json
   {
     "name": "Leave Approval",
     "steps": [
       { "stepName": "Manager Approval", "order": 1, "roleRequired": "Manager" },
       { "stepName": "HR Approval", "order": 2, "roleRequired": "HR" }
     ]
   }
   ```

2. **Leave Submit ???:**
   ```json
   POST /api/leave/requests
   {
     "employeeId": "emp-123",
     "startDate": "2024-02-01",
     "endDate": "2024-02-05",
     "reason": "Family vacation",
     "leaveType": "Annual"
   }
   ```

3. **Manager Approves ? HR Approves ? Completed**

---

## ?? ??????

### ?? ??? (100% Complete):
- ? ?? Domain Entities
- ? ?? DTOs
- ? ?? Service Interfaces
- ? ?? Service Implementations
- ? Complete Workflow Engine with History
- ? Role-based Security
- ? File Upload Support
- ? Document Module
- ? Leave Module
- ? Task Management with Filtering
- ? Database Configuration

### ?? ???? (???? Controllers):
- ?? 5-6?? Controller ????? ???
- ?? Migration run ???? ???
- ?? Role seed ???? ???
- ?? Test ???? ???

**Build successful! Backend logic 100% ready! ???? API endpoints expose ???? ???!** ??

---

## ?? ??????? ??? (Priority ????????):

### High Priority:
1. Controllers ???? ??? (1-2 ?????)
2. Migration run ??? (5 ?????)
3. Roles seed ??? (10 ?????)
4. Swagger ????? test ??? (30 ?????)

### Medium Priority:
- Validation add ??? (FluentValidation)
- Error handling middleware
- Authentication endpoints (Login/Register)

### Low Priority:
- Logging (Serilog)
- Email notifications
- Background jobs (Hangfire)
- Caching

---

**?? backend code complete! ??? ???? API wiring ????!** ??
