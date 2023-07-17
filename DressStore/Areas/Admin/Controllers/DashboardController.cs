using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using DressStore.Models.ViewModels;
using DressStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Pdf;
using PdfSharpCore;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace DressStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IWholeRepository _repo;
        [BindProperty]
        public OrderViewModel OrderViewModel { get; set; }

        public DashboardController(IWholeRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {

            IEnumerable<OrderHeader> orderHeaders = (await _repo.orderHeader.GetAllAsync()).ToList();

            int shippedCount = orderHeaders.Count(u => u.OrderStatus == "Shipped");
            int approvedCount = orderHeaders.Count(u => u.OrderStatus == "Approved");
            int cancelledCount = orderHeaders.Count(u => u.OrderStatus == "Cancelled");
            int pendingCount = orderHeaders.Count(u => u.OrderStatus == "Pending");
            int orderCount = orderHeaders.Count();
            decimal orderTotal = (decimal)orderHeaders.Sum(u => u.OrderTotal);

            DateTime currentDate = DateTime.Now;
            DateTime startOfWeek1 = currentDate.StartOfWeek(DayOfWeek.Monday);
            DateTime endOfWeek1 = startOfWeek1.AddDays(6);

            DateTime startOfWeek2 = currentDate.StartOfWeek(DayOfWeek.Monday).AddDays(-7);
            DateTime endOfWeek2 = startOfWeek2.AddDays(6);

            DateTime startOfWeek3 = currentDate.StartOfWeek(DayOfWeek.Monday).AddDays(-14);
            DateTime endOfWeek3 = startOfWeek3.AddDays(6);

            DateTime startOfWeek4 = currentDate.StartOfWeek(DayOfWeek.Monday).AddDays(-21);
            DateTime endOfWeek4 = startOfWeek4.AddDays(6);

             int ApprovedCountWeek1 = orderHeaders.Count(u => u.OrderTotal != null && u.OrderDate >= startOfWeek1 && u.OrderDate <= endOfWeek1);
            int ApprovedCountWeek2 = orderHeaders.Count(u => u.OrderTotal != null && u.OrderDate >= startOfWeek2 && u.OrderDate <= endOfWeek2);
            int ApprovedCountWeek3 = orderHeaders.Count(u => u.OrderTotal != null && u.OrderDate >= startOfWeek3 && u.OrderDate <= endOfWeek3);
            int ApprovedCountWeek4 = orderHeaders.Count(u => u.OrderTotal != null && u.OrderDate >= startOfWeek4 && u.OrderDate <= endOfWeek4);

            ViewBag.ShippedCount = shippedCount;
            ViewBag.ApprovedCount = approvedCount;
            ViewBag.CancelledCount = cancelledCount;
            ViewBag.PendingCount = pendingCount;
            ViewBag.OrderTotal = orderTotal;
            ViewBag.OrderCount = orderCount;
            ViewBag.ApprovedCountWeek1 = ApprovedCountWeek1;
            ViewBag.ApprovedCountWeek2 = ApprovedCountWeek2;
            ViewBag.ApprovedCountWeek3 = ApprovedCountWeek3;
            ViewBag.ApprovedCountWeek4 = ApprovedCountWeek4;

            return View(orderHeaders);

        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime startDate , DateTime endDate )
        {

            OrderViewModel = new OrderViewModel
            {
                orderHeaders = await _repo.orderHeader.GetAllAsync(u => u.OrderDate >= startDate && u.OrderDate <= endDate, includeProperties: "ApplicationUser"),
                OrderDetail = await _repo.orderDetail.GetAllAsync(u => u.OrderHeader.OrderDate >= startDate && u.OrderHeader.OrderDate <= endDate, includeProperties: "Product")
            };

            return View(OrderViewModel);
        }

        [HttpPost]
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> GeneratePDF(DateTime startDate, DateTime endDate)
        {

            OrderViewModel = new OrderViewModel
            {
                orderHeaders = await _repo.orderHeader.GetAllAsync(u => u.OrderDate >= startDate && u.OrderDate <= endDate, includeProperties: "ApplicationUser"),
                OrderDetail = await _repo.orderDetail.GetAllAsync(u => u.OrderHeader.OrderDate >= startDate && u.OrderHeader.OrderDate <= endDate, includeProperties: "Product")
            };

            var document = new PdfDocument();
            string htmlcontent = "<div style='width:100%; text-align:center'>";
            htmlcontent += "<h2>VENDOR store</h2>";

            if (OrderViewModel != null)
            {
                htmlcontent += "<h2> Sales Report - From :" + startDate + " To :" + endDate + "</h2>";
                htmlcontent += "<h3> Report By Admin </h3>";
                htmlcontent += "<div>";
            }

            htmlcontent += "<table style ='width:100%; border: 1px solid #000'>";
            htmlcontent += "<thead style='font-weight:bold'>";
            htmlcontent += "<tr>";
            htmlcontent += "<td style='border:1px solid #000'> Order Id </td>";
            htmlcontent += "<td style='border:1px solid #000'> Billing Name </td>";
            htmlcontent += "<td style='border:1px solid #000'>Order Date</td>";
            htmlcontent += "<td style='border:1px solid #000'>Total</td >";
            htmlcontent += "<td style='border:1px solid #000'>Order Status</td>";
            htmlcontent += "</tr>";
            htmlcontent += "</thead >";

            htmlcontent += "<tbody>";
            if (OrderViewModel != null)
            {
                foreach (var item in OrderViewModel.orderHeaders)
                {
                    htmlcontent += "<tr>";
                    htmlcontent += "<td>" + item.Id + "</td>";
                    htmlcontent += "<td>" + item.FirstName + "</td>";
                    htmlcontent += "<td>" + item.OrderDate + "</td >";
                    htmlcontent += "<td>" + item.OrderTotal.ToString("c") + "</td>";
                    htmlcontent += "<td>" + item.OrderStatus + "</td >";
                    htmlcontent += "</tr>";
                };
            }
            htmlcontent += "</tbody>";

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
            string Filename = "SalesReport-" + startDate + ".pdf";
            return File(response, "application/pdf", Filename);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }

}
