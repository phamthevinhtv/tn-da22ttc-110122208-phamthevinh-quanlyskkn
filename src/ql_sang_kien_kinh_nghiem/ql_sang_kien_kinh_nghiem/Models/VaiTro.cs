namespace ql_sang_kien_kinh_nghiem.Models
{
    public class VaiTro
    {
        public int MaVT { get; set; }

        public string TenVT { get; set; } = string.Empty;

        public ICollection<CBGVVaiTroHoiDong> DSCBGVVaiTroHoiDong { get; set; } = new List<CBGVVaiTroHoiDong>();
    }
}