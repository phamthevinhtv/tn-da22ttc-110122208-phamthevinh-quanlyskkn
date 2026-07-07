using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class ChucVuController : Controller
    {
        private readonly ILogger<ChucVuController> _logger;
        private readonly AppDbContext _appDbContext;

        public ChucVuController(ILogger<ChucVuController> logger, AppDbContext appDbContext)
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

            int tongSoDong = await _appDbContext.DbSetChucVu.CountAsync();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / soDongTrenTrang);

            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;
            
            var data = await _appDbContext.DbSetChucVu
                .OrderBy(x => x.TenCV)
                .Where(x => x.MaCV != 1)
                .Skip((trang - 1) * soDongTrenTrang)
                .Take(soDongTrenTrang)
                .Select(x => new ChucVuVM
                {
                    MaCV = x.MaCV,
                    TenCV = x.TenCV,

                    DaSuDung = _appDbContext.DbSetCBGVChucVuDonVi
                        .Any(y => y.MaCV == x.MaCV)
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
            var chucVuVM = new ChucVuVM();

            if (ma != null)
            {
                if (ma == 1)
                {
                    TempData["Error"] = "Chức vụ không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                var chucVu = await _appDbContext.DbSetChucVu
                    .FindAsync(ma);

                if (chucVu == null)
                {
                    TempData["Error"] = "Chức vụ không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                chucVuVM.MaCV = chucVu.MaCV;
                chucVuVM.TenCV = chucVu.TenCV;
            }

            return View(chucVuVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(ChucVuVM chucVuVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(chucVuVM);
                }

                string tenChucVu = chucVuVM.TenCV!.Trim();

                bool isExist = await _appDbContext.DbSetChucVu
                    .AnyAsync(x =>
                        x.TenCV.ToLower() == tenChucVu.ToLower() &&
                        x.MaCV != chucVuVM.MaCV
                    );

                if (isExist)
                {
                    ModelState.AddModelError(
                        nameof(chucVuVM.TenCV),
                        "Tên chức vụ đã tồn tại"
                    );

                    return View(chucVuVM);
                }

                if (chucVuVM.MaCV <= 0)
                {
                    var chucVu = new ChucVu
                    {
                        TenCV = tenChucVu
                    };

                    _appDbContext.DbSetChucVu.Add(chucVu);

                    TempData["Success"] = "Thêm chức vụ thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = chucVu.MaCV });
                }
                else
                {
                    var chucVu = await _appDbContext.DbSetChucVu
                        .FindAsync(chucVuVM.MaCV);

                    if (chucVu == null)
                    {
                        TempData["Error"] = "Chức vụ không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    chucVu.TenCV = tenChucVu;

                    TempData["Success"] = "Cập nhật chức vụ thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = chucVu.MaCV });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(chucVuVM);
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var chucVu = await _appDbContext.DbSetChucVu.FindAsync(ma);

                if (chucVu == null)
                {
                    TempData["Error"] = "Chức vụ không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetCBGVChucVuDonVi
                    .AnyAsync(x => x.MaDV == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Chức vụ đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetChucVu.Remove(chucVu);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa chức vụ thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
