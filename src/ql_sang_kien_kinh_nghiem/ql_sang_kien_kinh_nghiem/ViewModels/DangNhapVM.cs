using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class DangNhapVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [RegularExpression(@"^[a-z0-9!@#$%^&*()_+=\[{\]};:<>|./?,-]{6,}$", 
        ErrorMessage = "Tên đăng nhập phải có ít nhất 6 ký tự, gồm chữ thường, số và ký tự đặc biệt")]
        public string? TenDangNhap { get; set; } 

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string? MatKhau { get; set; } 

    }
}
