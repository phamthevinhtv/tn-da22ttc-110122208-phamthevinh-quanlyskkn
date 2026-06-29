namespace ql_sang_kien_kinh_nghiem.Models
{
    public class SangKien
    {
        public int MaSK { get; set; }

        public int? MaCDT { get; set; }

        public string MaCBGV { get; set; } = string.Empty;

        public string TenSK { get; set; } = string.Empty;

        public DateTime NgayApDung { get; set; }

        public string ThucTrangTruoc { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        public string KhaNangApDung { get; set; } = string.Empty;

        public string LoiIchDuKien { get; set; } = string.Empty;

        public string DieuKienCanThiet { get; set; } = string.Empty;

        public string? ThongTinBaoMat { get; set; }

        public DateTime NgayNop { get; set; }

        public DateTime? NgayYeuCauChinhSua { get; set; }

        public DateTime? NgayChapNhanDanhGia { get; set; }

        public DateTime? NgayTraKetQuaDanhGia { get; set; }

        public DateTime? NgayYeuCauPhucKhao { get; set; }

        public DateTime? NgayChapNhanPhucKhao { get; set; }

        public DateTime? NgayTraKetQuaPhucKhao { get; set; }

        public string? UrlTepDinhKem { get; set; }

        public string? UrlTepSK { get; set; }

        public string TrangThaiSK { get; set; } = string.Empty;

        public ChuDauTu? ChuDauTu { get; set; }

        public CanBoGiangVien CanBoGiangVien { get; set; } = null!;

        public ICollection<LinhVucSangKien> DSLinhVucSangKien { get; set; } = new List<LinhVucSangKien>();

        public ICollection<MonHocSangKien> DSMonHocSangKien { get; set; } = new List<MonHocSangKien>();

        public ICollection<CBGVSangKien> DSCBGVSangKien { get; set; } = new List<CBGVSangKien>();

        public ICollection<DanhGia> DSDanhGia { get; set; } = new List<DanhGia>();

        public ICollection<PhanHoi> DSPhanHoi { get; set; } = new List<PhanHoi>();
    }
}