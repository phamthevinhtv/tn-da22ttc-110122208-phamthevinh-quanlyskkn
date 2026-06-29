using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class VaiTroVM
    {
        public int MaVT { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên vai trò")]
        public string? TenVT { get; set; }

        public bool DaSuDung { get; set; }
    }
}
