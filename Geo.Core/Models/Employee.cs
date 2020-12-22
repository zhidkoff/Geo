using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Geo.Core.Models
{
    public class Employee : IdentityUser<int>
    {
        [Required]
        public string Name { get; set; }
        public byte[] Photo { get; set; }

        public virtual ICollection<IdentityUserClaim<int>> Claims { get; set; }
        public virtual ICollection<IdentityUserLogin<int>> Logins { get; set; }
        public virtual ICollection<IdentityUserToken<int>> Tokens { get; set; }
        public virtual ICollection<EmployeePermission> EmployeePermissions { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int? BrigadeId { get; set; }
        public Brigade Brigade { get; set; }

        [NotMapped]
        public string ShortName
        {
            get
            {
                string[] str = Name?.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (str?.Length == 3) return string.Format(CultureInfo.CurrentCulture, "{0} {1}. {2}.", str[0], str[1][0], str[2][0]);
                if (str?.Length == 2) return string.Format(CultureInfo.CurrentCulture, "{0} {1}.", str[0], str[1][0]);
                return Name;
            }
        }
    }
}
