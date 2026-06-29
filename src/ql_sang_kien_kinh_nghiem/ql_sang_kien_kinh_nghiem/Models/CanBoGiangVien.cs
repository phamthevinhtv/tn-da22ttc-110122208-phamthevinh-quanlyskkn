namespace ql_sang_kien_kinh_nghiem.Models
{
    public class CanBoGiangVien
    {
        public string MaCBGV { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public TaiKhoan? TaiKhoan { get; set; }

        public ICollection<CBGVChucVuDonVi> DSCBGVChucVuDonVi { get; set; } = new List<CBGVChucVuDonVi>();

        public ICollection<CBGVTrinhDoNganh> DSCBGVTrinhDoNganh { get; set; } = new List<CBGVTrinhDoNganh>();

        public ICollection<CBGVSangKien> DSCBGVSangKien { get; set; } = new List<CBGVSangKien>();

        public ICollection<CBGVVaiTroHoiDong> DSCBGVVaiTroHoiDong { get; set; } = new HashSet<CBGVVaiTroHoiDong>();

        public ICollection<DanhGia> DSDanhGia { get; set; } = new List<DanhGia>();

        public ICollection<SangKien> DSSangKien { get; set; } = new List<SangKien>();

        public ICollection<PhanHoi> DSPhanHoi { get; set; } = new List<PhanHoi>();
    }
}