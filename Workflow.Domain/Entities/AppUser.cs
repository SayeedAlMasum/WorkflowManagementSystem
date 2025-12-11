using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Workflow.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; } // Nullable since it's optional
    }

}
