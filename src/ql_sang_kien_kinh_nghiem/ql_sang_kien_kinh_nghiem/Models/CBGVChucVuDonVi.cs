namespace ql_sang_kien_kinh_nghiem.Models
{
    public class CBGVChucVuDonVi
    {
        public string MaCBGV { get; set; } = string.Empty;

        public int MaCV { get; set; }

        public int MaDV { get; set; }

        public DateTime ThoiGianBatDau { get; set; }

        public DateTime? ThoiGianKetThuc { get; set; }

        public CanBoGiangVien CanBoGiangVien { get; set; } = null!;

        public ChucVu ChucVu { get; set; } = null!;

        public DonVi DonVi { get; set; } = null!;
    }
}