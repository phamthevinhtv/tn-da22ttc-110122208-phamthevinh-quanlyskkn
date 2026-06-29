namespace ql_sang_kien_kinh_nghiem.Models
{
    public class LinhVuc
    {
        public int MaLV { get; set; }

        public string TenLV { get; set; } = string.Empty;

        public ICollection<LinhVucSangKien> DSLinhVucSangKien { get; set; } = new List<LinhVucSangKien>();
    }
}