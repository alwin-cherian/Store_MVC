using DressStore.DataAccess.Data;
using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using DressStore.Models.ViewModels;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DressStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContest _db;
        private readonly IWholeRepository _wholeRepository;
        public UserController(ApplicationDbContest db, IWholeRepository wholeRepository)
        {
            _db = db;
            _wholeRepository = wholeRepository;
        }

        public IActionResult Index()
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.ToList();
            return View(objUserList);
        }

        public async Task<IActionResult> Lock(string id)
        {

            var objFromDb = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);

            if (objFromDb == null)
            {
                return View();
            }
            if(objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to UnLock User
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Unlock(string id)
        {

            var objFromDb = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);

            if (objFromDb == null)
            {
                return View();
            }
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to UnLock User
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChangesAsync();
            return RedirectToAction("Index");

        }


        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserLists = _db.ApplicationUsers.ToList();
            return Json(new {data = objUserLists });
        }

        #endregion
    }
}
