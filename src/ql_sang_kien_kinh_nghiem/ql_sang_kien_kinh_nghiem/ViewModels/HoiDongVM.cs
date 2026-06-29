using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class HoiDongVM
    {
        public int MaHD { get; set; }

        public int MaDot { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên hội đồng")]
        public string? TenHD { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập căn cứ thành lập hội đồng")]
        public string? CanCu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập điều khoản thành lập hội đồng")]
        public string? DieuKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày lập")]
        public DateTime? NgayLap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày kết thúc")]
        public DateTime? NgayKetThuc { get; set; }

        public bool DaSuDung { get; set; }

        public IEnumerable<SelectListItem> DSCanBoGiangVien { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DSVaiTro { get; set; } = new List<SelectListItem>();

        public List<CBGVVaiTroHoiDongVM> DSCBGVVaiTro { get; set; } = new();

        [ValidateNever]
        public CBGVVaiTroHoiDongVM CBGVVaiTroHoiDongVM { get; set; } = new();
    }
}
