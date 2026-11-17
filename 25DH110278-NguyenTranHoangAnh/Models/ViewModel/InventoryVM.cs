using System.Collections.Generic;

namespace _25DH110278_NguyenTranHoangAnh.Models.ViewModel
{
    public class InventoryVM
    {
        public List<Product> InStockProducts { get; set; }
        public List<Product> OutOfStockProducts { get; set; }
    }
}
