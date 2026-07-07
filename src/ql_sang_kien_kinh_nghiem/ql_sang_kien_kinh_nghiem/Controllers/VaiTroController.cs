using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class VaiTroController : Controller
    {
        private readonly ILogger<VaiTroController> _logger;
        private readonly AppDbContext _appDbContext;

        public VaiTroController(ILogger<VaiTroController> logger, AppDbContext appDbContext)
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

            int tongSoDong = await _appDbContext.DbSetVaiTro.CountAsync();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / soDongTrenTrang);

            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;
            
            var data = await _appDbContext.DbSetVaiTro
                .OrderBy(x => x.TenVT)
                .Skip((trang - 1) * soDongTrenTrang)
                .Take(soDongTrenTrang)
                .Select(x => new VaiTroVM
                {
                    MaVT = x.MaVT,
                    TenVT = x.TenVT,

                    DaSuDung = _appDbContext.DbSetCBGVVaiTroHoiDong
                        .Any(y => y.MaVT == x.MaVT)
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
            var vaiTroVM = new VaiTroVM();

            if(ma != null)
            {
                var vaiTro = await _appDbContext.DbSetVaiTro
                    .FindAsync(ma);

                if (vaiTro == null)
                {
                    TempData["Error"] = "Vai trò không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                vaiTroVM.MaVT = vaiTro.MaVT;
                vaiTroVM.TenVT = vaiTro.TenVT;
            }

            return View(vaiTroVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(VaiTroVM vaiTroVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(vaiTroVM);
                }

                string tenVaiTro = vaiTroVM.TenVT!.Trim();

                bool isExist = await _appDbContext.DbSetVaiTro
                    .AnyAsync(x => x.TenVT.ToLower() == tenVaiTro.ToLower() && x.MaVT != vaiTroVM.MaVT
                    );

                if (isExist)
                {
                    ModelState.AddModelError(
                        nameof(vaiTroVM.TenVT),
                        "Tên vai trò đã tồn tại"
                    );

                    return View(vaiTroVM);
                }

                if (vaiTroVM.MaVT <= 0)
                {
                    var vaiTro = new VaiTro
                    {
                        TenVT = tenVaiTro
                    };

                    _appDbContext.DbSetVaiTro.Add(vaiTro);

                    TempData["Success"] = "Thêm vai trò thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = vaiTro.MaVT });
                }

                else
                {
                    var vaiTro = await _appDbContext.DbSetVaiTro
                        .FindAsync(vaiTroVM.MaVT);

                    if (vaiTro == null)
                    {
                        TempData["Error"] = "Vai trò không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    vaiTro.TenVT = tenVaiTro;

                    TempData["Success"] = "Cập nhật vai trò thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = vaiTro.MaVT });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(vaiTroVM);
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var vaiTro = await _appDbContext.DbSetVaiTro.FindAsync(ma);

                if (vaiTro == null)
                {
                    TempData["Error"] = "Vai trò không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetCBGVVaiTroHoiDong
                    .AnyAsync(x => x.MaVT == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Vai trò đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetVaiTro.Remove(vaiTro);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa vai trò thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
