using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class TrinhDoController : Controller
    {
        private readonly ILogger<TrinhDoController> _logger;
        private readonly AppDbContext _appDbContext;

        public TrinhDoController(ILogger<TrinhDoController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        [Authorize(Roles = "BGD")]
        [HttpGet]
        public async Task<IActionResult> Index(int trang = 1)
        {
            int soDongTrenTrang = 15;
            if (trang < 1) trang = 1;

            int tongSoDong = await _appDbContext.DbSetTrinhDo.CountAsync();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / soDongTrenTrang);

            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;
            
            var data = await _appDbContext.DbSetTrinhDo
                .OrderBy(x => x.TenTD)
                .Skip((trang - 1) * soDongTrenTrang)
                .Take(soDongTrenTrang)
                .Select(x => new TrinhDoVM
                    {
                        MaTD = x.MaTD,
                        TenTD = x.TenTD,

                        DaSuDung = _appDbContext.DbSetCBGVTrinhDoNganh
                            .Any(y => y.MaTD == x.MaTD)
                    })
                    .ToListAsync();
                
            ViewBag.TrangHienTai = trang;
            ViewBag.TongSoTrang = tongSoTrang;

            return View(data);
        }

        [Authorize(Roles = "BGD")]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int? ma)
        {
            var trinhDoVM = new TrinhDoVM();

            if (ma != null)
            {

                var trinhDo = await _appDbContext.DbSetTrinhDo
                    .FindAsync(ma);

                if (trinhDo == null)
                {
                    TempData["Error"] = "Trình độ không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                trinhDoVM.MaTD = trinhDo.MaTD;
                trinhDoVM.TenTD = trinhDo.TenTD;
            }

            return View(trinhDoVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(TrinhDoVM trinhDoVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(trinhDoVM);
                }

                string tenTrinhDo = trinhDoVM.TenTD!.Trim();

                bool isExist = await _appDbContext.DbSetTrinhDo
                    .AnyAsync(x =>
                        x.TenTD.ToLower() == tenTrinhDo.ToLower() &&
                        x.MaTD != trinhDoVM.MaTD
                    );

                if (isExist)
                {
                    ModelState.AddModelError(
                        nameof(trinhDoVM.TenTD),
                        "Tên trình độ đã tồn tại"
                    );

                    return View(trinhDoVM);
                }

                if (trinhDoVM.MaTD <= 0)
                {
                    var trinhDo = new TrinhDo
                    {
                        TenTD = tenTrinhDo
                    };

                    _appDbContext.DbSetTrinhDo.Add(trinhDo);

                    TempData["Success"] = "Thêm trình độ thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = trinhDo.MaTD });
                }
                else
                {
                    var trinhDo = await _appDbContext.DbSetTrinhDo
                        .FindAsync(trinhDoVM.MaTD);

                    if (trinhDo == null)
                    {
                        TempData["Error"] = "Trình độ không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    trinhDo.TenTD = tenTrinhDo;

                    TempData["Success"] = "Cập nhật trình độ thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = trinhDo.MaTD });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(trinhDoVM);
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var trinhDo = await _appDbContext.DbSetTrinhDo.FindAsync(ma);

                if (trinhDo == null)
                {
                    TempData["Error"] = "Trình độ không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetCBGVTrinhDoNganh
                    .AnyAsync(x => x.MaTD == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Trình độ đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetTrinhDo.Remove(trinhDo);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa trình độ thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
