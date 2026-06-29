using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class ChucVuVM
    {
        public int MaCV { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chức vụ")]
        public string? TenCV { get; set; }

        public bool DaSuDung { get; set; }
    }
}
