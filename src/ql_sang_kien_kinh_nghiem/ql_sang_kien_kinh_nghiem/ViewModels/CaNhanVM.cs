using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class CaNhanVM
    {
        public string Ten { get; set; } = string.Empty;

        public string TenCVDV { get; set; } = string.Empty;
        
        public string TenTDN { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [RegularExpression(@"^[a-z0-9!@#$%^&*()_+=\[{\]};:<>|./?,-]{6,}$", 
        ErrorMessage = "Tên đăng nhập phải có ít nhất 6 ký tự, gồm chữ thường, số và ký tự đặc biệt")]
        public string? TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }
    }
}