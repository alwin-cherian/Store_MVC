using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using DressStore.Models.ViewModels;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace DressStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IWholeRepository _wholeRepo;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IWholeRepository wholeRepo)
        {
            _wholeRepo = wholeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList =await _wholeRepo.shoppingCart.GetAllAsync(u => u.ApplicationUserId == userId,
                includeProperties:"Product"),
                OrderHeader = new()
            };
            
            if (ShoppingCartVM.ShoppingCartList.Count() == 0)
            {
                return View("cartEmpty");
            }
            foreach(var cart in  ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPrice(cart);
                cart.EachProductPrice = (cart.Price * cart.Count);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }
            return View(ShoppingCartVM);
        }

        public async Task<IActionResult> plus(int cartId)
        {
            var cartFromDb = await _wholeRepo.shoppingCart.GetAsync(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _wholeRepo.shoppingCart.Update(cartFromDb);
            _wholeRepo.Save();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cartFromDb = await _wholeRepo.shoppingCart.GetAsync(u => u.Id == cartId);
            if(cartFromDb.Count <= 1)
            {
                //remove that from the cart
                _wholeRepo.shoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _wholeRepo.shoppingCart.Update(cartFromDb);
            }
            _wholeRepo.Save();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Remove(int cartId)
        {
            var cartFromDb =await _wholeRepo.shoppingCart.GetAsync(u => u.Id == cartId);
            _wholeRepo.shoppingCart.Remove(cartFromDb);
            _wholeRepo.Save();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Checkout()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList =await _wholeRepo.shoppingCart.GetAllAsync(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser =await _wholeRepo.applicationUser.GetAsync(u=>u.Id == userId);

            //To pass the phone number
            //ShoppingCartVM.OrderHeader.phoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPrice(cart);
                cart.EachProductPrice = (cart.Price * cart.Count);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Checkout")]
        public async Task<IActionResult> CheckoutPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList =await _wholeRepo.shoppingCart.GetAllAsync(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser =await _wholeRepo.applicationUser.GetAsync(u => u.Id == userId);


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPrice(cart);
                cart.EachProductPrice = (cart.Price * cart.Count);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            //capture payment
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;

            _wholeRepo.orderHeader.Add(ShoppingCartVM.OrderHeader);
            _wholeRepo.Save();

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Count = cart.Count,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id
                };
                _wholeRepo.orderDetail.Add(orderDetail);
                _wholeRepo.Save();
            }

            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                var soldQuantity = item.Count; // Get the quantity of the item sold
                item.Product.TotalQuantity -= soldQuantity; // Reduce the total quantity of the product
                _wholeRepo.shoppingCart.Update(item);
                _wholeRepo.Save(); 
            }

            var domain = "https://localhost:7143/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain+ $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain+"customer/cart/Index",
                
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach(var item in ShoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            _wholeRepo.orderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _wholeRepo.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);



            return RedirectToAction(nameof(OrderConfirmation), new {id = ShoppingCartVM.OrderHeader.Id});
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = await _wholeRepo.orderHeader.GetAsync(u => u.Id == id, includeProperties:"ApplicationUser");
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if(session.PaymentStatus.ToLower() == "paid")
            {
                _wholeRepo.orderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                _wholeRepo.orderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                _wholeRepo.Save();
            }

            OrderConfirmationVM orderDetails = new OrderConfirmationVM
            {
                OrderId = id,
                TransactionId = session.PaymentIntentId
            };



            List<ShoppingCart> shoppingCarts = ( await _wholeRepo.shoppingCart.
                GetAllAsync(u => u.ApplicationUserId == orderHeader.ApplicationUserId)).ToList();

            _wholeRepo.shoppingCart.RemoveRange(shoppingCarts);
            _wholeRepo.Save();

            return View(orderDetails);
        }
        public IActionResult cartEmpty()
        {
            return View();
        }
        private double GetPrice (ShoppingCart shoppingCart)
        {
            return shoppingCart.Product.Price;
        }

    }
}
