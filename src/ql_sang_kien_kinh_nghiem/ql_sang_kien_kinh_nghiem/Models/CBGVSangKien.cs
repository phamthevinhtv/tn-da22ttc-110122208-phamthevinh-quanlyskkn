namespace ql_sang_kien_kinh_nghiem.Models
{
    public class CBGVSangKien
    {
        public string MaCBGV { get; set; } = string.Empty;

        public int MaSK { get; set; }

        public decimal TiLeDongGop { get; set; }

        public CanBoGiangVien CanBoGiangVien { get; set; } = null!;

        public SangKien SangKien { get; set; } = null!;

        public string? GhiChu { get; set; }
    }
}