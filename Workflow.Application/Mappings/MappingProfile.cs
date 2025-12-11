using AutoMapper;
using Workflow.Application.DTOs;
using Workflow.Domain.Entities;
using WorkflowEntity = Workflow.Domain.Entities.Workflow;

namespace Workflow.Application.Mappings
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Workflow Template mappings
            CreateMap<WorkflowEntity, WorkflowDto>();
            CreateMap<WorkflowEntity, WorkflowTemplateDto>();
            CreateMap<WorkflowStep, WorkflowStepDto>();
            CreateMap<CreateWorkflowDto, WorkflowEntity>();
            CreateMap<CreateWorkflowStepDto, WorkflowStep>();

            
            
            // Workflow Instance mappings
            CreateMap<WorkflowInstance, WorkflowInstanceDto>()
                .ForMember(dest => dest.WorkflowName, opt => opt.MapFrom(src => src.Workflow.Name))
                .ForMember(dest => dest.CurrentStepName, opt => opt.Ignore()); // Handled manually

            CreateMap<WorkflowInstanceStep, WorkflowInstanceStepDto>()
                .ForMember(dest => dest.AssignedToUserName, opt => opt.MapFrom(src => src.AssignedToUser != null ? src.AssignedToUser.FullName : null))
                .ForMember(dest => dest.CompletedByUserName, opt => opt.MapFrom(src => src.CompletedByUser != null ? src.CompletedByUser.FullName : null));

            // Document mappings
            CreateMap<Document, DocumentDto>();
            CreateMap<CreateDocumentDto, Document>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UploaderId, opt => opt.Ignore())
                .ForMember(dest => dest.Url, opt => opt.Ignore());

            // Leave Request mappings
            CreateMap<CreateLeaveRequestDto, LeaveRequest>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
            CreateMap<LeaveRequest, LeaveRequestDto>();
        }
    }
}