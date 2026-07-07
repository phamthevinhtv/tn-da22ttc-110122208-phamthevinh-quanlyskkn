using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class TaiKhoanVM
    {
        public int MaTK { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn cán bộ, giảng viên")]
        public string? MaCBGV { get; set; }

        public string TenDangNhap { get; set; } = string.Empty;

        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn quyền đăng nhập")]
        public string? Quyen { get; set; }

        public string TenQuyen => Quyen switch
        {
            "ADMIN" => "Quản trị viên",
            "CBGV" => "Cán bộ, giảng viên",
            "BGD" => "Ban giám đốc",
            "DVTT" => "Đơn vị trực thuộc",
            _ => string.Empty
        };

        public int TrangThaiTK { get; set; }

        public string TenTrangThai => TrangThaiTK switch
        {
            1 => "Hoạt động",
            0 => "Đã khóa",
            _ => string.Empty
        };

        public string HoTen { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> DSQuyen { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DSCanBoGiangVien { get; set; } = new List<SelectListItem>();
    }
}
