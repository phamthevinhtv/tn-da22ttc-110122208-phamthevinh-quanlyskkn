using System.ComponentModel.DataAnnotations;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class NganhVM
    {
        public int MaNganh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ngành")]
        public string? TenNganh { get; set; }

        public List<MonHocVM> DSMonHoc { get; set; } = new();

        public bool DaSuDung { get; set; }
    }
}
