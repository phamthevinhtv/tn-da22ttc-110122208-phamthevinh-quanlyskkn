using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class DanhGiaVM
    {
        public int MaSK { get; set; }

        public int MaHD { get; set; }

        public string MaCBGV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập ý kiến nhận xét")]
        public string YKienNhanXet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn công nhận/không công nhận")]
        public int CongNhan { get; set; }

        public string TenCBGV { get; set; } = string.Empty;

        public int LanDG { get; set; }

        public DateTime NgayDG { get; set; }
    }
}