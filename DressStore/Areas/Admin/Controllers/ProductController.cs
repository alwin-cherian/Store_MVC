﻿using DressStore.DataAccess.Data;
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

        public async Task<IActionResult> Index()
        {
            List<Product> objProductList = (await _wholeRepo.product.GetAllAsync(includeProperties:"category")).ToList();
            return View(objProductList);
        }

        public async Task<IActionResult> Create()
        {
            ProductViewModel productVM = new()
            {
                CategoryList = (await _wholeRepo.Category.GetAllAsync())
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
        public async Task<IActionResult> Create(ProductViewModel productViewModel , IFormFile? file)
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
                productViewModel.CategoryList = (await _wholeRepo.Category.GetAllAsync()).Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productViewModel);
            }

        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            ProductViewModel productVM = new()
            {
                CategoryList = (await _wholeRepo.Category.GetAllAsync()).Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                product = new Product()
            };

            productVM.product = await _wholeRepo.product.GetAsync(u => u.Id == id);
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
        

        #region APICALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Product> objProductList =(await _wholeRepo.product.GetAllAsync(includeProperties: "category")).ToList();
            return Json(new {data = objProductList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var productToBeDeleted = await _wholeRepo.product.GetAsync(u => u.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new { success = false , message = "Error while deleting" });
            }

            var oldImagePath =
                Path.Combine(_webHostEnvironment.WebRootPath,
                productToBeDeleted.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _wholeRepo.product.Remove(productToBeDeleted);
            _wholeRepo.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }



        #endregion
    }
}
