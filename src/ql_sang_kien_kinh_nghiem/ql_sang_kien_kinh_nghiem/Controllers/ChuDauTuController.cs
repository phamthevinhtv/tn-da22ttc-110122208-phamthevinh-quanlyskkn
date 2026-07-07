using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class ChuDauTuController : Controller
    {
        private readonly ILogger<ChuDauTuController> _logger;
        private readonly AppDbContext _appDbContext;

        public ChuDauTuController(ILogger<ChuDauTuController> logger, AppDbContext appDbContext)
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

            int tongSoDong = await _appDbContext.DbSetChuDauTu.CountAsync();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / soDongTrenTrang);

            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;
            
            var data = await _appDbContext.DbSetChuDauTu
                .OrderBy(x => x.TenCDT)
                .Skip((trang - 1) * soDongTrenTrang)
                .Take(soDongTrenTrang)
                .Select(x => new ChuDauTuVM
                {
                    MaCDT = x.MaCDT,
                    TenCDT = x.TenCDT,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,

                    DaSuDung = _appDbContext.DbSetSangKien
                        .Any(y => y.MaCDT == x.MaCDT)
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
            var chuDauTuVM = new ChuDauTuVM();

            if (ma != null)
            {
                var chuDauTu = await _appDbContext.DbSetChuDauTu
                    .FindAsync(ma);

                if (chuDauTu == null)
                {
                    TempData["Error"] = "Chủ đầu tư không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                chuDauTuVM.MaCDT = chuDauTu.MaCDT;
                chuDauTuVM.TenCDT = chuDauTu.TenCDT;
                chuDauTuVM.Email = chuDauTu.Email;
                chuDauTuVM.SoDienThoai = chuDauTu.SoDienThoai;
                chuDauTuVM.DiaChi = chuDauTu.DiaChi;
            }

            return View(chuDauTuVM);
        }

        [HttpPost]
        [Authorize(Roles = "BGD")]
        public async Task<IActionResult> ChiTiet(ChuDauTuVM chuDauTuVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(chuDauTuVM);
                }

                string tenCDT = chuDauTuVM.TenCDT!.Trim();
                string emailCDT = chuDauTuVM.Email!.Trim();
                string soDienThoaiCDT = chuDauTuVM.SoDienThoai!.Trim();
                string diaChiCDT = chuDauTuVM.DiaChi!.Trim();

                bool isEmailExist = await _appDbContext.DbSetChuDauTu
                    .AnyAsync(x =>
                        x.Email == emailCDT &&
                        x.MaCDT != chuDauTuVM.MaCDT
                    );

                if (isEmailExist)
                {
                    ModelState.AddModelError(
                        nameof(chuDauTuVM.Email),
                        "Email đã tồn tại"
                    );

                    return View(chuDauTuVM);
                }

                bool isSDTExist = await _appDbContext.DbSetChuDauTu
                    .AnyAsync(x =>
                        x.SoDienThoai == soDienThoaiCDT &&
                        x.MaCDT != chuDauTuVM.MaCDT
                    );

                if (isSDTExist)
                {
                    ModelState.AddModelError(
                        nameof(chuDauTuVM.SoDienThoai),
                        "Số điện thoại đã tồn tại"
                    );

                    return View(chuDauTuVM);
                }

                if (chuDauTuVM.MaCDT <= 0)
                {
                    var chuDauTu = new ChuDauTu
                    {
                        TenCDT = tenCDT,
                        Email = emailCDT,
                        SoDienThoai = soDienThoaiCDT,
                        DiaChi = diaChiCDT,
                    };

                    _appDbContext.DbSetChuDauTu.Add(chuDauTu);

                    await _appDbContext.SaveChangesAsync();

                    TempData["Success"] = "Thêm chủ đầu tư thành công";

                    return RedirectToAction(nameof(ChiTiet), new { ma = chuDauTu.MaCDT });
                }
                else
                {
                    var chuDauTu = await _appDbContext.DbSetChuDauTu
                        .FindAsync(chuDauTuVM.MaCDT);

                    if (chuDauTu == null)
                    {
                        TempData["Error"] = "Chủ đầu tư không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    chuDauTu.TenCDT = tenCDT;
                    chuDauTu.Email = emailCDT;
                    chuDauTu.SoDienThoai = soDienThoaiCDT;
                    chuDauTu.DiaChi = diaChiCDT;

                    await _appDbContext.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật chủ đầu tư thành công";

                    return RedirectToAction(nameof(ChiTiet), new { ma = chuDauTu.MaCDT });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(chuDauTuVM);
            }
        }

        [HttpPost]
        [Authorize(Roles = "BGD")]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var chuDauTu = await _appDbContext.DbSetChuDauTu.FindAsync(ma);

                if (chuDauTu == null)
                {
                    TempData["Error"] = "Chủ đầu tư không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetSangKien
                    .AnyAsync(x => x.MaCDT == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Chủ đầu tư đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetChuDauTu.Remove(chuDauTu);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa chủ đầu tư thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }     
    }
}
