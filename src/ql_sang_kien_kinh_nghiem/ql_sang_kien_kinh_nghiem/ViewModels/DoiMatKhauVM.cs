using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class DoiMatKhauVM
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string? MatKhauCu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu cũ")]
        [Compare(nameof(MatKhauCu), ErrorMessage = "Mật khẩu cũ không khớp")]
        public string? XacNhanMatKhauCu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string? MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu mới không khớp")]
        public string? XacNhanMatKhauMoi { get; set; }
    }
}