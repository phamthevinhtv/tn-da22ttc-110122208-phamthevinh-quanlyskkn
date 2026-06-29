namespace ql_sang_kien_kinh_nghiem.Models
{
    public class TrinhDo
    {
        public int MaTD { get; set; }

        public string TenTD { get; set; } = string.Empty;

        public ICollection<CBGVTrinhDoNganh> DSCBGVTrinhDoNganh { get; set; } = new List<CBGVTrinhDoNganh>();
    }
}