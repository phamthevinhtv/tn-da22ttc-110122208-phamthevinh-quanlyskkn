namespace ql_sang_kien_kinh_nghiem.Models
{
    public class Nganh
    {
        public int MaNganh { get; set; }

        public string TenNganh { get; set; } = string.Empty;

        public ICollection<CBGVTrinhDoNganh> DSCBGVTrinhDoNganh { get; set; } = new List<CBGVTrinhDoNganh>();

        public ICollection<MonHoc> DSMonHoc { get; set; } = new List<MonHoc>();
    }
}