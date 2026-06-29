using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class TrinhDoVM
    {
        public int MaTD { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên trình độ")]
        public string? TenTD { get; set; }

        public bool DaSuDung { get; set; }
    }
}
