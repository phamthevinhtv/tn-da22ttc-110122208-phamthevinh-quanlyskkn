using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class DonViVM
    {
        public int MaDV { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đơn vị")]
        public string? TenDV { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^[+0-9\s().,;-]{8,30}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        public int? MaDVCha { get; set; }

        public IEnumerable<SelectListItem> DSDonViCha { get; set; } = new List<SelectListItem>();

        public bool DaSuDung { get; set; }
        
        public List<DonViVM> DSDonViCon { get; set; } = new List<DonViVM>();
        
        [Required(ErrorMessage = "Vui lòng chọn đơn vị trực thuộc")]
        public int? MaDVCon { get; set; }

        public IEnumerable<SelectListItem> DSDonViConSelect { get; set; } = new List<SelectListItem>();

        public string TenDVCapTren { get; set; } = string.Empty;
    }
}
