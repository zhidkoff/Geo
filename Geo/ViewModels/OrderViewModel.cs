using Geo.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Geo.Web.ViewModels
{
    public class OrderViewModel
    {
        [Display(Name = "Наименование заявки")]
        public string Name { get; set; }
        [Display(Name = "Количество заявок")]
        public int Count { get; set; }

        [Display(Name = "Количество заявок выполненных")]
        public int DoneCount { get; set; }

        [Display(Name = "Флаг выполненных заявок")]
        public bool Done { get; set; } = false;

        [Display(Name = "Дата начала")]
        public DateTime? DateOpen { get; set; }

        [Display(Name = "Дата окончания")]
        public DateTime? DateClose { get; set; }

        public TimeSpan TotalTime { get; set; }

        public int? BrigadeId { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }
}
