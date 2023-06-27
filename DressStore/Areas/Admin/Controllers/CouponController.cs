using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace DressStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CouponController : Controller
    {
        private readonly IWholeRepository _repo;
        public CouponController(IWholeRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<Coupon> couponsList = (await _repo.coupon.GetAllAsync()).ToList();
            return View(couponsList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Coupon obj)
        {
            var coupenExist = await _repo.coupon.GetAsync(u=>u.CouponName == obj.CouponName);
            if(coupenExist == null)
            {
                if(ModelState.IsValid)
                {
                    _repo.coupon.Add(obj);
                    _repo.Save();
                    TempData["success"] = "Coupon created successfully";
                    return RedirectToAction("Index");
                }
            }

            return View();
        }

        public async Task<ActionResult> Update(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }

            Coupon? couponFromDb = await _repo.coupon.GetAsync(u=> u.Id == id);

            if (couponFromDb == null)
                return NotFound();
            return View(couponFromDb);
        }

        [HttpPost]
        public async Task<ActionResult> Update(Coupon? obj)
        {
            if (ModelState.IsValid)
            {
                _repo.coupon.Update(obj);
                _repo.Save();
                TempData["success"] = "Coupon Updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


        #region APICALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Coupon> objCouponList = (await _repo.coupon.GetAllAsync()).ToList();
            return Json(new { data = objCouponList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var couponToBeDeleted = await _repo.coupon.GetAsync(u => u.Id == id);
            if (couponToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _repo.coupon.Remove(couponToBeDeleted);
            _repo.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
