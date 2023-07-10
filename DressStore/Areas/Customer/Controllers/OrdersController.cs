using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models.ViewModels;
using DressStore.Models;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Pdf;
using PdfSharpCore;
using System.Security.Claims;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace DressStore.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Authorize(Roles = SD.Role_Customer)]
    public class OrdersController : Controller
    {
        private readonly IWholeRepository _repo;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrdersController(IWholeRepository repo)
        {
            _repo = repo;
        }

        [Area("Customer")]
        [Authorize(Roles = SD.Role_Customer)]
        public IActionResult Index()
        {
            return View();
        }

        [Area("Customer")]
        [Authorize(Roles = SD.Role_Customer)]
        public async Task<IActionResult> Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = await _repo.orderHeader.GetAsync(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = await _repo.orderDetail.GetAllAsync(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }


        [HttpGet]
        [Area("Customer")]
        [Authorize(Roles = SD.Role_Customer)]
        public async Task<IActionResult> GeneratePDF(int orderId)
        {

            OrderVM = new()
            {
                OrderHeader = await _repo.orderHeader.GetAsync(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = await _repo.orderDetail.GetAllAsync(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            var document = new PdfDocument();
            string htmlcontent = "<div style='width:100%; text-align:center'>";
            htmlcontent += "<h2>VENDOR store</h2>";

            if (OrderVM != null)
            {
                htmlcontent += "<h2> Invoice No: INV" + orderId + " & Invoice Date:" + DateTime.Now + "</h2>";
                htmlcontent += "<h3> Customer : " + OrderVM.OrderHeader.FirstName + " " + OrderVM.OrderHeader.LastName + "</h3>";
                htmlcontent += "<p>" + OrderVM.OrderHeader.StreetAddress + " , " + OrderVM.OrderHeader.City + "</p>";
                htmlcontent += "<p>" + OrderVM.OrderHeader.State + " , " + OrderVM.OrderHeader.PostalCode + "</p>";
                htmlcontent += "<h3> Contact : " + OrderVM.OrderHeader.phoneNumber + "</h3>";
                htmlcontent += "<div>";
            }



            htmlcontent += "<table style ='width:100%; border: 1px solid #000'>";
            htmlcontent += "<thead style='font-weight:bold'>";
            htmlcontent += "<tr>";
            htmlcontent += "<td style='border:1px solid #000'> Product Code </td>";
            htmlcontent += "<td style='border:1px solid #000'> Description </td>";
            htmlcontent += "<td style='border:1px solid #000'>Qty</td>";
            htmlcontent += "<td style='border:1px solid #000'>Price</td >";
            htmlcontent += "<td style='border:1px solid #000'>Total</td>";
            htmlcontent += "</tr>";
            htmlcontent += "</thead >";

            htmlcontent += "<tbody>";
            if (OrderVM != null)
            {
                foreach (var item in OrderVM.OrderDetail)
                {
                    htmlcontent += "<tr>";
                    htmlcontent += "<td>" + item.ProductId + "</td>";
                    htmlcontent += "<td>" + item.Product.Title + "</td>";
                    htmlcontent += "<td>" + item.Count + "</td >";
                    htmlcontent += "<td>" + item.Price.ToString("c") + "</td>";
                    htmlcontent += "<td> " + (item.Count * item.Price).ToString("c") + "</td >";
                    htmlcontent += "</tr>";
                };
            }
            htmlcontent += "</tbody>";

            htmlcontent += "</table>";
            htmlcontent += "</div>";
            htmlcontent += "<br/>";
            htmlcontent += "<br/>";
            htmlcontent += "<div style='text-align:left'>";
            htmlcontent += "<table style='width:100%; border:1px solid #000;float:right' >";
            htmlcontent += "<tr>";
            htmlcontent += "<td style='border:1px solid #000'> Summary Total </td>";
            htmlcontent += "</tr>";
            if (OrderVM != null)
            {
                htmlcontent += "<tr>";
                htmlcontent += "<td style='border: 1px solid #000'> " + OrderVM.OrderHeader.OrderTotal.ToString("c") + " </td>";

                htmlcontent += "</tr>";
            }
            htmlcontent += "</table>";
            htmlcontent += "</div>";

            htmlcontent += "</div>";

            PdfGenerator.AddPdfPages(document, htmlcontent, PageSize.A4);

            byte[]? response = null;
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                response = ms.ToArray();
            }
            string Filename = "Invoice_" + orderId + ".pdf";
            return File(response, "application/pdf", Filename);

        }

        [HttpPost]
        [Area("Customer")]
        [Authorize(Roles = SD.Role_Customer)]
        public async Task<IActionResult> CancelOrder()
        {
            var orderHeader = await _repo.orderHeader.GetAsync(u => u.Id == OrderVM.OrderHeader.Id);

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (orderHeader.OrderStatus == SD.PaymentStatusApproved)
            {
                var refundAmount = orderHeader.OrderTotal;
                var AppUser = await _repo.applicationUser.GetAsync(u => u.Id == userId);
                if (refundAmount > 0)
                {
                    AppUser.wallet = refundAmount;
                    _repo.applicationUser.Update(AppUser);
                }
                _repo.orderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _repo.orderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _repo.Save();
            TempData["success"] = "Order cancelled Succussfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        #region APICALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<OrderHeader> orderHeaders;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            orderHeaders = (await _repo.orderHeader.GetAllAsync(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser"));
            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
