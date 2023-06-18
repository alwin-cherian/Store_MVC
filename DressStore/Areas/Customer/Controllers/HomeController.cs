using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace DressStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWholeRepository _wholeRepo;

        public HomeController(ILogger<HomeController> logger , IWholeRepository unitOfWork)
        {
            _logger = logger;
            _wholeRepo = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> productList =await _wholeRepo.product.GetAllAsync(includeProperties: "category");
            return View(productList);
        }

        public async Task<IActionResult> Shop()
        {
            IEnumerable<Product> productList = await _wholeRepo.product.GetAllAsync(includeProperties: "category");
            return View(productList);
        }

        public async Task<IActionResult> Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = await _wholeRepo.product.GetAsync(u=> u.Id == productId, includeProperties: "category"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = await _wholeRepo.shoppingCart.GetAsync(u=> u.ApplicationUserId == userId && 
            u.ProductId == shoppingCart.ProductId);
            
            if(cartFromDb != null)
            {
                //Shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                _wholeRepo.shoppingCart.Update(cartFromDb);
            }
            else
            {
                //add cart record
                _wholeRepo.shoppingCart.Add(shoppingCart);
            }
            TempData["success"] = "Cart updated Sccessfully";
            _wholeRepo.Save();

            return RedirectToAction("Index", "Cart");
        }

        public IActionResult cartEmpty()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}