using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _25DH110278_NguyenTranHoangAnh.Models;
using _25DH110278_NguyenTranHoangAnh.Models.ViewModel;

namespace _25DH110278_NguyenTranHoangAnh.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private List<CartItem> GetCartItems()
        {
            var cartService = new CartService(Session);
            var cart = cartService.GetCart();
            if (cart != null && cart.Items != null && cart.Items.Any())
            {
                return cart.Items.ToList();
            }
            if (Session["Cart"] is List<CartItem> list && list.Any())
            {
                return list;
            }
            return new List<CartItem>();
        }

        public ActionResult Checkout()
        {
            var cart = GetCartItems();
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CheckoutVM
            {
                CartItems = cart,
                TotalAmount = cart.Sum(item => item.TotalPrice),
                OrderDate = DateTime.Now,
                ShippingAddress = customer.CustomerAddress,
                CustomerID = customer.CustomerID,
                Username = customer.Username,
                PaymentMethod = "Tiền mặt",
                ShippingMethod = "Giao hàng tiêu chuẩn"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = GetCartItems();
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
                if (customer == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (model.PaymentMethod == "Paypal")
                {
                    return RedirectToAction("PaymentWithPaypal", "Paypal", model);
                }

                string paymentStatus = "Chờ thanh toán";
                switch (model.PaymentMethod)
                {
                    case "Tiền mặt":
                        paymentStatus = "Thanh toán tiền mặt";
                        break;
                    case "Paypal":
                        paymentStatus = "Thanh toán paypal";
                        break;
                    case "Mua trước trả sau":
                        paymentStatus = "Trả góp";
                        break;
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var order = new Order
                        {
                            CustomerID = customer.CustomerID,
                            OrderDate = DateTime.Now,
                            TotalAmount = cart.Sum(item => item.TotalPrice),
                            PaymentStatus = paymentStatus,
                            AddressDelivery = model.ShippingAddress ?? customer.CustomerAddress,
                            ShippingAddress = model.ShippingAddress ?? customer.CustomerAddress,
                            DeliveryMethod = model.ShippingMethod ?? "Giao hàng tiêu chuẩn",
                            PaymentMethod = model.PaymentMethod ?? "Tiền mặt"
                        };

                        db.Orders.Add(order);
                        db.SaveChanges();

                        foreach (var item in cart)
                        {
                            var orderDetail = new OrderDetail
                            {
                                OrderID = order.OrderID,
                                ProductID = item.ProductID,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                TotalPrice = item.TotalPrice
                            };
                            db.OrderDetails.Add(orderDetail);

                            var prod = db.Products.Find(item.ProductID);
                            if (prod != null)
                            {
                                prod.Quantity = Math.Max(0, prod.Quantity - item.Quantity);
                            }
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        new CartService(Session).ClearCart();
                        Session["Cart"] = null;

                        return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                    }
                    catch
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình đặt hàng. Vui lòng thử lại.");
                    }
                }
            }

            model.CartItems = cart;
            model.TotalAmount = cart.Sum(item => item.TotalPrice);
            return View(model);
        }

        public ActionResult OrderSuccess(int id)
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

        public ActionResult MyOrder(string searchTerm, string status)
        {
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(od => od.Product.Category))
                .Where(o => o.CustomerID == customer.CustomerID)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                int searchId;
                bool isNumber = int.TryParse(searchTerm.Trim().Replace("#", ""), out searchId);
                orders = orders.Where(o => (isNumber && o.OrderID == searchId) || o.OrderDetails.Any(od => od.Product.ProductName.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả đơn")
            {
                orders = orders.Where(o => o.PaymentStatus == status);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentStatus = string.IsNullOrEmpty(status) ? "Tất cả đơn" : status;

            var orderList = orders.OrderByDescending(o => o.OrderID).ToList();
            return View(orderList);
        }

        public ActionResult CreateOrderAfterPaypal(CheckoutVM model)
        {
            var cart = GetCartItems();
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);

            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => i.TotalPrice),
                PaymentStatus = "Đã thanh toán qua PayPal",
                AddressDelivery = model != null && !string.IsNullOrEmpty(model.ShippingAddress) ? model.ShippingAddress : customer.CustomerAddress,
                ShippingAddress = model != null && !string.IsNullOrEmpty(model.ShippingAddress) ? model.ShippingAddress : customer.CustomerAddress,
                DeliveryMethod = model != null && !string.IsNullOrEmpty(model.ShippingMethod) ? model.ShippingMethod : "Giao hàng tiêu chuẩn",
                PaymentMethod = "PayPal"
            };

            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                };
                db.OrderDetails.Add(orderDetail);

                var prod = db.Products.Find(item.ProductID);
                if (prod != null)
                {
                    prod.Quantity = Math.Max(0, prod.Quantity - item.Quantity);
                }
            }
            db.SaveChanges();

            new CartService(Session).ClearCart();
            Session["Cart"] = null;
            Session["PayPalCheckoutModel"] = null;

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
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
