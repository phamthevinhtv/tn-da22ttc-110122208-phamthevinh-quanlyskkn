using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class NganhController : Controller
    {
        private readonly ILogger<NganhController> _logger;
        private readonly AppDbContext _appDbContext;

        public NganhController(ILogger<NganhController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        [Authorize(Roles = "BGD")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _appDbContext.DbSetNganh
                .OrderBy(n => n.TenNganh)
                .Select(x => new NganhVM
                    {
                        MaNganh = x.MaNganh,
                        TenNganh = x.TenNganh,

                        DaSuDung = _appDbContext.DbSetMonHoc
                                    .Any(y => y.MaNganh == x.MaNganh) ||
                                    _appDbContext.DbSetCBGVTrinhDoNganh
                                    .Any(y => y.MaNganh == x.MaNganh)
                    })
                    .ToListAsync();
                
            return View(data);
        }

        [Authorize(Roles = "BGD")]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int? ma)
        {
            var nganhVM = new NganhVM();

            if (ma != null)
            {
                var nganh = await _appDbContext.DbSetNganh
                    .FindAsync(ma);

                if (nganh == null)
                {
                    TempData["Error"] = "Ngành không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                nganhVM.MaNganh = nganh.MaNganh;
                nganhVM.TenNganh = nganh.TenNganh;
            }

            return View(nganhVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(NganhVM nganhVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(nganhVM);
                }

                string tenNganh = nganhVM.TenNganh!.Trim();

                bool isExist = await _appDbContext.DbSetNganh
                    .AnyAsync(x =>
                        x.TenNganh.ToLower() == tenNganh.ToLower() &&
                        x.MaNganh != nganhVM.MaNganh
                    );

                if (isExist)
                {
                    ModelState.AddModelError(
                        nameof(nganhVM.TenNganh),
                        "Tên ngành đã tồn tại"
                    );

                    return View(nganhVM);
                }

                if (nganhVM.MaNganh <= 0)
                {
                    var nganh = new Nganh
                    {
                        TenNganh = tenNganh
                    };

                    _appDbContext.DbSetNganh.Add(nganh);

                    TempData["Success"] = "Thêm ngành thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = nganh.MaNganh });
                }
                else
                {
                    var nganh = await _appDbContext.DbSetNganh
                        .FindAsync(nganhVM.MaNganh);

                    if (nganh == null)
                    {
                        TempData["Error"] = "Ngành không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    nganh.TenNganh = tenNganh;

                    TempData["Success"] = "Cập nhật ngành thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = nganh.MaNganh });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(nganhVM);
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var nganh = await _appDbContext.DbSetNganh.FindAsync(ma);

                if (nganh == null)
                {
                    TempData["Error"] = "Ngành không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetMonHoc
                                .AnyAsync(x => x.MaNganh == ma) ||
                                await _appDbContext.DbSetCBGVTrinhDoNganh
                                .AnyAsync(x => x.MaNganh == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Ngành đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetNganh.Remove(nganh);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa ngành thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
