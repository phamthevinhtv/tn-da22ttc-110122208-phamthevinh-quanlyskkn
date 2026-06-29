using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class CBGVVaiTroHoiDongVM
    {
        [Required]
        public int MaHD { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tên thành viên")] 
        public string? MaCBGV { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trờ của thành viên")] 
        public int? MaVT { get; set; }

        public string HoTen { get; set; } = string.Empty;

        public string TenVT { get; set; } = string.Empty;
    }
}
