using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class MonHocVM
    {
        public int MaMH { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn ngành")]
        public int? MaNganh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên môn học")]
        public string? TenMH { get; set; }

        public string? MoTaMH { get; set; }

        public string? CumNguNghia { get; set; }

        public IEnumerable<SelectListItem> DSNganh { get; set; } = new List<SelectListItem>();

        public bool DaSuDung { get; set; }
    }
}
