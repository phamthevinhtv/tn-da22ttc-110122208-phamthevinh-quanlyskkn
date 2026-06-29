namespace ql_sang_kien_kinh_nghiem.Models
{
    public class MonHoc
    {
        public int MaMH { get; set; }

        public int MaNganh { get; set; }

        public string TenMH { get; set; } = string.Empty;

        public string? MoTaMH { get; set; }

        public string? CumNguNghia { get; set; }

        public Nganh Nganh { get; set; } = null!;

        public ICollection<MonHocSangKien> DSMonHocSangKien { get; set; } = new List<MonHocSangKien>();
    }
}