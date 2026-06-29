using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class CBGVTrinhDoNganhVM
    {
        [Required]
        public string? MaCBGV { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trình độ")]
        public int? MaTD { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngành")]        
        public int? MaNganh { get; set; }

        public string TenTD { get; set; } = string.Empty;

        public string TenNganh { get; set; } = string.Empty;
    }
}
