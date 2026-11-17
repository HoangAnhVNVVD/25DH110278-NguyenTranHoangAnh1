using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using _25DH110278_NguyenTranHoangAnh.Models;
using _25DH110278_NguyenTranHoangAnh.Models.ViewModel;

namespace _25DH110278_NguyenTranHoangAnh.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var checkUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (checkUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "C"
                };

                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };

                db.Users.Add(user);
                db.Customers.Add(customer);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    if (user.UserRole == "A")
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác");
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        [Authorize]
        public ActionResult ProfileInfo()
        {
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            return View(customer);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileInfo(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Customers.Find(customer.CustomerID);
                if (existing != null)
                {
                    existing.CustomerName = customer.CustomerName;
                    existing.CustomerPhone = customer.CustomerPhone;
                    existing.CustomerEmail = customer.CustomerEmail;
                    existing.CustomerAddress = customer.CustomerAddress;
                    db.SaveChanges();
                    ViewBag.Message = "Cập nhật thông tin thành công!";
                }
            }
            return View(customer);
        }
    }
}