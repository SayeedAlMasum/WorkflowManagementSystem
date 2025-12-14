using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflow.Application.DTOs;

namespace Workflow.Application.Interfaces
{

    public interface ILeaveService
    {
        Task<int> SubmitAsync(CreateLeaveRequestDto dto);
        Task<LeaveRequestDto> GetAsync(int id);
        Task<List<LeaveRequestDto>> GetMyLeaveRequestsAsync(string employeeId);
        Task UpdateAsync(int id, CreateLeaveRequestDto dto);
        Task DeleteAsync(int id);
    }
}