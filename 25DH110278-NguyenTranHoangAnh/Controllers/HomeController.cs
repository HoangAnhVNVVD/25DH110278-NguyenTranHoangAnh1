using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using _25DH110278_NguyenTranHoangAnh.Models;
using _25DH110278_NguyenTranHoangAnh.Models.ViewModel;

namespace _25DH110278_NguyenTranHoangAnh.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            model.SearchTerm = searchTerm;

            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductDecription.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));
            }

            model.FeaturedProducts = db.Products.OrderByDescending(p => p.OrderDetails.Count()).Take(10).ToList();

            int pageNumber = page ?? 1;
            int pageSize = 6;
            model.NewProducts = products.OrderByDescending(p => p.ProductID).ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();
            ProductDetailsVM model = new ProductDetailsVM();
            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;

            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(8).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }
            model.estimatedValue = model.quantity * pro.ProductPrice;

            return View("ProductDetails", model);
        }

        public ActionResult ProductDetail(int? id, int? quantity, int? page)
        {
            return ProductDetails(id, quantity, page);
        }

        public ActionResult ProductList(int? categoryId, int? page)
        {
            if (categoryId == null)
            {
                return RedirectToAction("Index");
            }

            var category = db.Categories.Find(categoryId);
            if (category == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryName = category.CategoryName;

            int pageNumber = page ?? 1;
            int pageSize = 6;
            var products = db.Products
                .Where(p => p.CategoryID == categoryId)
                .OrderByDescending(p => p.ProductID)
                .ToPagedList(pageNumber, pageSize);

            return View(products);
        }

        [ChildActionOnly]
        public ActionResult CategoryMenu()
        {
            var categories = db.Categories.ToList();
            return PartialView("CategoryMenuPV", categories);
        }

        [ChildActionOnly]
        public ActionResult PersonalMenu()
        {
            var model = new PersonalMenuVM();
            if (User.Identity.IsAuthenticated)
            {
                model.IsLoggedIn = true;
                model.Username = User.Identity.Name;
            }
            else
            {
                model.IsLoggedIn = false;
            }

            if (Session["Cart"] is Cart cartObj)
            {
                model.CartCount = cartObj.Items != null ? cartObj.Items.Sum(i => i.Quantity) : 0;
            }
            else if (Session["Cart"] is List<CartItem> cartList)
            {
                model.CartCount = cartList.Sum(i => i.Quantity);
            }
            else
            {
                model.CartCount = 0;
            }

            return PartialView("PersonalMenuPV", model);
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
