using Geo.Core.Models;
using Geo.Data;
using Geo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Geo.Web.Controllers
{
    [RequireHttps]
    public class AccountController : Controller
    {
        private readonly UserManager<Employee> _userManager;
        private readonly SignInManager<Employee> _signInManager;
        private readonly UnitOfWork<Employee> _employee;
        private readonly UnitOfWork<Permission> _permission;
        private readonly UnitOfWork<EmployeePermission> _ePermission;

        public AccountController(UserManager<Employee> userManager, SignInManager<Employee> signInManager, 
            IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _employee = new UnitOfWork<Employee>(configuration, accessor);
            _permission = new UnitOfWork<Permission>(configuration, accessor);
            _ePermission = new UnitOfWork<EmployeePermission>(configuration, accessor);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index() =>
            View(_employee.Generic.Get(include: d => d
                .Include(s => s.EmployeePermissions)
                .ThenInclude(s => s.Permission)));

        [AllowAnonymous, HttpGet]
        public IActionResult Login(string returnUrl = null) =>
            View(new LoginViewModel { ReturnUrl = returnUrl });

        [AllowAnonymous, HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid && model != null)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    // проверяем, принадлежит ли URL приложению
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        return Redirect(model.ReturnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Неверный логин и/или пароль");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync().ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() =>
            View("AccessDenied");

        #region Добавить пользователя
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            ViewBag.RoleId =
                new SelectList(_permission.Generic.Get(), "Id", "NormalizedName");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterViewModel model, int RoleId, IFormFile Photo)
        {
            if (ModelState.IsValid && model != null)
            {
                var employee = new Employee { Email = model.Email, UserName = model.Email, PhoneNumber = model.Phone, Name = model.Name };
                if (Photo != null && employee != null)
                    employee.Photo = _employee.IFormFileToByte(Photo);

                var result = await _userManager.CreateAsync(employee, model.Password);

                var ePermission = new EmployeePermission() { RoleId = RoleId, UserId = employee.Id };
                _ePermission.Generic.Create(ePermission);
                _ePermission.Save();

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.RoleId =
                new SelectList(_permission.Generic.Get(), "Id", "NormalizedName");
            return View(model);
        }
        #endregion
        #region Редактировать пользователя
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var employee = _employee.Generic.GetById(i => i.Id == id, include: d => d
                .Include(s => s.EmployeePermissions)
                .ThenInclude(s => s.Permission))
                .FirstOrDefault();
            if (employee == null) { return NotFound(); }
            var permissions = _permission.Generic.Get();
            ViewBag.RoleId =
                new SelectList(permissions, "Id", "NormalizedName", employee.EmployeePermissions.FirstOrDefault().RoleId);

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(Employee Employee, int Id, int RoleId, IFormFile Photo)
        {
            if (Photo != null && Employee != null)
                Employee.Photo = _employee.IFormFileToByte(Photo);       // если есть фото пишим в БД

            var permission = _ePermission.Generic.GetById(d => d.UserId == Id).FirstOrDefault();
            if (permission != null)
            {
                _ePermission.Delete(permission.UserId, permission.RoleId);
                _ePermission.Save();
            }

            _ePermission.Generic.Create(new EmployeePermission() { RoleId = RoleId, UserId = Id });
            _ePermission.Save();
            var employee = _employee.Generic.GetById(i => i.Id == Id).FirstOrDefault();
            employee.PhoneNumber = Employee != null ? Employee.PhoneNumber : employee.PhoneNumber;
            if (Photo != null && Employee != null) employee.Photo = Employee.Photo;

            _employee.Update(employee);
            _employee.Save();
            return RedirectToAction("Index");
        }
        #endregion
        #region Удалить пользователя

        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int Id)
        {
            var employee = _employee.Generic.GetById(d => d.Id == Id).FirstOrDefault();
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int Id)
        {
            _employee.Delete(Id);
            _employee.Save();
            return RedirectToAction("Index");
        }
        #endregion
        #region Смена пароля
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var employee = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (employee == null)
                return NotFound();

            return View(new ChangePasswordViewModel { Id = employee.Id, Email = employee.Email, ShortName = employee.ShortName });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = await _userManager.FindByIdAsync(model.Id.ToString());
                if (employee != null)
                {
                    var _passwordValidator =
                        HttpContext.RequestServices.GetService(typeof(IPasswordValidator<Employee>)) as IPasswordValidator<Employee>;
                    var _passwordHasher =
                        HttpContext.RequestServices.GetService(typeof(IPasswordHasher<Employee>)) as IPasswordHasher<Employee>;

                    IdentityResult result =
                        await _passwordValidator.ValidateAsync(_userManager, employee, model.NewPassword);
                    if (result.Succeeded)
                    {
                        employee.PasswordHash = _passwordHasher.HashPassword(employee, model.NewPassword);
                        await _userManager.UpdateAsync(employee);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }
            }
            return View(model);
        }
        #endregion

    }
}
