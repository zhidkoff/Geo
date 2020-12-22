using Microsoft.AspNetCore.Identity;

namespace Geo.Core.Models
{
    public class EmployeePermission : IdentityUserRole<int>
    {
        public virtual Employee Employee { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
