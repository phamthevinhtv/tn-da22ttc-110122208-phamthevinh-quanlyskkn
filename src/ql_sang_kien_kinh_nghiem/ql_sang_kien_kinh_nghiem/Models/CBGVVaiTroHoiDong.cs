namespace ql_sang_kien_kinh_nghiem.Models
{
    public class CBGVVaiTroHoiDong
    {
        public string MaCBGV { get; set; } = string.Empty;

        public int MaHD { get; set; }

        public int MaVT { get; set; }

        public CanBoGiangVien CanBoGiangVien { get; set; } = null!;

        public HoiDong HoiDong { get; set; } = null!;

        public VaiTro VaiTro { get; set; } = null!;
    }
}