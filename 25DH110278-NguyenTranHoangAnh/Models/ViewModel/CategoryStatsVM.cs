using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110278_NguyenTranHoangAnh.Models.ViewModel
{
    public class CategoryStatsVM
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal AvgPrice { get; set; }
    }
}