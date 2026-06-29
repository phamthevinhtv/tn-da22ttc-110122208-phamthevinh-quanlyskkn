namespace ql_sang_kien_kinh_nghiem.Models
{
    public class TaiKhoan
    {
        public int MaTK { get; set; }

        public string? MaCBGV { get; set; }

        public string TenDangNhap { get; set; } = string.Empty;

        public string MatKhau { get; set; } = string.Empty;

        public string Quyen { get; set; } = string.Empty;

        public int TrangThaiTK { get; set; }

        public CanBoGiangVien? CanBoGiangVien { get; set; }
    }
}