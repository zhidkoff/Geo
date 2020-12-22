using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Geo.Core.Models
{
    public class Order
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Display(Name = "Дата/время регистрации заявки")]
        public DateTime DateOpen { get; set; }
        [Required, Display(Name = "Краткое наименование работ")]
        public string Name { get; set; }

        [Required, Display(Name = "Описание работ")]
        public string Description { get; set; }


        [HiddenInput(DisplayValue = false)]
        public int BrigadeId { get; set; }
        public Brigade Brigade { get; set; }

        [Display(Name = "Дата/время закрытия заявки")]
        public DateTime? DateClose { get; set; }

        [Display(Name = "Примечание бригады")]
        public string Memo { get; set; }
    }
}
