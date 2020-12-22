using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Geo.Core.Models
{
    public class Permission : IdentityRole<int>
    {
        public virtual ICollection<EmployeePermission> EmployeePermissions { get; set; }
    }
}
