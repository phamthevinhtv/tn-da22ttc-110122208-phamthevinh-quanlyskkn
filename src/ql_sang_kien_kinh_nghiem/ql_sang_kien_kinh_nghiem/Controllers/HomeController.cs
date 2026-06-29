using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.ViewModels;
using System.Security.Claims;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _appDbContext;

        public HomeController(ILogger<HomeController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(string type)
        {
            var quyen = User.FindFirstValue(ClaimTypes.Role);

            if(quyen == "CBGV" || quyen == "ADMIN")
            {
                return RedirectToAction("Index", "SangKien");
            }

            var statusMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["NHAP"] = "Nháp",
                ["CHO_DUYET"] = "Chờ duyệt",
                ["YEU_CAU_CHINH_SUA"] = "Yêu cầu chỉnh sửa",
                ["TU_CHOI"] = "Từ chối",
                ["CHO_DANH_GIA"] = "Chờ đánh giá",
                ["DA_CONG_NHAN"] = "Đã công nhận",
                ["KHONG_CONG_NHAN"] = "Không công nhận",
                ["YEU_CAU_PHUC_KHAO"] = "Yêu cầu phúc khảo",
                ["CHO_PHUC_KHAO"] = "Chờ phúc khảo"
            };

            var maCBGV = User.FindFirstValue("MaCBGV");

            var sangKienQuery = _appDbContext.DbSetSangKien.AsQueryable();
            var dsMaDV = new List<int>();

            if (quyen == "DVTT" && !string.IsNullOrEmpty(maCBGV))
            {
                var tatCaDonVi = await _appDbContext.DbSetDonVi
                    .Select(x => new { x.MaDV, x.MaDVCha })
                    .ToListAsync();

                var maDVHienTai = await _appDbContext.DbSetCBGVChucVuDonVi
                    .Where(x => x.MaCBGV == maCBGV)
                    .OrderByDescending(x => x.ThoiGianBatDau)
                    .Select(x => x.MaDV)
                    .FirstOrDefaultAsync();

                void LayDonViVaCon(int maDV)
                {
                    dsMaDV.Add(maDV);
                    foreach (var dv in tatCaDonVi.Where(x => x.MaDVCha == maDV))
                        LayDonViVaCon(dv.MaDV);
                }

                if (maDVHienTai != 0)
                    LayDonViVaCon(maDVHienTai);

                sangKienQuery = sangKienQuery.Where(x =>
                    x.DSCBGVSangKien.Any(s => s.MaCBGV == maCBGV) ||
                    x.MaCBGV == maCBGV ||
                    (x.TrangThaiSK != "NHAP" &&
                        x.DSCBGVSangKien.Any(s =>
                            s.CanBoGiangVien.DSCBGVChucVuDonVi.Any(cvdv =>
                                dsMaDV.Contains(cvdv.MaDV))))) ;
            }

            var totalSangKien = await sangKienQuery.CountAsync();
            var totalGiangVien = quyen == "DVTT"
                ? await _appDbContext.DbSetCanBoGiangVien
                    .Where(gv => gv.DSCBGVChucVuDonVi.Any(cvdv => dsMaDV.Contains(cvdv.MaDV)))
                    .Select(gv => gv.MaCBGV)
                    .Distinct()
                    .CountAsync()
                : await _appDbContext.DbSetCanBoGiangVien.CountAsync();
            var totalHoiDong = quyen == "DVTT"
                ? await _appDbContext.DbSetHoiDong
                    .Where(h => h.DSCBGVVaiTroHoiDong.Any(v =>
                        v.CanBoGiangVien.DSCBGVChucVuDonVi.Any(cvdv => dsMaDV.Contains(cvdv.MaDV))))
                    .CountAsync()
                : await _appDbContext.DbSetHoiDong.CountAsync();
            var totalDonVi = quyen == "DVTT" ? dsMaDV.Count : await _appDbContext.DbSetDonVi.CountAsync();
            var totalNganh = quyen == "DVTT"
                ? await _appDbContext.DbSetMonHocSangKien
                    .Where(mhs => sangKienQuery.Select(sk => sk.MaSK).Contains(mhs.MaSK))
                    .Join(_appDbContext.DbSetMonHoc,
                        mhs => mhs.MaMH,
                        mh => mh.MaMH,
                        (mhs, mh) => mh.MaNganh)
                    .Distinct()
                    .CountAsync()
                : await _appDbContext.DbSetNganh.CountAsync();
            var totalChuDauTu = quyen == "DVTT"
                ? await sangKienQuery.Where(x => x.MaCDT != null)
                    .Select(x => x.MaCDT)
                    .Distinct()
                    .CountAsync()
                : await _appDbContext.DbSetChuDauTu.CountAsync();

            var rawStatusCounts = await sangKienQuery
                .GroupBy(x => x.TrangThaiSK)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var statusCounts = rawStatusCounts
                .Select(x => new
                {
                    Status = statusMap.ContainsKey(x.Status) ? statusMap[x.Status] : x.Status,
                    x.Count
                })
                .ToList();

            var orderedStatuses = new[] { "Nháp", "Chờ duyệt", "Yêu cầu chỉnh sửa", "Từ chối", "Chờ đánh giá", "Đã công nhận", "Không công nhận", "Yêu cầu phúc khảo", "Chờ phúc khảo" };
            var statusLabels = orderedStatuses.ToList();
            var statusValues = orderedStatuses
                .Select(label => statusCounts.FirstOrDefault(x => x.Status == label)?.Count ?? 0)
                .ToList();

            var totalChoDuyet = rawStatusCounts.FirstOrDefault(x => x.Status == "CHO_DUYET")?.Count ?? 0;
            var totalYeuCauChinhSua = rawStatusCounts.FirstOrDefault(x => x.Status == "YEU_CAU_CHINH_SUA")?.Count ?? 0;
            var totalDaCongNhan = rawStatusCounts.FirstOrDefault(x => x.Status == "DA_CONG_NHAN")?.Count ?? 0;
            var totalKhongCongNhan = rawStatusCounts.FirstOrDefault(x => x.Status == "KHONG_CONG_NHAN")?.Count ?? 0;
            var totalYeuCauPhucKhao = rawStatusCounts.FirstOrDefault(x => x.Status == "YEU_CAU_PHUC_KHAO")?.Count ?? 0;
            var totalChoPhucKhao = rawStatusCounts.FirstOrDefault(x => x.Status == "CHO_PHUC_KHAO")?.Count ?? 0;
            var phanTramCongNhan = totalSangKien > 0 ? Math.Round((double)totalDaCongNhan / totalSangKien * 100, 2) : 0;

            var startMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
            var monthKeys = Enumerable.Range(0, 6)
                .Select(i => startMonth.AddMonths(i))
                .ToList();

            var nopTheothang = await sangKienQuery
                .Where(x => x.NgayNop >= startMonth)
                .GroupBy(x => new { x.NgayNop.Year, x.NgayNop.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var congNhanTheothang = await sangKienQuery
                .Where(x => x.TrangThaiSK == "DA_CONG_NHAN" && x.NgayChapNhanDanhGia.HasValue && x.NgayChapNhanDanhGia.Value >= startMonth)
                .GroupBy(x => new { Year = x.NgayChapNhanDanhGia!.Value.Year, Month = x.NgayChapNhanDanhGia!.Value.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var thangLabels = monthKeys.Select(m => m.ToString("MM/yyyy")).ToList();
            var thangNopValues = monthKeys
                .Select(m => nopTheothang.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Count ?? 0)
                .ToList();
            var thangCongNhanValues = monthKeys
                .Select(m => congNhanTheothang.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Count ?? 0)
                .ToList();

            var approvedSangKienQuery = sangKienQuery.Where(x => x.TrangThaiSK == "DA_CONG_NHAN");

            var topDonVi = await (from cv in _appDbContext.DbSetCBGVChucVuDonVi
                                  join sk in _appDbContext.DbSetCBGVSangKien on cv.MaCBGV equals sk.MaCBGV
                                  join s in approvedSangKienQuery on sk.MaSK equals s.MaSK
                                  select new { cv.MaDV, sk.MaSK })
                .Distinct()
                .GroupBy(x => x.MaDV)
                .Select(g => new
                {
                    MaDV = g.Key,
                    Count = g.Select(x => x.MaSK).Distinct().Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Join(_appDbContext.DbSetDonVi,
                    x => x.MaDV,
                    dv => dv.MaDV,
                    (x, dv) => new
                    {
                        dv.TenDV,
                        x.Count
                    })
                .ToListAsync();

            var topGiangVien = await (from sk in approvedSangKienQuery
                                        join csk in _appDbContext.DbSetCBGVSangKien on sk.MaSK equals csk.MaSK
                                        group csk by csk.MaCBGV into g
                                        select new
                                        {
                                            MaCBGV = g.Key,
                                            Count = g.Select(x => x.MaSK).Distinct().Count()
                                        })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Join(_appDbContext.DbSetCanBoGiangVien,
                    x => x.MaCBGV,
                    gv => gv.MaCBGV,
                    (x, gv) => new
                    {
                        gv.HoTen,
                        x.Count
                    })
                .ToListAsync();

            var topMonHoc = await (from mhs in _appDbContext.DbSetMonHocSangKien
                                     join sk in approvedSangKienQuery on mhs.MaSK equals sk.MaSK
                                     group mhs by mhs.MaMH into g
                                     select new
                                     {
                                         MaMH = g.Key,
                                         Count = g.Select(x => x.MaSK).Distinct().Count()
                                     })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Join(_appDbContext.DbSetMonHoc,
                    x => x.MaMH,
                    mh => mh.MaMH,
                    (x, mh) => new
                    {
                        mh.TenMH,
                        x.Count
                    })
                .ToListAsync();

            var dashboard = new ThongKeVM
            {
                TotalSangKien = totalSangKien,
                TotalGiangVien = totalGiangVien,
                TotalHoiDong = totalHoiDong,
                TotalDonVi = totalDonVi,
                TotalNganh = totalNganh,
                TotalChuDauTu = totalChuDauTu,
                TotalSangKienChoDuyet = totalChoDuyet,
                TotalSangKienYeuCauChinhSua = totalYeuCauChinhSua,
                TotalSangKienDaCongNhan = totalDaCongNhan,
                TotalSangKienKhongCongNhan = totalKhongCongNhan,
                TotalSangKienYeuCauPhucKhao = totalYeuCauPhucKhao,
                TotalSangKienChoPhucKhao = totalChoPhucKhao,
                PhanTramCongNhan = phanTramCongNhan,
                TrangThaiLabels = statusLabels,
                TrangThaiValues = statusValues,
                TopDonViLabels = topDonVi.Select(x => x.TenDV).ToList(),
                TopDonViValues = topDonVi.Select(x => x.Count).ToList(),
                TopGiangVienLabels = topGiangVien.Select(x => x.HoTen).ToList(),
                TopGiangVienValues = topGiangVien.Select(x => x.Count).ToList(),
                TopMonHocLabels = topMonHoc.Select(x => x.TenMH).ToList(),
                TopMonHocValues = topMonHoc.Select(x => x.Count).ToList(),
                ThangLabels = thangLabels,
                ThangNopValues = thangNopValues,
                ThangCongNhanValues = thangCongNhanValues
            };

            return View(dashboard);
        }
    }
}
