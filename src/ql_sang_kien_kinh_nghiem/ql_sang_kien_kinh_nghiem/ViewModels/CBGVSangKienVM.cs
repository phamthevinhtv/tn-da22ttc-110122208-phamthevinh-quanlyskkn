using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class CBGVSangKienVM
    {
        [Required(ErrorMessage = "Vui lòng chọn cán bộ/giảng viên")]
        public string? MaCBGV { get; set; }

        [Required]
        public int? MaSK { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tỉ lệ % đóng góp vào sáng kiến")]
        [Range(0, 100, ErrorMessage = "Tỉ lệ % đóng góp không hợp lệ")]
        public decimal? TiLeDongGop { get; set; }

        public string HoTen { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public List<CBGVTrinhDoNganhVM> DSTrinhDoNganh { get; set; } = new();

        public List<CBGVChucVuDonViVM> DSChucVuDonVi { get; set; } = new();
    }
}
