using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class LinhVucController : Controller
    {
        private readonly ILogger<LinhVucController> _logger;
        private readonly AppDbContext _appDbContext;

        public LinhVucController(ILogger<LinhVucController> logger, AppDbContext appDbContext)
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

            int tongSoDong = await _appDbContext.DbSetLinhVuc.CountAsync();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / soDongTrenTrang);

            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;

            var data = await _appDbContext.DbSetLinhVuc
                .OrderBy(x => x.TenLV)
                .Skip((trang - 1) * soDongTrenTrang)
                .Take(soDongTrenTrang)
                .Select(x => new LinhVucVM
                {
                    MaLV = x.MaLV,
                    TenLV = x.TenLV,

                    DaSuDung = _appDbContext.DbSetLinhVucSangKien
                        .Any(y => y.MaLV == x.MaLV)
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
            var linhVucVM = new LinhVucVM();

            if (ma != null)
            {
                var linhVuc = await _appDbContext.DbSetLinhVuc
                    .FindAsync(ma);

                if (linhVuc == null)
                {
                    TempData["Error"] = "Lĩnh vực không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                linhVucVM.MaLV = linhVuc.MaLV;
                linhVucVM.TenLV = linhVuc.TenLV;
            }

            return View(linhVucVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(LinhVucVM linhVucVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(linhVucVM);
                }

                string tenLinhVuc = linhVucVM.TenLV!.Trim();

                bool isExist = await _appDbContext.DbSetLinhVuc
                    .AnyAsync(x =>
                        x.TenLV.ToLower() == tenLinhVuc.ToLower() &&
                        x.MaLV != linhVucVM.MaLV
                    );

                if (isExist)
                {
                    ModelState.AddModelError(
                        nameof(linhVucVM.TenLV),
                        "Tên lĩnh vực đã tồn tại"
                    );

                    return View(linhVucVM);
                }

                if (linhVucVM.MaLV <= 0)
                {
                    var linhVuc = new LinhVuc
                    {
                        TenLV = tenLinhVuc
                    };

                    _appDbContext.DbSetLinhVuc.Add(linhVuc);

                    TempData["Success"] = "Thêm lĩnh vực thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = linhVuc.MaLV });
                }
                else
                {
                    var linhVuc = await _appDbContext.DbSetLinhVuc
                        .FindAsync(linhVucVM.MaLV);

                    if (linhVuc == null)
                    {
                        TempData["Error"] = "Lĩnh vực không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    linhVuc.TenLV = tenLinhVuc;

                    TempData["Success"] = "Cập nhật lĩnh vực thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = linhVuc.MaLV });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(linhVucVM);
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var linhVuc = await _appDbContext.DbSetLinhVuc.FindAsync(ma);

                if (linhVuc == null)
                {
                    TempData["Error"] = "Lĩnh vực không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetLinhVucSangKien
                    .AnyAsync(x => x.MaLV == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Lĩnh vực đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetLinhVuc.Remove(linhVuc);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa lĩnh vực thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }  
    }
}
