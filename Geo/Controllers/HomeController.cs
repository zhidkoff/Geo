using Geo.Core.Models;
using Geo.Data;
using Geo.Data.Infrastructure;
using Geo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Geo.Web.Controllers
{
    [Authorize]
    public class UserMain : ViewComponent
    {
        private readonly UnitOfWork<Employee> _employee;
        public UserMain(IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _employee = new UnitOfWork<Employee>(configuration, accessor);
        }
        public IViewComponentResult Invoke()
        {
            return View("_User",
                _employee.Generic.GetById(i => i.Id == ClaimsPrincipalExtensions.GetUserId(UserClaimsPrincipal)).FirstOrDefault());
        }
    }

    [Authorize]
    [RequireHttps]
    public class HomeController : Controller
    {
        private readonly UnitOfWork<Order> _order;
        private readonly UnitOfWork<Brigade> _brigade;
        private readonly UnitOfWork<Employee> _employee;
        public HomeController(IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _employee = new UnitOfWork<Employee>(configuration, accessor);
            _order = new UnitOfWork<Order>(configuration, accessor);
            _brigade = new UnitOfWork<Brigade>(configuration, accessor);
        }

        public ActionResult Index()
        {
            if (!User.IsInRole("Admin,Manager"))
                ViewBag.Brigade = new SelectList(_brigade.Generic.Get(), "Id", "Name");
            return View(getOrder(new OrderViewModel()));
        }

        [HttpPost]
        public IActionResult StatisticAjax(OrderViewModel model)
        {
            ViewBag.Brigade = new SelectList(_brigade.Generic.Get(), "Id", "Name", model.BrigadeId ?? 0);
            return PartialView("Components\\_Statistic", getOrder(model));
        }

        private OrderViewModel getOrder(OrderViewModel model)
        {
            model.Orders = _order.Generic.Get(include: d => d
                .Include(s => s.Brigade).ThenInclude(s => s.employees));
            if (!User.IsInRole("Master"))
                model.Orders = _order.Generic.Get(include: d => d.Include(s => s.Brigade), filterAttribute: "")
                 .OrderBy(d => d.DateOpen);

            if (model.Done)
                model.Orders = model.Orders.Where(d => d.DateClose != null);
            if (model.BrigadeId != null)
                model.Orders = model.Orders.Where(d => d.BrigadeId == model.BrigadeId);
            if (model.DateOpen != null)
            {
                if (model.DateClose != null)
                    model.Orders = model.Orders.Where(d => d.DateOpen.Date >= model.DateOpen?.Date
                        && d.DateOpen.Date <= model.DateClose?.Date);
                else
                    model.Orders = model.Orders.Where(d => d.DateOpen.Date >= model.DateOpen?.Date);
            }
            else
            {
                if (model.DateClose != null)
                    model.Orders = model.Orders.Where(d => d.DateOpen.Date <= model.DateClose?.Date);
            }

            model.Count = model.Orders.Count();
            model.DoneCount = model.Orders.Where(d => d.DateClose != null).Count();
            model.TotalTime = new TimeSpan(model.Orders.Where(d => d.DateClose != null)
                .Sum(r => (long)r.DateClose?.Ticks - r.DateOpen.Ticks));

            return model;
        }

        public IActionResult Details(int Id) =>
           PartialView("Components\\_Details", 
               _order.Generic.GetById(d => d.Id == Id, 
                   include: d => d.Include(d => d.Brigade).ThenInclude(d => d.employees))
               .IgnoreQueryFilters()
               .FirstOrDefault());

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Secretary")]
        public ActionResult Create()
        {
            ViewBag.Brigade = new SelectList(_brigade.Generic.Get(), "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Secretary")]
        public ActionResult Create(Order order, int BrigadeId)
        {
            try
            {
                order.DateOpen = DateTime.Now;
                order.BrigadeId = BrigadeId;
                _order.Generic.Create(order);
                _order.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Secretary,Master")]
        public ActionResult Edit(int id)
        {
            var model = _order.Generic.GetById(d => d.Id == id).IgnoreQueryFilters().FirstOrDefault();
            ViewBag.Brigade = new SelectList(_brigade.Generic.Get(), "Id", "Name", model?.BrigadeId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Secretary,Master")]
        public ActionResult Edit(Order order, int BrigadeId)
        {
            try
            {
                var model = _order.Generic.GetById(d => d.Id == order.Id).IgnoreQueryFilters().FirstOrDefault();
                if (User.IsInRole("Master"))
                {
                    model.DateClose = DateTime.Now;
                    model.Memo = order.Memo;
                }
                else
                {
                    model.BrigadeId = BrigadeId;
                    model.Description = order.Description;
                    model.Name = order.Name;
                }
                _order.Update(model);
                _order.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Secretary")]
        public ActionResult Delete(int id) =>
            View(_order.Generic.GetById(d => d.Id == id).IgnoreQueryFilters().FirstOrDefault());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin,Manager,Secretary")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                _order.Delete(id);
                _order.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [AllowAnonymous]
        public IActionResult Error(string code) => View("Error", code);
    }
}
