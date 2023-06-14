using DressStore.DataAccess.Data;
using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using DressStore.Models.ViewModels;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DressStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IWholeRepository _wholeRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IWholeRepository wholeRepo, IWebHostEnvironment webHostEnvironment)
        {
            _wholeRepo = wholeRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _wholeRepo.product.GetAll(includeProperties:"category").ToList();
            return View(objProductList);
        }

        //public IActionResult Upsert(int? id)
        //{

        //    ProductViewModel productVM = new()
        //    {
        //        CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
        //        {
        //            Text = u.Name,
        //            Value = u.Id.ToString()
        //        }),
        //        product = new Product()
        //    };
        //    if(id == null || id ==0)
        //    {
        //        return View(productVM);
        //    }
        //    else
        //    {
        //        //update
        //        productVM.product = _unitOfWork.product.Get(u => u.Id == id);
        //        return View(productVM); 
        //    }
            
        //}

        //[HttpPost]
        //public IActionResult Upsert(ProductViewModel productViewModel, IFormFile? file)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        string wwwRootPath = _webHostEnvironment.WebRootPath;
        //        if(file != null)
        //        {
        //            string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        //            string productPath = Path.Combine(wwwRootPath, @"images\products");

        //            if (!string.IsNullOrEmpty(productViewModel.product.ImageUrl))
        //            {
        //                //delete the old photo
        //                var oldImagePath = 
        //                    Path.Combine(wwwRootPath, productViewModel.product.ImageUrl.TrimStart('\\'));

        //                if(System.IO.File.Exists(oldImagePath))
        //                    System.IO.File.Delete(oldImagePath);
        //            }

        //            using (var fileStream = new FileStream(Path.Combine(productPath , filename), FileMode.Create))
        //            {
        //                file.CopyTo(fileStream);
        //            }
        //            productViewModel.product.ImageUrl = @"\images\products" + filename;
        //        }

        //        if(productViewModel.product.Id == 0)
        //        {
        //            _unitOfWork.product.Add(productViewModel.product);
        //        }
        //        else
        //        {
        //            _unitOfWork.product.Update(productViewModel.product);
        //        }
                
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product created successfully";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        productViewModel.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
        //        {
        //            Text = u.Name,
        //            Value = u.Id.ToString()
        //        });
        //        return View(productViewModel);
        //    }
            
        //}


        public IActionResult Create()
        {
            ProductViewModel productVM = new()
            {
                CategoryList = _wholeRepo.Category.GetAll()
                .Where(u => u.IsAvailable) // filter categories that are available
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                product = new Product()
            };
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Create(ProductViewModel productViewModel , IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\products");


                    using (var fileStream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productViewModel.product.ImageUrl = @"\images\products\" + filename;
                }

                _wholeRepo.product.Add(productViewModel.product);
                _wholeRepo.Save();
                TempData["success"] = "product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productViewModel.CategoryList = _wholeRepo.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productViewModel);
            }

        }

        public IActionResult Update(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            ProductViewModel productVM = new()
            {
                CategoryList = _wholeRepo.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                product = new Product()
            };

            productVM.product = _wholeRepo.product.Get(u => u.Id == id);
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Update(ProductViewModel productViewModel, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\products");

                    if (!string.IsNullOrEmpty(productViewModel.product.ImageUrl))
                    {
                        //delete the old photo
                        var oldImagePath =
                            Path.Combine(wwwRootPath, productViewModel.product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productViewModel.product.ImageUrl = @"\images\products\" + filename;
                }

                _wholeRepo.product.Update(productViewModel.product);
                _wholeRepo.Save();
                TempData["success"] = "Product Updated successfully";
                return RedirectToAction("Index");
            }
            return View();

        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            Product? categoryFromDb = _wholeRepo.product.Get(u => u.Id == id);

            if (categoryFromDb == null)
                return NotFound();
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _wholeRepo.product.Get(u => u.Id == id);
            if (obj == null)
                return NotFound();

            _wholeRepo.product.Remove(obj);
            _wholeRepo.Save();
            TempData["success"] = "Product Deleted successfully";
            return RedirectToAction("Index");

        }

        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _wholeRepo.product.GetAll(includeProperties: "category").ToList();
            return Json(new {data = objProductList });
        }

        #endregion
    }
}
