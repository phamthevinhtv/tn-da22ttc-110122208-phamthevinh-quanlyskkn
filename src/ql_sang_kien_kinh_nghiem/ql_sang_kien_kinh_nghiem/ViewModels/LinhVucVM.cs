using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class LinhVucVM
    {
        public int MaLV { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên lĩnh vực")]
        public string? TenLV { get; set; }

        public bool DaSuDung { get; set; }
    }
}
