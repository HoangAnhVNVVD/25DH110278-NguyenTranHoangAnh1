using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace _25DH110278_NguyenTranHoangAnh.Models
{
    public partial class Product
    {
        public Product()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
        }

        [Display(Name = "Mã sản phẩm")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm tối đa 255 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Mô tả sản phẩm không được để trống")]
        [Display(Name = "Mô tả sản phẩm")]
        public string ProductDecription { get; set; }

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(1000, 100000000, ErrorMessage = "Đơn giá từ 1,000 đ đến 100,000,000 đ")]
        [Display(Name = "Đơn giá")]
        public decimal ProductPrice { get; set; }

        [Display(Name = "Hình ảnh")]
        public string ProductImage { get; set; }

        [Required(ErrorMessage = "Số lượng tồn không được để trống")]
        [Range(0, 100000, ErrorMessage = "Số lượng tồn từ 0 đến 100,000")]
        [Display(Name = "Số lượng tồn")]
        public int Quantity { get; set; }

        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }

        [NotMapped]
        public string ProductDescription
        {
            get => ProductDecription;
            set => ProductDecription = value;
        }

        public virtual Category Category { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
