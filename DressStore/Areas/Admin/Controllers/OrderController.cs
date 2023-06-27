using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using DressStore.Models.ViewModels;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DressStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IWholeRepository _repo;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IWholeRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = await _repo.orderHeader.GetAsync(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = await _repo.orderDetail.GetAllAsync(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var OrderHeaderFromDb = await _repo.orderHeader.GetAsync(u => u.Id == OrderVM.OrderHeader.Id);
            OrderHeaderFromDb.FirstName = OrderVM.OrderHeader.FirstName;
            OrderHeaderFromDb.LastName = OrderVM.OrderHeader.LastName;
            OrderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            OrderHeaderFromDb.State = OrderVM.OrderHeader.State;
            OrderHeaderFromDb.City = OrderVM.OrderHeader.City;
            OrderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            OrderHeaderFromDb.phoneNumber = OrderVM.OrderHeader.phoneNumber;

            if (!String.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                OrderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;

            if(string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
                OrderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;

            _repo.orderHeader.Update(OrderHeaderFromDb);
            _repo.Save();

            TempData["success"] = "Order Details Updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderHeaderFromDb.Id });
        }

        [HttpPost]
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> StartProcessing()
        {
            _repo.orderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _repo.Save();
            TempData["success"] = "Order Details Updated successfully";
            return RedirectToAction(nameof(Details) , new {orderId =OrderVM.OrderHeader.Id});
        }

        [HttpPost]
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ShipOrder()
        {
            var orderHeader = await _repo.orderHeader.GetAsync(u=>u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _repo.orderHeader.Update(orderHeader);
            _repo.Save();
            TempData["success"] = "Order Shipped successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        #region APICALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin))
            {
                orderHeaders = (await _repo.orderHeader.GetAllAsync(includeProperties: "ApplicationUser")).ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaders = (await _repo.orderHeader.GetAllAsync(u=>u.ApplicationUserId ==  userId , includeProperties: "ApplicationUser"));
            }
            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
