using Geo.Core.Models;
using Geo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Geo.Web.Controllers
{
    [RequireHttps]
    [Authorize(Roles = "Admin,Secretary,Manager")]
    public class BrigadeController : Controller
    {
        private readonly UnitOfWork<Brigade> _brigade;
        private readonly UnitOfWork<Employee> _employee;
        public BrigadeController(IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _brigade = new UnitOfWork<Brigade>(configuration, accessor);
            _employee = new UnitOfWork<Employee>(configuration, accessor);
        }
        public ActionResult Index() =>
            View(_brigade.Generic.Get());

        [HttpGet]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Brigade brigade, int EmployeeId)
        {
            try
            {
                _brigade.Generic.Create(brigade);
                _brigade.Save();
                return RedirectToAction(nameof(Edit), new { brigade.Id });
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Edit(int Id)
        {
            ViewBag.Brigades = new SelectList(
               _employee.Generic.Get(orderBy: d => d.OrderBy(s => s.Name)), "Id", "ShortName");
            return View(_brigade.Generic.GetById(d => d.Id == Id,
                include: d => d.Include(s => s.employees)).FirstOrDefault());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Brigade brigade)
        {
            try
            {
                _brigade.Update(brigade);
                _brigade.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Delete(int Id) =>
            View(_brigade.Generic.GetById(d => d.Id == Id).FirstOrDefault());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int Id)
        {
            try
            {
                _brigade.Delete(Id);
                _brigade.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public IActionResult AddAjax(int Id, int EmployeeId)
        {
            var employee = _employee.Generic.GetById(d => d.Id == EmployeeId).FirstOrDefault();
            employee.BrigadeId = Id;
            _employee.Update(employee);
            _employee.Save();
            return getBrigade(Id);
        }

        public IActionResult DelAjax(int Id)
        {
            var employee = _employee.Generic.GetById(d => d.Id == Id).FirstOrDefault();
            var brigadeId = employee.BrigadeId;
            employee.BrigadeId = null;
            _employee.Update(employee);
            _employee.Save();
            return getBrigade((int)brigadeId);
        }

        private IActionResult getBrigade(int Id) =>
            PartialView("Components\\_Brigade",
                _employee.Generic.Get(d => d.BrigadeId == Id));
    }
}
