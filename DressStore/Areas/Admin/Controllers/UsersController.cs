using DressStore.DataAccess.Data;
using DressStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace DressStore.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            List<ApplicationUser> CustomerList = _userManager.Users.ToList();
            return View(CustomerList);
        }

        public IActionResult Block(string id)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            user.IsBlocked = true;
            _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        public IActionResult UnBlock(string id)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            user.IsBlocked = false;
            _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }
    }
}
