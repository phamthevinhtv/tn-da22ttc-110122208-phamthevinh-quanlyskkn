namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class ThongKeVM
    {
        public int TotalSangKien { get; set; }
        public int TotalGiangVien { get; set; }
        public int TotalHoiDong { get; set; }
        public int TotalDonVi { get; set; }
        public int TotalNganh { get; set; }
        public int TotalChuDauTu { get; set; }

        public int TotalSangKienChoDuyet { get; set; }
        public int TotalSangKienYeuCauChinhSua { get; set; }
        public int TotalSangKienDaCongNhan { get; set; }
        public int TotalSangKienKhongCongNhan { get; set; }
        public int TotalSangKienYeuCauPhucKhao { get; set; }
        public int TotalSangKienChoPhucKhao { get; set; }
        public double PhanTramCongNhan { get; set; }

        public List<string> TrangThaiLabels { get; set; } = new();
        public List<int> TrangThaiValues { get; set; } = new();

        public List<string> TopDonViLabels { get; set; } = new();
        public List<int> TopDonViValues { get; set; } = new();

        public List<string> TopGiangVienLabels { get; set; } = new();
        public List<int> TopGiangVienValues { get; set; } = new();

        public List<string> TopMonHocLabels { get; set; } = new();
        public List<int> TopMonHocValues { get; set; } = new();

        public List<string> ThangLabels { get; set; } = new();
        public List<int> ThangNopValues { get; set; } = new();
        public List<int> ThangCongNhanValues { get; set; } = new();
    }
}
