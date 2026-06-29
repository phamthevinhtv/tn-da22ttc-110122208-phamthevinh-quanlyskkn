namespace ql_sang_kien_kinh_nghiem.Models
{
    public class DanhGia
    {
        public int MaSK { get; set; }

        public int MaHD { get; set; }

        public string MaCBGV { get; set; } = string.Empty;

        public string YKienNhanXet { get; set; } = string.Empty;

        public int CongNhan { get; set; }

        public int LanDG { get; set; }

        public DateTime NgayDG { get; set; }

        public SangKien SangKien { get; set; } = null!;

        public HoiDong HoiDong { get; set; } = null!;

        public CanBoGiangVien CanBoGiangVien { get; set; } = null!;
    }
}