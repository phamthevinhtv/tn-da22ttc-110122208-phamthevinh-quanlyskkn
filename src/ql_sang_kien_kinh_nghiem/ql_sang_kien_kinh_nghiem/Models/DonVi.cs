namespace ql_sang_kien_kinh_nghiem.Models
{
    public class DonVi
    {
        public int MaDV { get; set; }

        public string TenDV { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public int? MaDVCha { get; set; }

        public DonVi? DonViCha { get; set; }

        public ICollection<DonVi> DSDonViCon { get; set; } = new List<DonVi>();

        public ICollection<CBGVChucVuDonVi> DSCBGVChucVuDonVi { get; set; } = new List<CBGVChucVuDonVi>();
    }
}