using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ql_sang_kien_kinh_nghiem.Services;
using System.Security.Claims;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class SangKienController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _appDbContext;

        private readonly XuLyVanBanService _xuLyVanBanService;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public SangKienController(ILogger<HomeController> logger, AppDbContext appDbContext, XuLyVanBanService xuLyVanBanService, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _xuLyVanBanService = xuLyVanBanService;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<List<SangKienVM>> LayDSSangKienTheoQuyen()
        {
            var quyen = User.FindFirstValue(ClaimTypes.Role);

            var maCBGV = User.FindFirstValue("MaCBGV");

            var maVaiTroHDChuoi = User.FindFirstValue("MaVaiTroHD");
            var maVaiTroHD = 0;
            if (!string.IsNullOrEmpty(maVaiTroHDChuoi))
            {
                int.TryParse(maVaiTroHDChuoi, out maVaiTroHD);
            }

            var dsSangKien = new List<SangKienVM>();

            if(quyen == "BGD")
            {
                dsSangKien = await _appDbContext.DbSetSangKien
                    .Where(x => x.TrangThaiSK != "NHAP" || 
                        x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) || 
                        x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.NgayNop)
                    .Select(x => new SangKienVM
                    {
                        MaSK = x.MaSK,
                        TenSK = x.TenSK,
                        TrangThaiSK = x.TrangThaiSK,
                        NgayNop = x.NgayNop,
                        NgayYeuCauChinhSua = x.NgayYeuCauChinhSua,
                        NgayChapNhanDanhGia = x.NgayChapNhanDanhGia,
                        NgayTraKetQuaDanhGia = x.NgayTraKetQuaDanhGia,
                        NgayYeuCauPhucKhao = x.NgayYeuCauPhucKhao,
                        NgayChapNhanPhucKhao = x.NgayChapNhanPhucKhao,
                        NgayTraKetQuaPhucKhao = x.NgayTraKetQuaPhucKhao
                    })
                    .ToListAsync();
            }

            if(quyen == "DVTT")
            {
                var tatCaDonVi = await _appDbContext.DbSetDonVi
                    .Select(x => new { x.MaDV, x.MaDVCha })
                    .ToListAsync();

                var maDVHienTai = await _appDbContext.DbSetCBGVChucVuDonVi
                    .Where(x => x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.ThoiGianBatDau)
                    .Select(x => x.MaDV)
                    .FirstOrDefaultAsync();

                var dsMaDV = new List<int>();
                void LayDonViVaCon(int maDV)
                {
                    dsMaDV.Add(maDV);
                    foreach (var dv in tatCaDonVi.Where(x => x.MaDVCha == maDV))
                        LayDonViVaCon(dv.MaDV);
                }
                if (maDVHienTai != 0) LayDonViVaCon(maDVHienTai);

                dsSangKien = await _appDbContext.DbSetSangKien
                    .Where(x => x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) || 
                        x.MaCBGV == maCBGV ||
                        (x.TrangThaiSK != "NHAP" &&
                        x.DSCBGVSangKien.Any(s =>
                            s.CanBoGiangVien.DSCBGVChucVuDonVi.Any(cvdv =>
                                dsMaDV.Contains(cvdv.MaDV)))))
                    .OrderByDescending(x => x.NgayNop)
                    .Select(x => new SangKienVM
                    {
                        MaSK = x.MaSK,
                        TenSK = x.TenSK,
                        TrangThaiSK = x.TrangThaiSK,
                        NgayNop = x.NgayNop,
                        NgayYeuCauChinhSua = x.NgayYeuCauChinhSua,
                        NgayChapNhanDanhGia = x.NgayChapNhanDanhGia,
                    })
                    .ToListAsync();
            }

            if(quyen == "DVTT" && maVaiTroHD > 0)
            {
                var tatCaDonVi = await _appDbContext.DbSetDonVi
                    .Select(x => new { x.MaDV, x.MaDVCha })
                    .ToListAsync();

                var maDVHienTai = await _appDbContext.DbSetCBGVChucVuDonVi
                    .Where(x => x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.ThoiGianBatDau)
                    .Select(x => x.MaDV)
                    .FirstOrDefaultAsync();

                var dsMaDV = new List<int>();
                void LayDonViVaCon(int maDV)
                {
                    dsMaDV.Add(maDV);
                    foreach (var dv in tatCaDonVi.Where(x => x.MaDVCha == maDV))
                        LayDonViVaCon(dv.MaDV);
                }
                if (maDVHienTai != 0) LayDonViVaCon(maDVHienTai);

                dsSangKien = await _appDbContext.DbSetSangKien
                    .Where(x => x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) ||
                        x.TrangThaiSK == "CHO_DANH_GIA" || x.TrangThaiSK == "DA_CONG_NHAN" || x.TrangThaiSK == "KHONG_CONG_NHAN" ||
                        x.TrangThaiSK == "YEU_CAU_PHUC_KHAO" || x.TrangThaiSK == "CHO_PHUC_KHAO" ||
                        x.MaCBGV == maCBGV ||
                        (x.TrangThaiSK != "NHAP" &&
                        x.DSCBGVSangKien.Any(s =>
                            s.CanBoGiangVien.DSCBGVChucVuDonVi.Any(cvdv =>
                                dsMaDV.Contains(cvdv.MaDV)))))
                    .OrderByDescending(x => x.NgayNop)
                    .Select(x => new SangKienVM
                    {
                        MaSK = x.MaSK,
                        TenSK = x.TenSK,
                        TrangThaiSK = x.TrangThaiSK,
                        NgayNop = x.NgayNop,
                        NgayYeuCauChinhSua = x.NgayYeuCauChinhSua,
                        NgayChapNhanDanhGia = x.NgayChapNhanDanhGia,
                    })
                    .ToListAsync();
            }

            if(quyen == "CBGV" || quyen == "ADMIN")
            {
                dsSangKien = await _appDbContext.DbSetSangKien
                    .Where(x => x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) || 
                        x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.NgayNop)
                    .Select(x => new SangKienVM
                    {
                        MaSK = x.MaSK,
                        TenSK = x.TenSK,
                        TrangThaiSK = x.TrangThaiSK,
                        NgayNop = x.NgayNop,
                        NgayYeuCauChinhSua = x.NgayYeuCauChinhSua,
                        NgayChapNhanDanhGia = x.NgayChapNhanDanhGia,
                    })
                    .ToListAsync();
            }

            if((quyen == "CBGV" || quyen == "ADMIN") && maVaiTroHD > 0)
            {
                dsSangKien = await _appDbContext.DbSetSangKien
                    .Where(x => x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) ||
                        x.TrangThaiSK == "CHO_DANH_GIA" || 
                        x.TrangThaiSK == "DA_CONG_NHAN" || 
                        x.TrangThaiSK == "KHONG_CONG_NHAN" ||
                        x.TrangThaiSK == "YEU_CAU_PHUC_KHAO" ||
                        x.TrangThaiSK == "CHO_PHUC_KHAO" ||
                        x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.NgayNop)
                    .Select(x => new SangKienVM
                    {
                        MaSK = x.MaSK,
                        TenSK = x.TenSK,
                        TrangThaiSK = x.TrangThaiSK,
                        NgayNop = x.NgayNop,
                        NgayYeuCauChinhSua = x.NgayYeuCauChinhSua,
                        NgayChapNhanDanhGia = x.NgayChapNhanDanhGia,
                    })
                    .ToListAsync();
            }

            return dsSangKien;
        }

        private async Task<SangKienVM> LaySangKienTheoQuyen(int ma)
        {
            var quyen = User.FindFirstValue(ClaimTypes.Role);

            var maCBGV = User.FindFirstValue("MaCBGV");

            var maVaiTroHDChuoi = User.FindFirstValue("MaVaiTroHD");
            var maVaiTroHD = 0;
            if (!string.IsNullOrEmpty(maVaiTroHDChuoi))
            {
                int.TryParse(maVaiTroHDChuoi, out maVaiTroHD);
            }

            var sangKien = new SangKien();

            var sangKienVM = new SangKienVM();

            if(quyen == "BGD")
            {
                sangKien = await _appDbContext.DbSetSangKien
                    .FirstOrDefaultAsync(x => x.MaSK == ma && (x.TrangThaiSK != "NHAP" || 
                        x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) || 
                        x.MaCBGV == maCBGV));
            }

            if(quyen == "DVTT")
            {
                var tatCaDonVi = await _appDbContext.DbSetDonVi
                    .Select(x => new { x.MaDV, x.MaDVCha })
                    .ToListAsync();

                var maDVHienTai = await _appDbContext.DbSetCBGVChucVuDonVi
                    .Where(x => x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.ThoiGianBatDau)
                    .Select(x => x.MaDV)
                    .FirstOrDefaultAsync();

                var dsMaDV = new List<int>();
                void LayDonViVaCon(int maDV)
                {
                    dsMaDV.Add(maDV);
                    foreach (var dv in tatCaDonVi.Where(x => x.MaDVCha == maDV))
                        LayDonViVaCon(dv.MaDV);
                }
                if (maDVHienTai != 0) LayDonViVaCon(maDVHienTai);

                sangKien = await _appDbContext.DbSetSangKien
                    .FirstOrDefaultAsync(x => x.MaSK == ma && (x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) || 
                        x.MaCBGV == maCBGV ||
                        (x.TrangThaiSK != "NHAP" &&
                        x.DSCBGVSangKien.Any(s =>
                            s.CanBoGiangVien.DSCBGVChucVuDonVi.Any(cvdv =>
                                dsMaDV.Contains(cvdv.MaDV))))));
            }

            if(quyen == "DVTT" && maVaiTroHD > 0)
            {
                var tatCaDonVi = await _appDbContext.DbSetDonVi
                    .Select(x => new { x.MaDV, x.MaDVCha })
                    .ToListAsync();

                var maDVHienTai = await _appDbContext.DbSetCBGVChucVuDonVi
                    .Where(x => x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.ThoiGianBatDau)
                    .Select(x => x.MaDV)
                    .FirstOrDefaultAsync();

                var dsMaDV = new List<int>();
                void LayDonViVaCon(int maDV)
                {
                    dsMaDV.Add(maDV);
                    foreach (var dv in tatCaDonVi.Where(x => x.MaDVCha == maDV))
                        LayDonViVaCon(dv.MaDV);
                }
                if (maDVHienTai != 0) LayDonViVaCon(maDVHienTai);

                sangKien = await _appDbContext.DbSetSangKien
                    .FirstOrDefaultAsync(x => x.MaSK == ma && (x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) || 
                        x.MaCBGV == maCBGV ||
                        x.TrangThaiSK == "CHO_DANH_GIA" ||
                        x.TrangThaiSK == "DA_CONG_NHAN" ||
                        x.TrangThaiSK == "KHONG_CONG_NHAN" ||
                        x.TrangThaiSK == "YEU_CAU_PHUC_KHAO" ||
                        x.TrangThaiSK == "CHO_PHUC_KHAO" ||
                        (x.TrangThaiSK != "NHAP" &&
                        x.DSCBGVSangKien.Any(s =>
                            s.CanBoGiangVien.DSCBGVChucVuDonVi.Any(cvdv =>
                                dsMaDV.Contains(cvdv.MaDV))))));
            }

            if(quyen == "CBGV" || quyen == "ADMIN")
            {
                sangKien = await _appDbContext.DbSetSangKien
                    .FirstOrDefaultAsync(x => x.MaSK == ma && (x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) || 
                        x.MaCBGV == maCBGV));
            }

            if((quyen == "CBGV" || quyen == "ADMIN") && maVaiTroHD > 0)
            {
                sangKien = await _appDbContext.DbSetSangKien
                    .FirstOrDefaultAsync(x => x.MaSK == ma && (x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) || 
                        x.TrangThaiSK == "CHO_DANH_GIA" ||
                        x.TrangThaiSK == "YEU_CAU_PHUC_KHAO" ||
                        x.TrangThaiSK == "CHO_PHUC_KHAO" ||
                        x.MaCBGV == maCBGV));
            }

            if (sangKien != null)
            {
                var dSMaLV = await _appDbContext.DbSetLinhVucSangKien
                    .Where(x => x.MaSK == ma)      
                    .Select(x => x.MaLV)
                    .ToListAsync();

                var dSMaMH = await _appDbContext.DbSetMonHocSangKien
                    .Where(x => x.MaSK == ma)      
                    .Select(x => x.MaMH)
                    .ToListAsync();

                var danhGiaCuaToi = await _appDbContext.DbSetDanhGia
                    .Where(x => x.MaSK == ma && x.MaCBGV == maCBGV)
                    .Select(x => new DanhGiaVM
                    {
                        MaSK = x.MaSK,
                        MaCBGV = x.MaCBGV,
                        MaHD = x.MaHD,
                        YKienNhanXet = x.YKienNhanXet,
                        CongNhan = x.CongNhan,
                        NgayDG = x.NgayDG
                    })
                    .FirstOrDefaultAsync();
                
                sangKienVM.MaSK = sangKien.MaSK;
                sangKienVM.MaCDT = sangKien.MaCDT;
                sangKienVM.MaCBGV = sangKien.MaCBGV;
                sangKienVM.TenSK = sangKien.TenSK;
                sangKienVM.NgayApDung = sangKien.NgayApDung;
                sangKienVM.ThucTrangTruoc = sangKien.ThucTrangTruoc;
                sangKienVM.NoiDung = sangKien.NoiDung;
                sangKienVM.KhaNangApDung = sangKien.KhaNangApDung;
                sangKienVM.LoiIchDuKien = sangKien.LoiIchDuKien;
                sangKienVM.DieuKienCanThiet = sangKien.DieuKienCanThiet;
                sangKienVM.ThongTinBaoMat = sangKien.ThongTinBaoMat;
                sangKienVM.UrlTepDinhKem = sangKien.UrlTepDinhKem;
                sangKienVM.UrlTepSK = sangKien.UrlTepSK;
                sangKienVM.NgayNop = sangKien.NgayNop;
                sangKienVM.NgayYeuCauChinhSua = sangKien.NgayYeuCauChinhSua;
                sangKienVM.NgayChapNhanDanhGia = sangKien.NgayChapNhanDanhGia;
                sangKienVM.TrangThaiSK = sangKien.TrangThaiSK;
                sangKienVM.DSMaLV = dSMaLV;
                sangKienVM.DSMaMH = dSMaMH;
                sangKienVM.DanhGiaVM = danhGiaCuaToi ?? new DanhGiaVM();

                if (!string.IsNullOrEmpty(sangKienVM.UrlTepSK))
                {
                    string filePath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        sangKienVM.UrlTepSK.TrimStart('/')
                    );

                    if (!System.IO.File.Exists(filePath))
                    {
                        sangKienVM.UrlTepSK = null;
                    }
                }

                var dSCBGV = await _appDbContext.DbSetCBGVSangKien
                .Where(x => x.MaSK == ma)
                .Select(x => new CBGVSangKienVM
                {
                    MaCBGV = x.MaCBGV,
                    MaSK = x.MaSK,
                    HoTen = x.CanBoGiangVien.HoTen,
                    Email = x.CanBoGiangVien.Email,
                    SoDienThoai = x.CanBoGiangVien.SoDienThoai,
                    TiLeDongGop = x.TiLeDongGop,
                })
                .ToListAsync();

                var chucVuDonVi = await _appDbContext.DbSetCBGVChucVuDonVi
                    .Where(x => dSCBGV.Select(d => d.MaCBGV).Contains(x.MaCBGV))
                    .Select(x => new CBGVChucVuDonViVM
                    {
                        MaCBGV = x.MaCBGV,
                        MaCV = x.MaCV,
                        MaDV = x.MaDV,
                        TenCV = x.ChucVu.TenCV,
                        TenDV = x.DonVi.TenDV,
                        ThoiGianBatDau = x.ThoiGianBatDau,
                        ThoiGianKetThuc = x.ThoiGianKetThuc
                    })
                    .ToListAsync();

                var trinhDoNganh = await _appDbContext.DbSetCBGVTrinhDoNganh
                    .Where(x => dSCBGV.Select(d => d.MaCBGV).Contains(x.MaCBGV))
                    .Select(x => new CBGVTrinhDoNganhVM
                    {
                        MaCBGV = x.MaCBGV,
                        TenTD = x.TrinhDo.TenTD,
                        TenNganh = x.Nganh.TenNganh
                    })
                    .ToListAsync();

                foreach (var cbgv in dSCBGV)
                {
                    cbgv.DSChucVuDonVi = chucVuDonVi
                        .Where(x => x.MaCBGV == cbgv.MaCBGV)
                        .Select(x => new CBGVChucVuDonViVM
                        {
                            MaCBGV = x.MaCBGV,
                            MaCV = x.MaCV,
                            MaDV = x.MaDV,
                            TenCV = x.TenCV,
                            TenDV = x.TenDV,
                            ThoiGianBatDau = x.ThoiGianBatDau,
                            ThoiGianKetThuc = x.ThoiGianKetThuc
                        })
                        .ToList();

                    cbgv.DSTrinhDoNganh = trinhDoNganh
                        .Where(x => x.MaCBGV == cbgv.MaCBGV)
                        .Select(x => new CBGVTrinhDoNganhVM
                        {
                            MaCBGV = x.MaCBGV,
                            TenTD = x.TenTD,
                            TenNganh = x.TenNganh
                        })
                        .ToList();
                }

                sangKienVM.DSPhanHoi = await _appDbContext.DbSetPhanHoi
                    .Where(x => x.MaSK == ma)
                    .OrderByDescending(x => x.NgayGui)
                    .Select(x => new PhanHoiVM
                    {
                        MaSK = x.MaSK,
                        MaCBGV = x.MaCBGV,
                        NoiDung = x.NoiDung,
                        NgayGui = x.NgayGui,
                        TenCBGV = x.CanBoGiangVien.HoTen,
                        TenDV = x.CanBoGiangVien.DSCBGVChucVuDonVi
                            .OrderByDescending(c => c.ThoiGianBatDau)
                            .Select(c => c.DonVi.TenDV)
                            .FirstOrDefault() ?? string.Empty
                    })
                    .ToListAsync();

                sangKienVM.DSDanhGia = await _appDbContext.DbSetDanhGia
                    .Where(x => x.MaSK == ma)
                    .OrderByDescending(x => x.NgayDG)
                    .Select(x => new DanhGiaVM
                    {
                        MaSK = x.MaSK,
                        MaCBGV = x.MaCBGV,
                        YKienNhanXet = x.YKienNhanXet,
                        CongNhan = x.CongNhan,
                        NgayDG = x.NgayDG,
                        LanDG = x.LanDG,
                        TenCBGV = x.CanBoGiangVien.HoTen
                    })
                    .ToListAsync();

                var activeHoiDong = await _appDbContext.DbSetHoiDong
                    .Where(h => h.NgayLap <= DateTime.Now && h.NgayKetThuc >= DateTime.Now)
                    .Select(h => new
                    {
                        h.MaHD,
                        SoThanhVien = h.DSCBGVVaiTroHoiDong.Count()
                    })
                    .FirstOrDefaultAsync();

                if (activeHoiDong != null && activeHoiDong.SoThanhVien > 0)
                {
                    var soDanhGiaHoiDong = await _appDbContext.DbSetDanhGia
                        .Where(d => d.MaSK == ma && d.MaHD == activeHoiDong.MaHD)
                        .Select(d => d.MaCBGV)
                        .Distinct()
                        .CountAsync();

                    var dsMaCBGVHoiDong = await _appDbContext.DbSetCBGVVaiTroHoiDong
                        .Where(vt => vt.MaHD == activeHoiDong.MaHD)
                        .Select(vt => vt.MaCBGV)
                        .Distinct()
                        .ToListAsync();

                    var dsMaCBGVSangKien = dSCBGV
                        .Select(x => x.MaCBGV)
                        .Distinct()
                        .ToList();

                    var soThanhVienHoiDongCoTheDanhGia = dsMaCBGVHoiDong
                        .Count(ma => !dsMaCBGVSangKien.Contains(ma));

                    sangKienVM.TatCaHoiDongDaDanhGia = soThanhVienHoiDongCoTheDanhGia > 0 && soDanhGiaHoiDong >= soThanhVienHoiDongCoTheDanhGia;

                    var maCBGVHienTai = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("MaCBGV");
                    sangKienVM.DaDanhGiaTrongHoiDongHienTai = await _appDbContext.DbSetDanhGia
                        .AnyAsync(d => d.MaSK == ma && d.MaHD == activeHoiDong.MaHD && d.MaCBGV == maCBGVHienTai);
                }

                sangKienVM.DSCanBoGiangVienSK = dSCBGV;
            }

            return sangKienVM;
        }

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dsSangKien = await LayDSSangKienTheoQuyen();

            return View(dsSangKien);
        }

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int? ma)
        {
            var sangKienVM = new SangKienVM();

            if (ma != null)
            {
                sangKienVM = await LaySangKienTheoQuyen(ma.Value);

                if (sangKienVM.MaSK <= 0)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }
            }

            sangKienVM.DSChuDauTu = await _appDbContext.DbSetChuDauTu
                .OrderBy(x => x.MaCDT)
                .Select(x => new SelectListItem
                {
                    Value = x.MaCDT.ToString(),
                    Text = $"{x.MaCDT} - {x.TenCDT}"
                })
                .ToListAsync();

            sangKienVM.DSLinhVuc = await _appDbContext.DbSetLinhVuc
                .OrderBy(x => x.MaLV)
                .Select(x => new SelectListItem
                {
                    Value = x.MaLV.ToString(),
                    Text = x.TenLV
                })
                .ToListAsync();

            sangKienVM.DSMonHoc = await _appDbContext.DbSetMonHoc
                .OrderBy(x => x.MaMH)
                .Select(x => new SelectListItem
                {
                    Value = x.MaMH.ToString(),
                    Text = x.TenMH
                })
                .ToListAsync();

            sangKienVM.DSCanBoGiangVien = await _appDbContext.DbSetCanBoGiangVien
                .Where(x => !x.DSCBGVSangKien.Any(cbgvsk => cbgvsk.MaSK == ma))
                .OrderBy(x => x.MaCBGV)
                .Select(x => new SelectListItem
                {
                    Value = x.MaCBGV.ToString(),
                    Text = $"{x.MaCBGV} - {x.HoTen}"
                })
                .ToListAsync();

            return View(sangKienVM);
        }

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(SangKienVM sangKienVM)
        {
            try
            {
                if (sangKienVM.NgayApDung > DateTime.Today)
                {
                    ModelState.AddModelError(
                        nameof(sangKienVM.NgayApDung),
                        "Ngày áp dụng không được sau ngày hiện tại"
                    );
                }
                else if (sangKienVM.NgayApDung < DateTime.Today.AddYears(-1))
                {
                    ModelState.AddModelError(
                        nameof(sangKienVM.NgayApDung),
                        "Ngày áp dụng không được quá 01 năm tính đến ngày hiện tại"
                    );
                }

                if (!ModelState.IsValid)
                {
                    sangKienVM.DSLinhVuc = await _appDbContext.DbSetLinhVuc
                        .OrderBy(x => x.MaLV)
                        .Select(x => new SelectListItem
                        {
                            Value = x.MaLV.ToString(),
                            Text = x.TenLV
                        })
                        .ToListAsync();

                    sangKienVM.DSMonHoc = await _appDbContext.DbSetMonHoc
                        .OrderBy(x => x.MaMH)
                        .Select(x => new SelectListItem
                        {
                            Value = x.MaMH.ToString(),
                            Text = x.TenMH
                        })
                        .ToListAsync();

                    sangKienVM.DSChuDauTu = await _appDbContext.DbSetChuDauTu
                        .OrderBy(x => x.MaCDT)
                        .Select(x => new SelectListItem
                        {
                            Value = x.MaCDT.ToString(),
                            Text = $"{x.TenCDT} - {x.MaCDT}"
                        })
                        .ToListAsync();

                    sangKienVM.DSCanBoGiangVien = await _appDbContext.DbSetCanBoGiangVien
                        .OrderBy(x => x.MaCBGV)
                        .Select(x => new SelectListItem
                        {
                            Value = x.MaCBGV.ToString(),
                            Text = $"{x.HoTen} - {x.MaCBGV}"
                        })
                        .ToListAsync();

                    var dSCBGV = await _appDbContext.DbSetCBGVSangKien
                    .Where(x => x.MaSK == sangKienVM.MaSK)
                    .Select(x => new CBGVSangKienVM
                    {
                        MaCBGV = x.MaCBGV,
                        MaSK = x.MaSK,
                        HoTen = x.CanBoGiangVien.HoTen,
                        Email = x.CanBoGiangVien.Email,
                        SoDienThoai = x.CanBoGiangVien.SoDienThoai,
                        TiLeDongGop = x.TiLeDongGop,
                    })
                    .ToListAsync();

                    var chucVuDonVi = await _appDbContext.DbSetCBGVChucVuDonVi
                        .Where(x => dSCBGV.Select(d => d.MaCBGV).Contains(x.MaCBGV))
                        .Select(x => new CBGVChucVuDonViVM
                        {
                            MaCBGV = x.MaCBGV,
                            MaCV = x.MaCV,
                            MaDV = x.MaDV,
                            TenCV = x.ChucVu.TenCV,
                            TenDV = x.DonVi.TenDV,
                            ThoiGianBatDau = x.ThoiGianBatDau,
                            ThoiGianKetThuc = x.ThoiGianKetThuc
                        })
                        .ToListAsync();

                    var trinhDoNganh = await _appDbContext.DbSetCBGVTrinhDoNganh
                        .Where(x => dSCBGV.Select(d => d.MaCBGV).Contains(x.MaCBGV))
                        .Select(x => new CBGVTrinhDoNganhVM
                        {
                            MaCBGV = x.MaCBGV,
                            TenTD = x.TrinhDo.TenTD,
                            TenNganh = x.Nganh.TenNganh
                        })
                        .ToListAsync();

                    foreach (var cbgv in dSCBGV)
                    {
                        cbgv.DSChucVuDonVi = chucVuDonVi
                            .Where(x => x.MaCBGV == cbgv.MaCBGV)
                            .Select(x => new CBGVChucVuDonViVM
                            {
                                MaCBGV = x.MaCBGV,
                                MaCV = x.MaCV,
                                MaDV = x.MaDV,
                                TenCV = x.TenCV,
                                TenDV = x.TenDV,
                                ThoiGianBatDau = x.ThoiGianBatDau,
                                ThoiGianKetThuc = x.ThoiGianKetThuc
                            })
                            .ToList();

                        cbgv.DSTrinhDoNganh = trinhDoNganh
                            .Where(x => x.MaCBGV == cbgv.MaCBGV)
                            .Select(x => new CBGVTrinhDoNganhVM
                            {
                                MaCBGV = x.MaCBGV,
                                TenTD = x.TenTD,
                                TenNganh = x.TenNganh
                            })
                            .ToList();
                    }

                    sangKienVM.DSCanBoGiangVienSK = dSCBGV;
                        
                    return View(sangKienVM);
                }

                var dSLinhVucSangKien = sangKienVM.DSMaLV != null 
                    ? sangKienVM.DSMaLV.Select(maLV => new LinhVucSangKien
                    {
                        MaLV = maLV
                    }).ToList()
                    : new List<LinhVucSangKien>();

                var dSMonHocSangKien = sangKienVM.DSMaMH != null 
                    ? sangKienVM.DSMaMH.Select(maMH => new MonHocSangKien
                    {
                        MaMH = maMH
                    }).ToList()
                    : new List<MonHocSangKien>();

                string tenSK = sangKienVM.TenSK!.Trim();
                int? maCDT = sangKienVM.MaCDT;
                string maCBGV = User.FindFirst("MaCBGV")?.Value ?? "";
                string thucTrangTruoc = sangKienVM.ThucTrangTruoc!.Trim();
                string noiDung = sangKienVM.NoiDung!.Trim();
                string khaNangApDung = sangKienVM.KhaNangApDung!.Trim();
                string loiIch = sangKienVM.LoiIchDuKien!.Trim();
                string dieuKienCanThiet = sangKienVM.DieuKienCanThiet!.Trim();
                string? thongTinBaoMat = sangKienVM.ThongTinBaoMat?.Trim();

                string? urlFile = null;

                if (sangKienVM.FileDinhKem != null && sangKienVM.FileDinhKem.Length > 0)
                {
                    var allowedExtensions = new[]
                    {
                        ".pdf",
                        ".doc",
                        ".docx"
                    };

                    var extension = Path
                        .GetExtension(sangKienVM.FileDinhKem.FileName)
                        .ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError(
                            nameof(sangKienVM.FileDinhKem),
                            "Chỉ chấp nhận file PDF hoặc Word"
                        );

                        return View(sangKienVM);
                    }

                    if (sangKienVM.FileDinhKem.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            nameof(sangKienVM.FileDinhKem),
                            "File tối đa 10MB"
                        );

                        return View(sangKienVM);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;

                    var uploadPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/uploads"
                    );

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await sangKienVM.FileDinhKem.CopyToAsync(stream);
                    }

                    urlFile = "/uploads/" + fileName;
                }

                if (sangKienVM.MaSK <= 0)
                {
                    var sangKien = new SangKien
                    {
                        MaCDT = maCDT,
                        MaCBGV = maCBGV,
                        TenSK = tenSK,
                        NgayApDung = sangKienVM.NgayApDung!.Value,
                        ThucTrangTruoc = thucTrangTruoc,
                        NoiDung = noiDung,
                        KhaNangApDung = khaNangApDung,
                        LoiIchDuKien = loiIch,
                        DieuKienCanThiet = dieuKienCanThiet,
                        ThongTinBaoMat = thongTinBaoMat,
                        UrlTepDinhKem = urlFile,
                        DSLinhVucSangKien = dSLinhVucSangKien,
                        DSMonHocSangKien = dSMonHocSangKien,
                        TrangThaiSK = "NHAP",
                        NgayNop = DateTime.Now,
                    };

                    _appDbContext.DbSetSangKien.Add(sangKien);

                    await _appDbContext.SaveChangesAsync();

                    ToPDF(sangKien.MaSK);

                    TempData["Success"] = "Thêm sáng kiến thành công";

                    TempData["ModalThemThanhVien"] = "Mo";

                    return RedirectToAction(nameof(ChiTiet), new { ma = sangKien.MaSK });
                }

                else
                {
                    var sangKien = await _appDbContext.DbSetSangKien
                        .Include(s => s.DSLinhVucSangKien) 
                        .Include(s => s.DSMonHocSangKien) 
                        .FirstOrDefaultAsync(s => s.MaSK == sangKienVM.MaSK);

                    if (sangKien == null)
                    {
                        TempData["Error"] = "Sáng kiến không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    sangKien.MaCDT = maCDT;
                    sangKien.TenSK = tenSK;
                    sangKien.NgayApDung = sangKienVM.NgayApDung!.Value;
                    sangKien.ThucTrangTruoc = thucTrangTruoc;
                    sangKien.NoiDung = noiDung;
                    sangKien.KhaNangApDung = khaNangApDung;
                    sangKien.LoiIchDuKien = loiIch;
                    sangKien.DieuKienCanThiet = dieuKienCanThiet;
                    sangKien.ThongTinBaoMat = thongTinBaoMat;
                    sangKien.UrlTepDinhKem = urlFile ?? sangKien.UrlTepDinhKem;

                    sangKien.DSLinhVucSangKien.Clear(); 
                    if (dSLinhVucSangKien != null)
                    {
                        foreach (var lv in dSLinhVucSangKien)
                        {
                            sangKien.DSLinhVucSangKien.Add(lv); 
                        }
                    }

                    sangKien.DSMonHocSangKien.Clear(); 
                    if (dSMonHocSangKien != null)
                    {
                        foreach (var mh in dSMonHocSangKien)
                        {
                            sangKien.DSMonHocSangKien.Add(mh); 
                        }
                    }

                    await _appDbContext.SaveChangesAsync();
                
                    TempData["Success"] = "Cập nhật sáng kiến thành công";

                    ToPDF(sangKien.MaSK);

                    return RedirectToAction(nameof(ChiTiet), new { ma = sangKienVM.MaSK });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(sangKienVM);
            }
        }

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien.FindAsync(ma);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool laBanNhap = sangKien.TrangThaiSK == "NHAP";

                if (!laBanNhap)
                {
                    TempData["Error"] = "Chỉ có thể xóa sáng kiến ở trạng thái nháp";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetSangKien.Remove(sangKien);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa sáng kiến thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Nop(int ma)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien
                    .Include(s => s.DSCBGVSangKien)
                    .FirstOrDefaultAsync(s => s.MaSK == ma);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool coTheNop = (sangKien.TrangThaiSK == "NHAP" || sangKien.TrangThaiSK == "YEU_CAU_CHINH_SUA") && (sangKien.DSCBGVSangKien?.Count ?? 0) > 0;

                if (!coTheNop)
                {
                    TempData["Error"] = "Chỉ có thể nộp sáng kiến ở trạng thái nháp hoặc yêu cầu chỉnh sửa";
                    return RedirectToAction(nameof(Index));
                }

                sangKien.TrangThaiSK = "CHO_DUYET";
                sangKien.NgayNop = DateTime.Now;

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Nộp sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = sangKien.MaSK });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "DVTT")]
        [HttpPost]
        public async Task<IActionResult> ChapNhan(int ma)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien
                    .Include(s => s.DSCBGVSangKien)
                    .FirstOrDefaultAsync(s => s.MaSK == ma);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                string maCBGV = User.FindFirst("MaCBGV")?.Value ?? "";

                bool coTheChapNhan = sangKien.TrangThaiSK == "CHO_DUYET" && (sangKien.DSCBGVSangKien?.Count ?? 0) > 0 && sangKien.MaCBGV != maCBGV;

                if (!coTheChapNhan)
                {
                    TempData["Error"] = "Không thể chấp nhận sáng kiến";
                    return RedirectToAction(nameof(Index));
                }

                sangKien.TrangThaiSK = "CHO_DANH_GIA";
                sangKien.NgayChapNhanDanhGia = DateTime.Now;
                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Chấp nhận sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = sangKien.MaSK });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "DVTT")]
        [HttpPost]
        public async Task<IActionResult> YeuCauChinhSua(PhanHoiVM phanHoiVM)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien
                    .Include(s => s.DSCBGVSangKien)
                    .FirstOrDefaultAsync(s => s.MaSK == phanHoiVM.MaSK);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                string noiDungPhanHoi = phanHoiVM.NoiDung.Trim();
                string maCBGV = User.FindFirst("MaCBGV")?.Value ?? "";

                bool coTheYeuCauChinhSua = sangKien.TrangThaiSK == "CHO_DUYET" && (sangKien.DSCBGVSangKien?.Count ?? 0) > 0 && sangKien.MaCBGV != maCBGV;

                if (!coTheYeuCauChinhSua)
                {
                    TempData["Error"] = "Không thể yêu cầu chỉnh sửa sáng kiến";
                    return RedirectToAction(nameof(Index));
                }

                var phanHoi = new PhanHoi
                {
                    MaCBGV = maCBGV,
                    MaSK = phanHoiVM.MaSK,
                    NoiDung = noiDungPhanHoi,
                    NgayGui = DateTime.Now
                };

                _appDbContext.DbSetPhanHoi.Add(phanHoi);

                sangKien.TrangThaiSK = "YEU_CAU_CHINH_SUA";
                sangKien.NgayYeuCauChinhSua = DateTime.Now;

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Yêu cầu chỉnh sửa sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = sangKien.MaSK });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
                _logger.LogError(ex, "Lỗi khi yêu cầu chỉnh sửa sáng kiến");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "DVTT")]
        public async Task<IActionResult> TuChoi(PhanHoiVM phanHoiVM)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien
                    .Include(s => s.DSCBGVSangKien)
                    .FirstOrDefaultAsync(s => s.MaSK == phanHoiVM.MaSK);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                string noiDungPhanHoi = phanHoiVM.NoiDung.Trim();
                string maCBGV = User.FindFirst("MaCBGV")?.Value ?? "";

                bool coTheTuChoi = sangKien.TrangThaiSK == "YEU_CAU_CHINH_SUA" && (sangKien.DSCBGVSangKien?.Count ?? 0) > 0 && sangKien.MaCBGV != maCBGV && sangKien.NgayYeuCauChinhSua.HasValue && sangKien.NgayYeuCauChinhSua.Value.AddDays(30) < DateTime.Now;

                if (!coTheTuChoi)
                {
                    TempData["Error"] = "Không thể từ chối sáng kiến";
                    return RedirectToAction(nameof(Index));
                }

                var phanHoi = new PhanHoi
                {
                    MaCBGV = maCBGV,
                    MaSK = phanHoiVM.MaSK,
                    NoiDung = noiDungPhanHoi,
                    NgayGui = DateTime.Now
                };

                _appDbContext.DbSetPhanHoi.Add(phanHoi);

                sangKien.TrangThaiSK = "TU_CHOI";

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Từ chối sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = sangKien.MaSK });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> YeuCauPhucKhao(PhanHoiVM phanHoiVM)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien
                    .Include(s => s.DSCBGVSangKien)
                    .FirstOrDefaultAsync(s => s.MaSK == phanHoiVM.MaSK);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                string noiDungPhanHoi = phanHoiVM.NoiDung.Trim();
                string maCBGV = User.FindFirst("MaCBGV")?.Value ?? "";

                bool coTheYeuCauPhucKhao = sangKien.TrangThaiSK == "KHONG_CONG_NHAN" && (sangKien.MaCBGV == maCBGV || sangKien.DSCBGVSangKien.Any(cbgvsk => cbgvsk.MaCBGV == maCBGV));

                if (!coTheYeuCauPhucKhao)
                {
                    TempData["Error"] = "Không thể gửi yêu cầu phúc khảo sáng kiến";
                    return RedirectToAction(nameof(Index));
                }

                var phanHoi = new PhanHoi
                {
                    MaCBGV = maCBGV,
                    MaSK = phanHoiVM.MaSK,
                    NoiDung = noiDungPhanHoi,
                    NgayGui = DateTime.Now
                };

                _appDbContext.DbSetPhanHoi.Add(phanHoi);

                sangKien.TrangThaiSK = "YEU_CAU_PHUC_KHAO";

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Gửi yêu cầu phúc khảo sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = sangKien.MaSK });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> ChapNhanPhucKhao(int ma)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien
                    .Include(s => s.DSCBGVSangKien)
                    .FirstOrDefaultAsync(s => s.MaSK == ma);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                var maCBGV = User.FindFirst("MaCBGV")?.Value ?? "";

                bool coTheChapNhanPhucKhao = sangKien.TrangThaiSK == "YEU_CAU_PHUC_KHAO" && sangKien.MaCBGV != maCBGV && !sangKien.DSCBGVSangKien.Any(cbgvsk => cbgvsk.MaCBGV == maCBGV);

                if (!coTheChapNhanPhucKhao)
                {
                    TempData["Error"] = "Không thể chấp nhận phúc khảo cho sáng kiến";
                    return RedirectToAction(nameof(Index));
                }

                sangKien.TrangThaiSK = "CHO_PHUC_KHAO";

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Chấp nhận phúc khảo cho sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = sangKien.MaSK });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "BGD, DVTT")]
        [HttpPost]
        public async Task<IActionResult> PhanHoi(PhanHoiVM phanHoiVM)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien
                    .Include(s => s.DSCBGVSangKien)
                    .FirstOrDefaultAsync(s => s.MaSK == phanHoiVM.MaSK);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                string noiDungPhanHoi = phanHoiVM.NoiDung.Trim();
                string maCBGV = User.FindFirst("MaCBGV")?.Value ?? "";

                bool coThePhanHoi = sangKien.MaCBGV != maCBGV && !sangKien.DSCBGVSangKien.Any(cbgvsk => cbgvsk.MaCBGV == maCBGV);

                if (!coThePhanHoi)
                {
                    TempData["Error"] = "Không thể phản hồi về sáng kiến";
                    return RedirectToAction(nameof(Index));
                }

                var phanHoi = new PhanHoi
                {
                    MaCBGV = maCBGV,
                    MaSK = phanHoiVM.MaSK,
                    NoiDung = noiDungPhanHoi,
                    NgayGui = DateTime.Now
                };

                _appDbContext.DbSetPhanHoi.Add(phanHoi);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Phản hồi về sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = sangKien.MaSK });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }
        
        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> LuuCBGVSK(CBGVSangKienVM cBGVSangKienVM)
        {
            try
            {
                bool isSKExist = await _appDbContext.DbSetSangKien.AnyAsync(x => x.MaSK == cBGVSangKienVM.MaSK);
                bool isCBGVExist = await _appDbContext.DbSetCanBoGiangVien.AnyAsync(x => x.MaCBGV == cBGVSangKienVM.MaCBGV);

                if (!isSKExist)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng nhập đủ thông tin";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
                }

                if (!isCBGVExist)
                {
                    TempData["Error"] = "Cán bộ/giảng viên không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
                }

                bool isExist = await _appDbContext.DbSetCBGVSangKien
                            .AnyAsync(x => x.MaSK == cBGVSangKienVM.MaSK &&
                            x.MaCBGV == cBGVSangKienVM.MaCBGV);
                
                if (isExist)
                {
                    TempData["Error"] = "Thành viên thực hiện sáng kiến đã tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
                }

                var tongTiLeHienTai = await _appDbContext.DbSetCBGVSangKien
                    .Where(x => x.MaSK == cBGVSangKienVM.MaSK)
                    .SumAsync(x => x.TiLeDongGop);

                if (tongTiLeHienTai + cBGVSangKienVM.TiLeDongGop!.Value > 100)
                {
                    TempData["Error"] = $"Tổng tỉ lệ đóng góp vượt quá 100% (hiện tại: {tongTiLeHienTai}%, thêm: {cBGVSangKienVM.TiLeDongGop.Value}%)";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
                }

                var cBGVSangKien = new CBGVSangKien
                {
                    MaSK = cBGVSangKienVM.MaSK!.Value,
                    MaCBGV = cBGVSangKienVM.MaCBGV!,
                    TiLeDongGop = cBGVSangKienVM.TiLeDongGop!.Value
                };

                _appDbContext.DbSetCBGVSangKien.Add(cBGVSangKien);

                await _appDbContext.SaveChangesAsync();

                ToPDF(cBGVSangKienVM.MaSK!.Value);

                TempData["Success"] = "Thêm thành viên thực hiện sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
        }

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> XoaCBGVSK(CBGVSangKienVM cBGVSangKienVM)
        {
            try
            {
                var cBGVSangKien = await _appDbContext.DbSetCBGVSangKien.FirstOrDefaultAsync(x => x.MaSK == cBGVSangKienVM.MaSK && x.MaCBGV == cBGVSangKienVM.MaCBGV);
        

                if (cBGVSangKien == null)
                {
                    TempData["Error"] = "Thành viên thực hiện sáng kiến không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
                }

                _appDbContext.DbSetCBGVSangKien.Remove(cBGVSangKien);

                await _appDbContext.SaveChangesAsync();

                ToPDF(cBGVSangKienVM.MaSK!.Value);

                TempData["Success"] = "Xóa thành viên thực hiện sáng kiến thành công";
            } catch 
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
        }  

        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> SuaTiLe(CBGVSangKienVM cBGVSangKienVM)
        {
            try
            {
                var cBGVSangKien = await _appDbContext.DbSetCBGVSangKien.FirstOrDefaultAsync(x => x.MaSK == cBGVSangKienVM.MaSK && x.MaCBGV == cBGVSangKienVM.MaCBGV);
        

                if (cBGVSangKien == null)
                {
                    TempData["Error"] = "Thành viên thực hiện sáng kiến không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
                }

                var tongTiLeKhacTruoc = await _appDbContext.DbSetCBGVSangKien
                    .Where(x => x.MaSK == cBGVSangKienVM.MaSK && x.MaCBGV != cBGVSangKienVM.MaCBGV)
                    .SumAsync(x => x.TiLeDongGop);

                if (tongTiLeKhacTruoc + cBGVSangKienVM.TiLeDongGop!.Value > 100)
                {
                    TempData["Error"] = $"Tổng tỉ lệ đóng góp vượt quá 100% (các thành viên khác: {tongTiLeKhacTruoc}%, cập nhật: {cBGVSangKienVM.TiLeDongGop.Value}%)";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
                }

                cBGVSangKien.TiLeDongGop = cBGVSangKienVM.TiLeDongGop!.Value;

                await _appDbContext.SaveChangesAsync();

                ToPDF(cBGVSangKienVM.MaSK!.Value);

                TempData["Success"] = "Cập nhật tỉ lệ đóng góp thành công";
            } catch 
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVSangKienVM.MaSK });
        }

        [HttpPost]
        [Authorize(Roles = "CBGV, BGD, DVTT, ADMIN")]
        public async Task<IActionResult> XoaTep(int ma)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien.FindAsync(ma);

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma });
                }

                if (string.IsNullOrEmpty(sangKien.UrlTepDinhKem))
                {
                    TempData["Error"] = "Không có tệp đính kèm để xóa";
                    return RedirectToAction(nameof(ChiTiet), new { ma });
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", sangKien.UrlTepDinhKem.TrimStart('/'));

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                sangKien.UrlTepDinhKem = null;

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa tệp đính kèm thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma });
        }

        private void ToPDF(int ma)
        {
            var sangKien = _appDbContext.DbSetSangKien
                .Include(s => s.ChuDauTu)
                .FirstOrDefault(s => s.MaSK == ma);

            if (sangKien == null)
            {
                _logger.LogError("Sáng kiến không tồn tại");
                return;
            }

            var dSTenLV = _appDbContext.DbSetLinhVucSangKien
                .Where(x => x.MaSK == ma)
                .Select(x => x.LinhVuc.TenLV)
                .ToList();

            var dSCBGV = _appDbContext.DbSetCBGVSangKien
                .Where(x => x.MaSK == ma)
                .Select(x => new CBGVSangKienVM
                {
                    MaCBGV = x.MaCBGV,
                    MaSK = x.MaSK,
                    HoTen = x.CanBoGiangVien.HoTen,
                    Email = x.CanBoGiangVien.Email,
                    SoDienThoai = x.CanBoGiangVien.SoDienThoai,
                    TiLeDongGop = x.TiLeDongGop,
                })
                .ToList();

            var maCBGVList = dSCBGV.Select(d => d.MaCBGV).ToList();

            var chucVuDonVi = _appDbContext.DbSetCBGVChucVuDonVi
                .Where(x => maCBGVList.Contains(x.MaCBGV))
                .Select(x => new CBGVChucVuDonViVM
                {
                    MaCBGV = x.MaCBGV,
                    MaCV = x.MaCV,
                    TenCV = x.ChucVu.TenCV,
                    TenDV = x.DonVi.TenDV,
                    ThoiGianBatDau = x.ThoiGianBatDau,
                    ThoiGianKetThuc = x.ThoiGianKetThuc
                })
                .ToList();

            var trinhDoNganh = _appDbContext.DbSetCBGVTrinhDoNganh
                .Where(x => maCBGVList.Contains(x.MaCBGV))
                .Select(x => new CBGVTrinhDoNganhVM
                {
                    MaCBGV = x.MaCBGV,
                    TenTD = x.TrinhDo.TenTD,
                    TenNganh = x.Nganh.TenNganh
                })
                .ToList();

            var rows = dSCBGV.Select((x, index) =>
            {
                var chucVuMoiNhat = chucVuDonVi
                    .Where(c => c.MaCBGV == x.MaCBGV)
                    .OrderByDescending(c => c.ThoiGianBatDau)
                    .FirstOrDefault();

                var dsTrinhDo = trinhDoNganh
                    .Where(t => t.MaCBGV == x.MaCBGV)
                    .ToList();

                return new
                {
                    STT = index + 1,
                    MaCBGV = x.MaCBGV,
                    HoTen = $"{x.HoTen}\n({x.TiLeDongGop}%)",
                    ChucVu = chucVuMoiNhat == null ? "" : (chucVuMoiNhat.MaCV == 1 ? "" : chucVuMoiNhat.TenCV ?? ""),
                    DonViSDTEmail = string.Join("\n", new[]
                    {
                        chucVuMoiNhat?.TenDV ?? "",
                        x.SoDienThoai ?? "",
                        x.Email ?? ""
                    }.Where(s => !string.IsNullOrEmpty(s))),
                    TrinhDo = string.Join("\n", dsTrinhDo.Select(t => $"{t.TenTD} - {t.TenNganh}"))
                };
            }).ToList();

            string outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            string pdfPath = Path.Combine(outputFolder, $"sk_{sangKien.MaSK}.pdf");
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                const string fontBody = "Times New Roman";
                const float sizeBody = 13;
                const float sizeTitle = 14;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.MarginTop(20, Unit.Millimetre);
                        page.MarginBottom(20, Unit.Millimetre);
                        page.MarginLeft(30, Unit.Millimetre);
                        page.MarginRight(20, Unit.Millimetre);
                        page.DefaultTextStyle(x => x.FontFamily(fontBody).FontSize(sizeBody));

                        page.Content().Column(col =>
                        {
                            col.Item().AlignCenter()
                                .Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
                                .Bold().FontSize(sizeTitle);
                            col.Item().AlignCenter()
                                .BorderBottom(1)  
                                .BorderColor(Colors.Black)
                                .Text("Độc lập – Tự do – Hạnh phúc")
                                .Bold()
                                .FontSize(sizeTitle);

                            col.Item().PaddingTop(24).AlignCenter()
                                .Text("BÁO CÁO YÊU CẦU CÔNG NHẬN SÁNG KIẾN")
                                .Bold().FontSize(sizeTitle);

                            col.Item().PaddingTop(12)
                                .Text(t =>
                                {
                                    t.Span("        "); 
                                    t.Span("Kính gửi: ");
                                    t.Span("Hội đồng sáng kiến Đại học ABC");
                                });

                            col.Item().PaddingTop(6)
                                .Text(t =>
                                {
                                    t.Span("        "); 
                                    t.Span("Tôi (chúng tôi) ghi tên dưới đây:");
                                });

                            col.Item().PaddingTop(6).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(30); 
                                    c.RelativeColumn(3);    
                                    c.RelativeColumn(2);    
                                    c.RelativeColumn(3);    
                                    c.RelativeColumn(2);    
                                    c.ConstantColumn(40);  
                                });

                                static IContainer HeaderCell(IContainer c) =>
                                    c.Border(1).BorderColor(Colors.Black).Padding(2)
                                    .AlignMiddle().AlignCenter();

                                table.Header(h =>
                                {
                                    h.Cell().Element(HeaderCell).Text("TT").Bold().AlignCenter();
                                    h.Cell().Element(HeaderCell)
                                        .AlignCenter()
                                        .Text(t =>
                                        {
                                            t.Line("Họ và tên").Bold();
                                            t.Span("(Ghi rõ tỷ lệ % đóng góp)").Italic().FontSize(12);
                                        });
                                    h.Cell().Element(HeaderCell).Text("Chức vụ").Bold().AlignCenter();
                                    h.Cell().Element(HeaderCell).Text("Đơn vị và điện thoại/ Email").Bold().AlignCenter();
                                    h.Cell().Element(HeaderCell).Text("Trình độ chuyên môn").Bold().AlignCenter();
                                    h.Cell().Element(HeaderCell).Text("Ghi chú").Bold().AlignCenter();
                                });

                                static IContainer DataCell(IContainer c) =>
                                    c.Border(1).BorderColor(Colors.Black).Padding(3)
                                    .AlignMiddle();

                                foreach (var row in rows)
                                {
                                    table.Cell().Element(DataCell).AlignCenter().Text(row.STT.ToString());
                                    table.Cell().Element(DataCell).Text(row.HoTen);
                                    table.Cell().Element(DataCell).Text(row.ChucVu);
                                    table.Cell().Element(DataCell).Text(row.DonViSDTEmail);
                                    table.Cell().Element(DataCell).Text(row.TrinhDo);
                                    table.Cell().Element(DataCell).Text("");
                                }
                            });

                            col.Item().PaddingTop(12)
                                .Text(t =>
                                {
                                    t.Span("        "); 
                                    t.Span("Đề nghị Hội đồng xét công nhận sáng kiến với các thông tin như sau:");
                                });

                            void MucBold(ColumnDescriptor col2, string so, string tieuDe, string noidung) =>
                                col2.Item().PaddingTop(6).Text(t =>
                                {
                                    t.Span("        ");
                                    t.Span($"{so} ").Bold();
                                    t.Span(tieuDe).Bold();
                                    t.Span($" {noidung}");
                                });

                            MucBold(col, "1.", "Tên sáng kiến:", sangKien.TenSK ?? "");
                            MucBold(col, "2.", "Chủ đầu tư tạo ra sáng kiến (nếu có):", !string.IsNullOrEmpty(sangKien.ChuDauTu?.DiaChi) && !string.IsNullOrEmpty(sangKien.ChuDauTu?.TenCDT) ? $"{sangKien.ChuDauTu.TenCDT} ({sangKien.ChuDauTu.DiaChi})" : "");
                            MucBold(col, "3.", "Lĩnh vực áp dụng sáng kiến:", string.Join(", ", dSTenLV));
                            MucBold(col, "4.", "Thời gian triển khai áp dụng tại đơn vị:", sangKien.NgayApDung.ToString("dd/MM/yyyy"));

                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span("5. ").Bold();
                                t.Span("Thực trạng trước khi áp dụng sáng kiến:").Bold();
                            });
                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span(sangKien.ThucTrangTruoc ?? "");
                            });

                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span("6. ").Bold();
                                t.Span("Mô tả sáng kiến (giải pháp):").Bold();
                            });

                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span("6.1. ").Italic();
                                t.Span("Về nội dung của sáng kiến:").Italic();
                            });
                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span(sangKien.NoiDung ?? "");
                            });

                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span("6.2. ").Italic();
                                t.Span("Về khả năng áp dụng của sáng kiến:").Italic();
                            });
                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span(sangKien.KhaNangApDung ?? "");
                            });

                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span("6.3. ").Italic();
                                t.Span("Các thông tin cần được bảo mật (nếu có):").Italic();
                            });
                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span(sangKien.ThongTinBaoMat ?? "");
                            });

                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span("7. ").Bold();
                                t.Span("Các điều kiện cần thiết để áp dụng sáng kiến:").Bold();
                            });
                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span(sangKien.DieuKienCanThiet ?? "");
                            });

                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span("8. ").Bold();
                                t.Span("Đánh giá lợi ích thu được hoặc dự kiến có thể thu được do áp dụng sáng kiến theo ý kiến của tác giả:").Bold();
                            });
                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span("        ");
                                t.Span(sangKien.LoiIchDuKien ?? "");
                            });

                            col.Item().PaddingTop(12)
                                .Text(t =>
                                {
                                    t.Span("        "); 
                                    t.Span("Tôi (chúng tôi) xin cam đoan mọi thông tin nêu trong đơn là trung thực, đúng sự thật và hoàn toàn chịu trách nhiệm trước pháp luật.");
                                });
                            col.Item().PaddingTop(24).Row(row =>
                            {
                                row.RelativeItem().Column(c1 =>
                                {
                                    c1.Item().AlignCenter()
                                        .Text("XÁC NHẬN CỦA ĐƠN VỊ").Bold();
                                    c1.Item().AlignCenter().PaddingTop(6)
                                        .Text("").Bold().Italic();
                                });
                                row.RelativeItem().Column(c2 =>
                                {
                                    c2.Item().AlignCenter()
                                        .Text($"Vĩnh Long, ngày {sangKien.NgayNop.Day} tháng {sangKien.NgayNop.Month} năm {sangKien.NgayNop.Year}")
                                        .Italic();
                                    c2.Item().AlignCenter().PaddingTop(6)
                                        .Text("Người nộp đơn/Đại diện nhóm").Bold();
                                    c2.Item().AlignCenter().PaddingTop(6)
                                        .Text("(Ký và ghi rõ họ tên)").Italic();
                                });
                            });
                        });
                    });
                });

                document.GeneratePdf(pdfPath);

                sangKien.UrlTepSK = $"/uploads/sk_{ma}.pdf";
                
                _appDbContext.SaveChanges();

                _logger.LogInformation("Tạo file sáng kiến PDF thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo file sáng kiến PDF");
            }
        }

        [HttpGet]
        [Authorize(Roles = "DVTT, BGD")]
        public async Task<IActionResult> TimKiem(string tukhoa)
        {
            var danhSachCumTu = _xuLyVanBanService.TaoDanhSachCumTuTimKiem(tukhoa);

            if (danhSachCumTu == null || !danhSachCumTu.Any())
            {
                return View(nameof(Index), new List<SangKienVM>());
            }

            var quyen = User.FindFirstValue(ClaimTypes.Role);
            var maCBGV = User.FindFirstValue("MaCBGV");
            IQueryable<SangKien> query = _appDbContext.DbSetSangKien;

            if (quyen == "BGD")
            {
                query = query.Where(x => x.TrangThaiSK != "NHAP" ||
                    x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) ||
                    x.MaCBGV == maCBGV);
            }
            else if (quyen == "DVTT")
            {
                var tatCaDonVi = await _appDbContext.DbSetDonVi
                    .Select(x => new { x.MaDV, x.MaDVCha })
                    .ToListAsync();

                var maDVHienTai = await _appDbContext.DbSetCBGVChucVuDonVi
                    .Where(x => x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.ThoiGianBatDau)
                    .Select(x => x.MaDV)
                    .FirstOrDefaultAsync();

                var dsMaDV = new List<int>();
                void LayDonViVaCon(int maDV)
                {
                    dsMaDV.Add(maDV);
                    foreach (var dv in tatCaDonVi.Where(x => x.MaDVCha == maDV))
                        LayDonViVaCon(dv.MaDV);
                }

                if (maDVHienTai != 0)
                    LayDonViVaCon(maDVHienTai);

                query = query.Where(x => x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) ||
                    x.MaCBGV == maCBGV ||
                    (x.TrangThaiSK != "NHAP" &&
                        x.DSCBGVSangKien.Any(s =>
                            s.CanBoGiangVien.DSCBGVChucVuDonVi.Any(cvdv =>
                                dsMaDV.Contains(cvdv.MaDV)))));
            }
            else if (quyen == "CBGV" || quyen == "ADMIN")
            {
                query = query.Where(x => x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) ||
                    x.MaCBGV == maCBGV);
            }
            else
            {
                query = query.Where(x => false);
            }

            var allSangKien = query
                .Include(sk => sk.DSCBGVSangKien)
                    .ThenInclude(cbgvsk => cbgvsk.CanBoGiangVien)
                .Include(sk => sk.DSMonHocSangKien)
                    .ThenInclude(mhsk => mhsk.MonHoc)
                .Include(sk => sk.DSLinhVucSangKien)
                    .ThenInclude(lvsk => lvsk.LinhVuc)
                .ToList();

            var danhSachCumTuLower = danhSachCumTu
                .Where(ct => !string.IsNullOrWhiteSpace(ct))
                .Select(ct => ct.ToLower().Trim())
                .Where(ct => !string.IsNullOrEmpty(ct))
                .Distinct()
                .ToList();

            var resultWithScore = allSangKien
                .Select(sk => new
                {
                    SangKien = sk,
                    DiemMonHoc = sk.DSMonHocSangKien
                        .Sum(mhsk =>
                        {
                            var cumNghiaList = string.IsNullOrWhiteSpace(mhsk.MonHoc.CumNguNghia)
                                ? new List<string>()
                                : mhsk.MonHoc.CumNguNghia
                                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(cn => cn.ToLower().Trim())
                                    .Where(cn => !string.IsNullOrEmpty(cn))
                                    .Distinct()
                                    .ToList();

                            bool coKhopSemantic = cumNghiaList.Any(cumNghia =>
                                danhSachCumTuLower.Any(ct => cumNghia.Contains(ct) || ct.Contains(cumNghia)));

                            var tenMH = string.IsNullOrEmpty(mhsk.MonHoc.TenMH)
                                ? string.Empty
                                : mhsk.MonHoc.TenMH.ToLower();

                            bool coKhopTenMH = danhSachCumTuLower.Any(ct => tenMH.Contains(ct));

                            return coKhopSemantic || coKhopTenMH ? 1 : 0;
                        }),
                    DiemLinhVuc = sk.DSLinhVucSangKien
                        .Sum(lvsk =>
                        {
                            if (string.IsNullOrEmpty(lvsk.LinhVuc.TenLV))
                                return 0;

                            var tenLV = lvsk.LinhVuc.TenLV.ToLower();
                            return danhSachCumTuLower.Count(ct => tenLV.Contains(ct)) > 0 ? 1 : 0;
                        }),
                    DiemSangKien = danhSachCumTuLower.Sum(ct =>
                    {
                        var tenSK = string.IsNullOrEmpty(sk.TenSK) ? string.Empty : sk.TenSK.ToLower();
                        var noiDung = string.IsNullOrEmpty(sk.NoiDung) ? string.Empty : sk.NoiDung.ToLower();
                        var thucTrang = string.IsNullOrEmpty(sk.ThucTrangTruoc) ? string.Empty : sk.ThucTrangTruoc.ToLower();
                        var khaNang = string.IsNullOrEmpty(sk.KhaNangApDung) ? string.Empty : sk.KhaNangApDung.ToLower();
                        var loiIch = string.IsNullOrEmpty(sk.LoiIchDuKien) ? string.Empty : sk.LoiIchDuKien.ToLower();
                        var dieuKien = string.IsNullOrEmpty(sk.DieuKienCanThiet) ? string.Empty : sk.DieuKienCanThiet.ToLower();

                        return (tenSK.Contains(ct) ? 1 : 0) +
                               (noiDung.Contains(ct) ? 1 : 0) +
                               (thucTrang.Contains(ct) ? 1 : 0) +
                               (khaNang.Contains(ct) ? 1 : 0) +
                               (loiIch.Contains(ct) ? 1 : 0) +
                               (dieuKien.Contains(ct) ? 1 : 0);
                    })
                })
                .Where(x => x.DiemMonHoc > 0 || x.DiemLinhVuc > 0 || x.DiemSangKien > 0)
                .OrderByDescending(x => x.DiemMonHoc)
                .ThenByDescending(x => x.DiemLinhVuc)
                .ThenByDescending(x => x.DiemSangKien)
                .Take(5)
                .ToList();

            if (!resultWithScore.Any())
            {
                return View(nameof(Index), new List<SangKienVM>());
            }

            var kq = resultWithScore
                .Select(x => new SangKienVM
                {
                    MaSK = x.SangKien.MaSK,
                    TenSK = x.SangKien.TenSK,
                    NgayNop = x.SangKien.NgayNop,
                    TrangThaiSK = x.SangKien.TrangThaiSK,
                    DiemTimKhiem = x.DiemMonHoc + x.DiemLinhVuc + x.DiemSangKien
                })
                .ToList();

            return View(nameof(Index), kq);
        }

        [HttpPost]
        [Authorize(Roles = "DVTT, BGD, CBGV, ADMIN")]
        public async Task<IActionResult> DanhGia(DanhGiaVM danhGiaVM)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien.FindAsync(danhGiaVM.MaSK);

                var now = DateTime.Now;
                var activeHoiDong = await _appDbContext.DbSetHoiDong
                    .FirstOrDefaultAsync(h => h.NgayLap <= now && h.NgayKetThuc >= now);

                if (activeHoiDong == null)
                {
                    TempData["Error"] = "Không có hội đồng nào đang hoạt động";
                    return RedirectToAction(nameof(Index));
                }

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng nhập đủ thông tin đánh giá";
                    return RedirectToAction(nameof(ChiTiet), new { ma = danhGiaVM.MaSK });
                }

                var maCBGV = User.FindFirstValue("MaCBGV");

                var lanDGMoiNhat = await _appDbContext.DbSetDanhGia
                    .Where(dg => dg.MaSK == danhGiaVM.MaSK && dg.MaHD == activeHoiDong.MaHD)
                    .OrderByDescending(dg => dg.NgayDG)
                    .FirstOrDefaultAsync();

                if (lanDGMoiNhat != null && lanDGMoiNhat.LanDG >= 2) 
                {
                    TempData["Error"] = "Sáng kiến này đã được đánh giá 2 lần, không thể đánh giá thêm";
                    return RedirectToAction(nameof(ChiTiet), new { ma = danhGiaVM.MaSK });
                }

                var danhGia = new DanhGia
                {
                    MaSK = danhGiaVM.MaSK,
                    MaHD = activeHoiDong.MaHD,
                    MaCBGV = maCBGV!,
                    NgayDG = DateTime.Now,
                    LanDG = lanDGMoiNhat == null ? 1 : lanDGMoiNhat.LanDG + 1,
                    YKienNhanXet = danhGiaVM.YKienNhanXet,
                    CongNhan = danhGiaVM.CongNhan,
                };

                _appDbContext.DbSetDanhGia.Add(danhGia);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Đánh giá sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = danhGiaVM.MaSK });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "DVTT, BGD, CBGV, ADMIN")]
        public async Task<IActionResult> QuyetDinh(int MaSK, int CongNhan)
        {
            try
            {
                var sangKien = await _appDbContext.DbSetSangKien.FindAsync(MaSK);

                var now = DateTime.Now;
                var activeHoiDong = await _appDbContext.DbSetHoiDong
                    .FirstOrDefaultAsync(h => h.NgayLap <= now && h.NgayKetThuc >= now);

                if (activeHoiDong == null)
                {
                    TempData["Error"] = "Không có hội đồng nào đang hoạt động";
                    return RedirectToAction(nameof(Index));
                }

                if (sangKien == null)
                {
                    TempData["Error"] = "Sáng kiến không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                if (MaSK <= 0)
                {
                    TempData["Error"] = "Sáng kiến không hợp lệ";
                    return RedirectToAction(nameof(Index));
                }

                if (CongNhan != 0 && CongNhan != 1)
                {
                    TempData["Error"] = "Vui chọn quyết định công nhận hoặc không công nhận sáng kiến";
                    return RedirectToAction(nameof(ChiTiet), new { ma = MaSK });
                }

                if (CongNhan == 1)
                {
                    sangKien.TrangThaiSK = "DA_CONG_NHAN";
                }
                else if (CongNhan == 0)
                {
                    sangKien.TrangThaiSK = "KHONG_CONG_NHAN";
                }

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Gửi quyết định công nhận sáng kiến thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = MaSK });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
                _logger.LogError(ex, "Lỗi khi gửi quyết định công nhận sáng kiến");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
