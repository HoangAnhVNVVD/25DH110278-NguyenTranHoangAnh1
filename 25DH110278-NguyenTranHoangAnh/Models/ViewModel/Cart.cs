using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH110278_NguyenTranHoangAnh.Models.ViewModel
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items => items;

        public void AddItem(int productid, string productImage, string productName, decimal unitPrice, int quantity, string category)
        {
            var existingItem = items.FirstOrDefault(i => i.ProductID == productid);
            if (existingItem == null)
            {
                items.Add(new CartItem
                {
                    ProductID = productid,
                    ProductImage = productImage,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    Category = category
                });
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        public void Add(CartItem item)
        {
            var existing = items.FirstOrDefault(i => i.ProductID == item.ProductID);
            if (existing == null)
            {
                items.Add(item);
            }
            else
            {
                existing.Quantity += item.Quantity;
            }
        }

        public void RemoveItem(int productid)
        {
            items.RemoveAll(i => i.ProductID == productid);
        }

        public void Remove(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        public decimal TotalAmount()
        {
            return items.Sum(i => i.TotalPrice);
        }

        public void Clear()
        {
            items.Clear();
        }

        public void UpdateQuantity(int productid, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productid);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    items.Remove(item);
                }
            }
        }

        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => i.Category).ToList();

        public int PageNumber { get; set; }

        public int PageSize { get; set; } = 6;

        public IPagedList<Product> SimilarProducts { get; set; }
    }
}
