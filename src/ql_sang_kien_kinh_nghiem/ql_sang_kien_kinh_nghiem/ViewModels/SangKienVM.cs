using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ql_sang_kien_kinh_nghiem.ViewModels
{
    public class SangKienVM
    {
        public int MaSK { get; set; }

        public int? MaCDT { get; set; }

        public string MaCBGV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên sáng kiến")]
        public string? TenSK { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày áp dụng sáng kiến lần đầu")]
        public DateTime? NgayApDung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thực trạng trước khi áp dụng sáng kiến")]
        public string? ThucTrangTruoc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung của sáng kiến")]
        public string? NoiDung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khả năng áp dụng của sáng kiến")]
        public string? KhaNangApDung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lợi ích thu được hoặc dự kiến thu được do áp dụng sáng kiến")]
        public string? LoiIchDuKien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập điều kiện cần thiết để áp dụng sáng kiến")]
        public string? DieuKienCanThiet { get; set; }

        public string? ThongTinBaoMat { get; set; }

        public string? UrlTepDinhKem { get; set; }

        public string? UrlTepSK { get; set; }

        public IFormFile? FileDinhKem { get; set; }

        public DateTime NgayNop { get; set; }

        public DateTime? NgayYeuCauChinhSua { get; set; }

        public DateTime? NgayChapNhanDanhGia { get; set; }

        public DateTime? NgayTraKetQuaDanhGia { get; set; }

        public DateTime? NgayYeuCauPhucKhao { get; set; }

        public DateTime? NgayChapNhanPhucKhao { get; set; }

        public DateTime? NgayTraKetQuaPhucKhao { get; set; }

        public DateTime? HanXetDuyet
        {
            get
            {
                if (TrangThaiSK == "CHO_DUYET" && NgayYeuCauChinhSua != null)
                {
                    return NgayYeuCauChinhSua.Value.AddDays(30);
                }

                if (TrangThaiSK == "CHO_DUYET")
                {
                    return NgayNop.AddDays(30);
                }

                return null;
            }
        }

        public DateTime? HanChinhSua {
            get
            {
                if (TrangThaiSK == "YEU_CAU_CHINH_SUA")
                {
                    return NgayYeuCauChinhSua?.AddDays(30);
                }

                return null;
            }
        }

        public DateTime? HanDanhGia
        {
            get
            {
                if (TrangThaiSK == "CHO_DANH_GIA")
                {
                    return NgayChapNhanDanhGia?.AddDays(90);
                }

                return null;
            }
        }

        public DateTime? HanGiaiQuyetPhucKhao
        {
            get
            {
                if (TrangThaiSK == "YEU_CAU_PHUC_KHAO")
                {
                    return NgayYeuCauPhucKhao?.AddDays(30);
                }

                return null;
            }
        }

        public DateTime? HanPhucKhao
        {
            get
            {
                if (TrangThaiSK == "CHO_PHUC_KHAO")
                {
                    return NgayYeuCauPhucKhao?.AddDays(90);
                }

                return null;
            }
        }

        public string TrangThaiSK { get; set; } = string.Empty;

        public string TenTrangThai => TrangThaiSK switch
        {
            "NHAP" => "Bản nháp",
            "CHO_DUYET" => "Chờ duyệt",
            "YEU_CAU_CHINH_SUA" => "Yêu cầu chỉnh sửa",
            "TU_CHOI" => "Từ chối",
            "CHO_DANH_GIA" => "Chờ đánh giá",
            "DA_CONG_NHAN" => "Đã công nhận",
            "KHONG_CONG_NHAN" => "Không công nhận",
            "YEU_CAU_PHUC_KHAO" => "Yêu cầu phúc khảo",
            "CHO_PHUC_KHAO" => "Chờ phúc khảo",

            _ => string.Empty
        };

        public IEnumerable<SelectListItem> DSChuDauTu { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DSLinhVuc { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DSMonHoc { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DSCanBoGiangVien { get; set; } = new List<SelectListItem>();

        public List<CBGVSangKienVM> DSCanBoGiangVienSK { get; set; } = new();

        [ValidateNever]
        public CBGVSangKienVM CBGVSangKienVM { get; set; } = new();

        [ValidateNever]
        public PhanHoiVM PhanHoiVM { get; set; } = new();

        [ValidateNever]
        public DanhGiaVM DanhGiaVM { get; set; } = new();

        public List<DanhGiaVM> DSDanhGia { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn lĩnh vực áp dụng sáng kiến")]
        public List<int> DSMaLV { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn học liên quan")]
        public List<int> DSMaMH { get; set; } = new();

        public List<PhanHoiVM> DSPhanHoi { get; set; } = new();

        public bool TatCaHoiDongDaDanhGia { get; set; }

        public bool DaDanhGiaTrongHoiDongHienTai { get; set; }

        public int DiemTimKhiem { get; set; }
    }
}
