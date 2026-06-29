using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class PhanHoiVM
    {
        public string MaCBGV { get; set; } = string.Empty;

        public int MaSK { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayGui { get; set; }

        public string TenCBGV { get; set; } = string.Empty;

        public string TenDV { get; set; } = string.Empty;
    }
}