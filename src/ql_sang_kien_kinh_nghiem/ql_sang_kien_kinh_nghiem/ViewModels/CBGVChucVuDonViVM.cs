using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class CBGVChucVuDonViVM
    {
        [Required]
        public string? MaCBGV { get; set; }

        public int? MaCV { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đơn vị")]        
        public int? MaDV { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime? ThoiGianBatDau { get; set; }

        public DateTime? ThoiGianKetThuc { get; set; }

        public string TenCV { get; set; } = string.Empty;

        public string TenDV { get; set; } = string.Empty;
    }
}