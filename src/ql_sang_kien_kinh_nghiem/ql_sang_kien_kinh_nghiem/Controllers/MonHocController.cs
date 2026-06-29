using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;
using ql_sang_kien_kinh_nghiem.Services;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class MonHocController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _appDbContext;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly XuLyVanBanService _xuLyVanBanService;

        public MonHocController(ILogger<HomeController> logger, AppDbContext appDbContext, IServiceScopeFactory scopeFactory, XuLyVanBanService xuLyVanBanService)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _scopeFactory = scopeFactory;
            _xuLyVanBanService = xuLyVanBanService;
        }

        [Authorize(Roles = "BGD")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _appDbContext.DbSetNganh
                .OrderBy(n => n.TenNganh)
                .Select(n => new NganhVM
                {
                    MaNganh = n.MaNganh,
                    TenNganh = n.TenNganh,

                    DSMonHoc = n.DSMonHoc
                        .OrderBy(m => m.TenMH)
                        .Select(m => new MonHocVM
                        {
                            MaMH = m.MaMH,
                            TenMH = m.TenMH,

                            DaSuDung = _appDbContext.DbSetMonHocSangKien
                                .Any(mhsk => mhsk.MaMH == m.MaMH)
                        })
                        .ToList()
                })
                .ToListAsync();
                
            return View(data);
        }

        [HttpGet]
        [Authorize(Roles = "BGD")]
        public async Task<IActionResult> ChiTiet(int? ma)
        {
            var monHocVM = new MonHocVM();

            monHocVM.DSNganh = await _appDbContext.DbSetNganh
                .OrderBy(x => x.TenNganh)
                .Select(x => new SelectListItem
                {
                    Value = x.MaNganh.ToString(),
                    Text = x.TenNganh
                })
                .ToListAsync();

            if (ma != null)
            {
                var monHoc = await _appDbContext.DbSetMonHoc
                    .FindAsync(ma);

                if (monHoc == null)
                {
                    TempData["Error"] = "Môn học không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                monHocVM.MaMH = monHoc.MaMH;
                monHocVM.TenMH = monHoc.TenMH;
                monHocVM.MoTaMH = monHoc.MoTaMH;
                monHocVM.MaNganh = monHoc.MaNganh;
            }
            return View(monHocVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(MonHocVM monHocVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    monHocVM.DSNganh = await _appDbContext.DbSetNganh
                        .OrderBy(x => x.TenNganh)
                        .Select(x => new SelectListItem
                        {
                            Value = x.MaNganh.ToString(),
                            Text = x.TenNganh
                        })
                        .ToListAsync();

                    return View(monHocVM);
                }

                string tenMH = monHocVM.TenMH!.Trim();
                string? moTaMH = monHocVM.MoTaMH?.Trim();
                
                bool isExist = await _appDbContext.DbSetMonHoc
                    .AnyAsync(x =>
                        x.MaNganh == monHocVM.MaNganh &&
                        x.TenMH.ToLower() == tenMH.ToLower() &&
                        x.MaMH != monHocVM.MaMH
                    );

                if (isExist)
                {
                    ModelState.AddModelError(
                        nameof(monHocVM.TenMH),
                        "Tên môn học đã tồn tại"
                    );

                    monHocVM.DSNganh = await _appDbContext.DbSetNganh
                        .OrderBy(x => x.TenNganh)
                        .Select(x => new SelectListItem
                        {
                            Value = x.MaNganh.ToString(),
                            Text = x.TenNganh
                        })
                        .ToListAsync();

                    return View(monHocVM);
                }

                if (monHocVM.MaMH <= 0)
                {
                    var monHoc = new MonHoc
                    {
                        TenMH = tenMH,
                        MoTaMH = moTaMH,
                        MaNganh = monHocVM.MaNganh!.Value
                    };

                    _appDbContext.DbSetMonHoc.Add(monHoc);

                    await _appDbContext.SaveChangesAsync();

                    TempData["Success"] = "Thêm môn học thành công";

                    return RedirectToAction(nameof(ChiTiet), new { ma = monHoc.MaMH });
                }
                else
                {
                    var monHoc = await _appDbContext.DbSetMonHoc
                        .FindAsync(monHocVM.MaMH);

                    if (monHoc == null)
                    {
                        TempData["Error"] = "Môn học không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    monHoc.TenMH = tenMH;
                    monHoc.MoTaMH = moTaMH;
                    monHoc.MaNganh = monHocVM.MaNganh!.Value;

                    await _appDbContext.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật môn học thành công";

                    return RedirectToAction(nameof(ChiTiet), new { ma = monHoc.MaMH });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                monHocVM.DSNganh = await _appDbContext.DbSetNganh
                    .OrderBy(x => x.TenNganh)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaNganh.ToString(),
                        Text = x.TenNganh
                    })
                    .ToListAsync();

                return View(monHocVM);
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var monHoc = await _appDbContext.DbSetMonHoc.FindAsync(ma);

                if (monHoc == null)
                {
                    TempData["Error"] = "Môn học không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetMonHocSangKien
                                .AnyAsync(mhsk => mhsk.MaMH == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Môn học đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetMonHoc.Remove(monHoc);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa môn học thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> TaoMoTaMonHocAI(int ma) 
        {
            _ = Task.Run(async () =>
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<HomeController>>();

                    try
                    {
                        logger.LogInformation($"Bắt đầu tạo mô tả môn học");

                        var monHoc = await dbContext.DbSetMonHoc
                            .Include(x => x.Nganh)
                            .FirstOrDefaultAsync(x => x.MaMH == ma);

                        if (monHoc == null) return;

                        var geminiService = scope.ServiceProvider.GetRequiredService<GeminiService>(); 
                        var moTa = await geminiService.TaoMoTaMonHoc(monHoc.Nganh.TenNganh, monHoc.TenMH);

                        monHoc.MoTaMH = moTa;

                        await dbContext.SaveChangesAsync();

                        logger.LogInformation($"Tạo mô tả môn học thành công");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Lỗi tạo mô tả môn học");
                    }
                }
            });

            return RedirectToAction(nameof(ChiTiet), new { ma = ma });
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> TaoCacCumTu(int ma, string moTaMH)
        {
            if(!string.IsNullOrWhiteSpace(moTaMH))
            {
                _ = Task.Run(async () =>
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HomeController>>();

                        try
                        {
                            logger.LogInformation($"Bắt đầu xử lý cụm từ cho mô tả môn học");

                            string[] danhSachVe = _xuLyVanBanService.TachVeCau(moTaMH);

                            List<string> danhSachCum = _xuLyVanBanService.SinhCumTuTho(danhSachVe);

                            List<string> danhSachCumLocBien = _xuLyVanBanService.LocBienCumTu(danhSachCum);

                            List<string> danhSachCumDaXuLy = _xuLyVanBanService.XuLyCumChaCon(danhSachCumLocBien);

                            string chuoiCumTu = string.Join(", ", danhSachCumDaXuLy);

                            var monHoc = await dbContext.DbSetMonHoc
                            .Include(x => x.Nganh)
                            .FirstOrDefaultAsync(x => x.MaMH == ma);

                            if (monHoc == null) return;

                            monHoc.CumNguNghia = chuoiCumTu;

                            await dbContext.SaveChangesAsync();

                            logger.LogInformation($"Xử lý cụm từ cho mô tả môn học thành công");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Lỗi xử lý cụm từ cho mô tả môn học");
                        }
                    }
                });
            } else
            {
                _logger.LogWarning($"Mô tả môn học trống, không thể xử lý cụm từ");
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = ma });
        }
    }
}
