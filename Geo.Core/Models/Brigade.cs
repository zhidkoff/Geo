using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Geo.Core.Models
{
    public class Brigade
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Required, Display(Name = "Наименование бригады")]
        public string Name { get; set; }

        public List<Employee> employees { get; set; }
    }
}
