namespace ql_sang_kien_kinh_nghiem.Models
{
    public class LinhVucSangKien
    {
        public int MaLV { get; set; }

        public int MaSK { get; set; }

        public LinhVuc LinhVuc { get; set; } = null!;

        public SangKien SangKien { get; set; } = null!;
    }
}