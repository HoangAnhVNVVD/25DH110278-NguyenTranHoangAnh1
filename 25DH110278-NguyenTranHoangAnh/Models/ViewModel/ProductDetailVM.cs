using System.Collections.Generic;
using PagedList;

namespace _25DH110278_NguyenTranHoangAnh.Models.ViewModel
{
    public class ProductDetailVM : ProductDetailsVM
    {
        public Product ProductDetail
        {
            get => product;
            set => product = value;
        }

        public int Quantity
        {
            get => quantity;
            set => quantity = value;
        }
    }
}
