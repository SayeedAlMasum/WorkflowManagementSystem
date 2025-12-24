using System.Collections.Generic;
using Workflow.Domain.Entities;

namespace Workflow.Application.Interfaces;

public interface IJwtService
{
 string GenerateToken(AppUser user, IList<string> roles);
}
