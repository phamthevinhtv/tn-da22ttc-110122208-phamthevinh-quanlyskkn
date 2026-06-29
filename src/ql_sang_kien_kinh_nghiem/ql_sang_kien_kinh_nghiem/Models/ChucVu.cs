namespace ql_sang_kien_kinh_nghiem.Models
{
    public class ChucVu
    {
        public int MaCV { get; set; }

        public string TenCV { get; set; } = string.Empty;

        public ICollection<CBGVChucVuDonVi> DSCBGVChucVuDonVi { get; set; } = new List<CBGVChucVuDonVi>();
    }
}