namespace ql_sang_kien_kinh_nghiem.Models
{
    public class CBGVTrinhDoNganh
    {
        public string MaCBGV { get; set; } = string.Empty;

        public int MaTD { get; set; }

        public int MaNganh { get; set; }

        public CanBoGiangVien CanBoGiangVien { get; set; } = null!;

        public TrinhDo TrinhDo { get; set; } = null!;

        public Nganh Nganh { get; set; } = null!;
    }
}