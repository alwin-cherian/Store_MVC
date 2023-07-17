using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using DressStore.Models.ViewModels;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
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
                OrderHeader = new(),
            };

            ShoppingCartVM.ApplicationUser = await _wholeRepo.applicationUser.GetAsync(u => u.Id == userId);
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

            if (!string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.CouponCode))
            {
                var couponResponse = await CouponCheckout(ShoppingCartVM.OrderHeader.CouponCode, (int)ShoppingCartVM.OrderHeader.OrderTotal);
                
                if (couponResponse != null)
                {
                    // Update the order total with the new total
                    ShoppingCartVM.OrderHeader.CouponDiscount = couponResponse;
                    ShoppingCartVM.OrderHeader.OrderTotal -= (double)couponResponse;
                    
                }
                else
                {
                    // Handle the error case when the coupon is not valid or the order total is below the minimum purchase amount

                    return RedirectToAction(nameof(Index));
                }
            }

            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                var soldQuantity = item.Count; // Get the quantity of the item sold
                item.Product.TotalQuantity -= soldQuantity; // Reduce the total quantity of the product
                _wholeRepo.shoppingCart.Update(item);
                _wholeRepo.Save(); 
            }

            double totalAmountStripe = ShoppingCartVM.OrderHeader.OrderTotal;

            var domain = Request.Scheme+ "://"+ Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/Index",

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "inr",
                            UnitAmount = (long)(totalAmountStripe * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "VENDOR Store",
                                Description = "Paying to Vendor Store through Stripe"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
            };

            //foreach (var item in ShoppingCartVM.ShoppingCartList)
            //{
            //    var sessionLineItem = new SessionLineItemOptions
            //    {
            //        PriceData = new SessionLineItemPriceDataOptions
            //        {
            //            UnitAmount = (long)(item.Price * 100),
            //            Currency = "inr",
            //            ProductData = new SessionLineItemPriceDataProductDataOptions
            //            {
            //                Name = item.Product.Title
            //            }
            //        },
            //        Quantity = item.Count
            //    };
            //    options.LineItems.Add(sessionLineItem);
            //}
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

        public async Task<IActionResult> Coupon(string coupon, int? OrderTotal)
        {
            if (string.IsNullOrEmpty(coupon) || OrderTotal == null)
            {
                return BadRequest(); // Return an appropriate error response
            }

            var couponObj = await _wholeRepo.coupon.GetAsync(u => u.CouponName == coupon);

            if (couponObj != null)
            {
                if (OrderTotal >= couponObj.MinPurchase)
                {
                    decimal discountPrice;
                    decimal cartTotal = Convert.ToDecimal(OrderTotal);
                    if (couponObj.DiscountAmount > 0)
                    {
                        discountPrice = (decimal)(cartTotal - couponObj.DiscountAmount);
                    }
                    else
                    {
                        discountPrice = (decimal)(cartTotal - (cartTotal) * (couponObj.DiscountPercentage/100));
                    }

                    decimal newTotal = (decimal)(OrderTotal - discountPrice);

                    var response = new
                    {
                        success = true,
                        discountPrice,
                        newTotal
                    };

                    return Json(response); // Return the discount price
                }
                else
                {
                    TempData["error"] = "Order total is below the minimum purchase amount.";
                    
                    var responses = new
                    {
                        success = false,
                        errorMessage = "Order total is below the minimum purchase amount."
                };
                    return Json(responses);
                    // Return an appropriate error response
                }
            }
            TempData["error"] = "Coupon not found.";
            var responsed = new
            {
                success = false,
                errorMessage = "Coupon not found"
            };
            return Json(responsed);
        }

        public async Task<decimal> CouponCheckout(string coupon, int? OrderTotal)
        {
            if (string.IsNullOrEmpty(coupon) || OrderTotal == null)
            {
                return 0; // Return an appropriate error response
            }

            var couponObj = await _wholeRepo.coupon.GetAsync(u => u.CouponName == coupon);

            if (couponObj != null)
            {
                if (OrderTotal >= couponObj.MinPurchase)
                {
                    decimal newTotal;
                    decimal cartTotal = Convert.ToDecimal(OrderTotal);
                    if (couponObj.DiscountAmount > 0)
                    {
                        newTotal = (decimal)(cartTotal - couponObj.DiscountAmount);
                    }
                    else
                    {
                        newTotal = (decimal)(cartTotal - (cartTotal) * (couponObj.DiscountPercentage / 100));
                    }

                    decimal discountPrice = (decimal)(OrderTotal - newTotal);

                    return discountPrice; // Return the discount price
                }
                else
                {
                    TempData["error"] = "Order total is below the minimum purchase amount.";
                    return 0;
                    // Return an appropriate error response
                }
            }
            TempData["error"] = "Coupon not found.";
            return 0;
        }

        private double GetPrice (ShoppingCart shoppingCart)
        {
            return shoppingCart.Product.Price;
        }

    }
}
