namespace ql_sang_kien_kinh_nghiem.Models
{
    public class MonHocSangKien
    {
        public int MaMH { get; set; }

        public int MaSK { get; set; }

        public MonHoc MonHoc { get; set; } = null!;

        public SangKien SangKien { get; set; } = null!;
    }
}