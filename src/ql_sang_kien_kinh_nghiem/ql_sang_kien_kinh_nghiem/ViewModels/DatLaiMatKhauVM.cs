using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class DatLaiMatKhauVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [RegularExpression(@"^[a-z0-9!@#$%^&*()_+=\[{\]};:<>|./?,-]{6,}$", 
        ErrorMessage = "Tên đăng nhập phải có ít nhất 6 ký tự, gồm chữ thường, số và ký tự đặc biệt")]
        public string? TenDangNhap { get; set; } 

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
    }
}
