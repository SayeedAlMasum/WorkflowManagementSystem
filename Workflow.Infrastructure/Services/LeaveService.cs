using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Domain.Entities;
using Workflow.Infrastructure.Persistence;

namespace Workflow.Infrastructure.Services
{

    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public LeaveService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<int> SubmitAsync(CreateLeaveRequestDto dto)
        {
            var leave = _mapper.Map<LeaveRequest>(dto);
            leave.CreatedDate = DateTime.UtcNow;

            _db.LeaveRequests.Add(leave);
            await _db.SaveChangesAsync();

            return leave.Id;
        }

        public async Task<LeaveRequestDto> GetAsync(int id)
        {
            var leave = await _db.LeaveRequests
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
                throw new KeyNotFoundException($"Leave request {id} not found");

            return _mapper.Map<LeaveRequestDto>(leave);
        }
    }
}