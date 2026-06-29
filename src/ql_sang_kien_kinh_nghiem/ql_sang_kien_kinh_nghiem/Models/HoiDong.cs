namespace ql_sang_kien_kinh_nghiem.Models
{
    public class HoiDong
    {
        public int MaHD { get; set; }

        public string TenHD { get; set; } = string.Empty;

        public string CanCu { get; set; } = string.Empty;

        public string DieuKhoan { get; set; } = string.Empty;

        public DateTime NgayLap { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public ICollection<CBGVVaiTroHoiDong> DSCBGVVaiTroHoiDong { get; set; } = new List<CBGVVaiTroHoiDong>();

        public ICollection<DanhGia> DSDanhGia { get; set; } = new List<DanhGia>();
    }
}