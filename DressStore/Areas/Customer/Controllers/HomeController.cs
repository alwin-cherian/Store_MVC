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

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _wholeRepo.product.GetAll(includeProperties: "category");
            return View(productList);
        }

        public IActionResult Shop()
        {
            IEnumerable<Product> productList = _wholeRepo.product.GetAll(includeProperties: "category");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _wholeRepo.product.Get(u=> u.Id == productId, includeProperties: "category"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _wholeRepo.shoppingCart.Get(u=> u.ApplicationUserId == userId && 
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

            return RedirectToAction(nameof(Index));
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