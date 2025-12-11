# ?? Database Migration Guide - Bengali

## ? Configuration ???????!

### 1. Connection String (appsettings.json)
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=DESKTOP-EG34HUK;Database=WorkflowManagementDb;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
}
```

**????? SQL Server Details:**
- Server Name: `DESKTOP-EG34HUK`
- Authentication: Windows Authentication (Integrated Security)
- Database: `WorkflowManagementDb` (auto-create ??? migration ?)

### 2. Program.cs Configuration ?
- ? DbContext registered with retry logic
- ? Identity configured with AppUser
- ? AutoMapper registered (v12.0.1)
- ? All Services registered
- ? Storage Service configured
- ? CORS enabled
- ? Swagger configured
- ? Role seeding enabled

---

## ?? Migration Run ???? Steps

### Option 1: Package Manager Console (Visual Studio)

#### Step 1: Migration ???? ???
```powershell
Add-Migration InitialCreate -Project Workflow.Infrastructure -StartupProject Workflow.Api
```

#### Step 2: Database Update ???
```powershell
Update-Database -Project Workflow.Infrastructure -StartupProject Workflow.Api
```

---

### Option 2: Command Line (Terminal)

#### Step 1: Migration ???? ???
```bash
cd C:\Users\User\source\repos\WorkflowManagementSystem
dotnet ef migrations add InitialCreate --project Workflow.Infrastructure --startup-project Workflow.Api
```

#### Step 2: Database Update ???
```bash
dotnet ef database update --project Workflow.Infrastructure --startup-project Workflow.Api
```

---

## ?? Migration ??? ??? ?? ???? ???:

### Database: `WorkflowManagementDb`

#### Tables ???? ???:
1. **AspNetUsers** - Identity users (AppUser)
2. **AspNetRoles** - Roles (Admin, Manager, HR, Reviewer, Employee)
3. **AspNetUserRoles** - User-Role mapping
4. **AspNetUserClaims**, **AspNetRoleClaims**, etc. - Identity tables
5. **Workflows** - Workflow templates
6. **WorkflowSteps** - Template steps
7. **WorkflowInstances** - Running workflows
8. **WorkflowInstanceSteps** - Instance step tracking
9. **WorkflowInstanceHistories** - Audit log
10. **Documents** - File metadata
11. **LeaveRequests** - Leave request data

---

## ?? Possible Issues & Solutions

### Issue 1: "Unable to connect to SQL Server"
**Solution:**
- SQL Server ???? ??? ???? check ???
- SQL Server Configuration Manager ???? SQL Server service start ???

### Issue 2: "Login failed for user"
**Solution:**
- Windows Authentication use ???? ??????? ???
- ????? Windows user-?? SQL Server ? access ??? ???? check ???

---

**??? ???? migration run ???? ????!** ??
