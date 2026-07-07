using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class CanBoGiangVienVM
    {
        public string MaCBGV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên cán bộ, giảng viên")]
        public string? HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        public bool DaSuDung { get; set; }

        [ValidateNever]
        public CBGVTrinhDoNganhVM CBGVTrinhDoNganhVM { get; set; } = new();

        [ValidateNever]
        public CBGVChucVuDonViVM CBGVChucVuDonViVM { get; set; } = new();

        public IEnumerable<SelectListItem> DSTrinhDo { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DSNganh { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DSChucVu{ get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DSDonVi { get; set; } = new List<SelectListItem>();

        public List<CBGVTrinhDoNganhVM> DSTrinhDoNganh { get; set; } = new();

        public List<CBGVChucVuDonViVM> DSChucVuDonVi { get; set; } = new();
    }
}