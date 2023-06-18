using DressStore.DataAccess.Data;
using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DressStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IWholeRepository _repo;

        public CategoryController(IWholeRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> objCategoryList =(await _repo.Category.GetAllAsync()).ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name and Display order cannot be Same ");
            }

            if (ModelState.IsValid)
            {
                obj.IsAvailable = true;
                _repo.Category.Add(obj);
                _repo.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            Category? categoryFromDb = await _repo.Category.GetAsync(u => u.Id == id);

            if (categoryFromDb == null)
                return NotFound();
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {

            if (ModelState.IsValid)
            {
                _repo.Category.Update(obj);
                _repo.Save();
                TempData["success"] = "Category Updated successfully";
                return RedirectToAction("Index");
            }
            return View();

        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            Category? categoryFromDb = await _repo.Category.GetAsync(u => u.Id == id);

            if (categoryFromDb == null)
                return NotFound();
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            Category? obj = await _repo.Category.GetAsync(u => u.Id == id);
            if (obj == null)
                return NotFound();

            _repo.Category.Remove(obj);
            _repo.Save();
            TempData["success"] = "Category Deleted successfully";
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Available(int? id)
        {
            Category? obj = await _repo.Category.GetAsync(u => u.Id == id);
            obj.IsAvailable = true;
            _repo.Category.Update(obj);
            _repo.Save();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UnAvailable(int? id)
        {
            Category? obj =await  _repo.Category.GetAsync(u => u.Id == id);
            obj.IsAvailable = false;
            _repo.Category.Update(obj);
            _repo.Save();
            return RedirectToAction("Index");
        }
    }
}
