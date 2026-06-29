namespace ql_sang_kien_kinh_nghiem.Models
{
    public class PhanHoi
    {
        public string MaCBGV { get; set; } = string.Empty;

        public int MaSK { get; set; }

        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayGui { get; set; }

        public CanBoGiangVien CanBoGiangVien { get; set; } = null!;

        public SangKien SangKien { get; set; } = null!;
    }
}