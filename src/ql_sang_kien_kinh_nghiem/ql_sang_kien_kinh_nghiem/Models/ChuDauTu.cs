namespace ql_sang_kien_kinh_nghiem.Models
{
    public class ChuDauTu
    {
        public int MaCDT { get; set; }

        public string TenCDT { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public string DiaChi { get; set; } = string.Empty;

        public ICollection<SangKien> DSSangKien { get; set; } = new List<SangKien>();
    }
}