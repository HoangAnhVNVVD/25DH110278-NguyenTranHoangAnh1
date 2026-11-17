using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using _25DH110278_NguyenTranHoangAnh.Models;

namespace _25DH110278_NguyenTranHoangAnh.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string customerName, string paymentStatus, DateTime? orderDate, int? page)
        {
            var orders = db.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).AsQueryable();

            if (!string.IsNullOrEmpty(customerName))
            {
                orders = orders.Where(o => o.Customer.CustomerName.Contains(customerName));
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentStatus == paymentStatus);
            }

            if (orderDate.HasValue)
            {
                orders = orders.Where(o => DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(orderDate.Value));
            }

            ViewBag.CustomerName = customerName;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.OrderDate = orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") : "";

            var statusList = new List<string> { "Chờ thanh toán", "Chưa thanh toán", "Thanh toán tiền mặt", "Thanh toán paypal", "Trả góp", "Đang xử lý", "Đang vận chuyển", "Đã giao", "Đã hủy" };
            ViewBag.PaymentStatusList = new SelectList(statusList, paymentStatus);

            int pageNumber = page ?? 1;
            int pageSize = 15;
            var pagedOrders = orders.OrderByDescending(o => o.OrderID).ToPagedList(pageNumber, pageSize);

            return View(pagedOrders);
        }

        public ActionResult DetailedOrder(int id)
        {
            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        public ActionResult UpdateStatus(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var statusList = new List<string> { "Chờ thanh toán", "Chưa thanh toán", "Đang xử lý", "Đang vận chuyển", "Đã giao", "Đã hủy" };
            ViewBag.PaymentStatusList = new SelectList(statusList, order.PaymentStatus);

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string paymentStatus)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;
                db.SaveChanges();
            }
            return RedirectToAction("DetailedOrder", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
