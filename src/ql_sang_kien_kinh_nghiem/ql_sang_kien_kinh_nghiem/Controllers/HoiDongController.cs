using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class HoiDongController : Controller
    {
        private readonly ILogger<HoiDongController> _logger;
        private readonly AppDbContext _appDbContext;

        public HoiDongController(ILogger<HoiDongController> logger, AppDbContext appDbContext)
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

            int tongSoDong = await _appDbContext.DbSetHoiDong.CountAsync();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / soDongTrenTrang);

            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;
            
            var data = await _appDbContext.DbSetHoiDong
                .OrderByDescending(x => x.NgayLap)
                .Skip((trang - 1) * soDongTrenTrang)
                .Take(soDongTrenTrang)
                .Select(x => new HoiDongVM
                {
                    MaHD = x.MaHD,
                    TenHD = x.TenHD,
                    CanCu = x.CanCu,
                    DieuKhoan = x.DieuKhoan,
                    NgayLap = x.NgayLap,
                    NgayKetThuc = x.NgayKetThuc,
                    DaSuDung = _appDbContext.DbSetCBGVVaiTroHoiDong.Any(y => y.MaHD == x.MaHD) || 
                               _appDbContext.DbSetDanhGia.Any(y => y.MaHD == x.MaHD)
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
            var hoiDongVM = new HoiDongVM();

            if (ma != null)
            {
                hoiDongVM.DSCanBoGiangVien = await _appDbContext.DbSetCanBoGiangVien
                    .Where(x => !x.DSCBGVVaiTroHoiDong.Any(y => y.MaHD == ma))
                    .OrderBy(x => x.MaCBGV)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaCBGV.ToString(),
                        Text = $"{x.MaCBGV} - {x.HoTen}"
                    })
                    .ToListAsync();

                hoiDongVM.DSVaiTro = await _appDbContext.DbSetVaiTro
                    .Where(x => !x.DSCBGVVaiTroHoiDong.Any(y => y.MaHD == ma) || x.MaVT != 1)
                    .OrderBy(x => x.MaVT)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaVT.ToString(),
                        Text = x.TenVT
                    })
                    .ToListAsync();

                var hoiDong = await _appDbContext.DbSetHoiDong
                    .FindAsync((ma));

                if (hoiDong == null)
                {
                    TempData["Error"] = "Hội đồng không tồn tại";
                    return RedirectToAction(nameof(Index));
                }
                
                hoiDongVM.MaHD = hoiDong.MaHD;        
                hoiDongVM.TenHD = hoiDong.TenHD;        
                hoiDongVM.CanCu = hoiDong.CanCu;        
                hoiDongVM.DieuKhoan = hoiDong.DieuKhoan;
                hoiDongVM.NgayLap = hoiDong.NgayLap;
                hoiDongVM.NgayKetThuc = hoiDong.NgayKetThuc;

                hoiDongVM.DSCBGVVaiTro = await _appDbContext.DbSetCBGVVaiTroHoiDong
                .Where(x => x.MaHD == ma)
                .OrderBy(x => x.MaVT)
                .Select(x => new CBGVVaiTroHoiDongVM
                {
                    MaHD = x.MaHD,
                    MaCBGV = x.MaCBGV,
                    HoTen = x.CanBoGiangVien.HoTen,
                    MaVT = x.MaVT,
                    TenVT = x.VaiTro.TenVT,
                })
                .ToListAsync();        
            }

            return View(hoiDongVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(HoiDongVM hoiDongVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(hoiDongVM);
                }

                string tenHD = hoiDongVM.TenHD!.Trim();
                string canCu = hoiDongVM.CanCu!.Trim();
                string dieuKhoan = hoiDongVM.DieuKhoan!.Trim();
                DateTime ngayLap = hoiDongVM.NgayLap!.Value;
                DateTime ngayKetThuc = hoiDongVM.NgayKetThuc!.Value;

                if (hoiDongVM.MaHD <= 0)
                {
                    var hoiDong = new HoiDong
                    {
                        TenHD = tenHD,
                        CanCu = canCu,
                        DieuKhoan = dieuKhoan,
                        NgayLap = ngayLap,
                        NgayKetThuc = ngayKetThuc
                    };

                    _appDbContext.DbSetHoiDong.Add(hoiDong);

                    await _appDbContext.SaveChangesAsync();

                    TempData["Success"] = "Thêm hội đồng thành công";

                    return RedirectToAction(nameof(ChiTiet), new { ma = hoiDong.MaHD });
                }
                else
                {
                    var hoiDong = await _appDbContext.DbSetHoiDong
                        .FindAsync(hoiDongVM.MaHD);

                    if (hoiDong == null)
                    {
                        TempData["Error"] = "Hội đồng không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    hoiDong.TenHD = tenHD;
                    hoiDong.CanCu = canCu;
                    hoiDong.DieuKhoan = dieuKhoan;
                    hoiDong.NgayLap = ngayLap;
                    hoiDong.NgayKetThuc = ngayKetThuc;

                    await _appDbContext.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật hội đồng thành công";

                    return RedirectToAction(nameof(ChiTiet), new { ma = hoiDong.MaHD });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(hoiDongVM);
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> LuuCBGVVT(CBGVVaiTroHoiDongVM cBGVVaiTroHoiDongVM)
        {
            try
            {
                bool isCBGVExist = await _appDbContext.DbSetCanBoGiangVien.AnyAsync(x => x.MaCBGV == cBGVVaiTroHoiDongVM.MaCBGV);
                bool isVTExist = await _appDbContext.DbSetVaiTro.AnyAsync(x => x.MaVT == cBGVVaiTroHoiDongVM.MaVT);
                bool isHDExist = await _appDbContext.DbSetHoiDong.AnyAsync(x => x.MaHD == cBGVVaiTroHoiDongVM.MaHD);
                
                if (!isHDExist)
                {
                    TempData["Error"] = "Hội đồng không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng nhập đủ thông tin";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
                }

                if (!isCBGVExist)
                {
                    TempData["Error"] = "Thành viên không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
                }

                if (!isVTExist)
                {
                    TempData["Error"] = "Vai trò không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
                }

                bool isExist = await _appDbContext.DbSetCBGVVaiTroHoiDong
                            .AnyAsync(x => x.MaHD == cBGVVaiTroHoiDongVM.MaHD &&
                            x.MaCBGV == cBGVVaiTroHoiDongVM.MaCBGV &&
                            x.MaVT == cBGVVaiTroHoiDongVM.MaVT);
                
                if (isExist)
                {
                    TempData["Error"] = "Thành viên với vai trò này đã tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
                }

                var cBGVVaiTroHoiDong = new CBGVVaiTroHoiDong
                {
                    MaHD = cBGVVaiTroHoiDongVM.MaHD,
                    MaCBGV = cBGVVaiTroHoiDongVM.MaCBGV!,
                    MaVT = cBGVVaiTroHoiDongVM.MaVT!.Value,
                };

                _appDbContext.DbSetCBGVVaiTroHoiDong.Add(cBGVVaiTroHoiDong);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Thêm thành viên thành công";
            } catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
        }


        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> XoaThanhVien(CBGVVaiTroHoiDongVM cBGVVaiTroHoiDongVM)
        {
            try
            {
                bool isCBGVExist = await _appDbContext.DbSetCanBoGiangVien.AnyAsync(x => x.MaCBGV == cBGVVaiTroHoiDongVM.MaCBGV);
                bool isVTExist = await _appDbContext.DbSetVaiTro.AnyAsync(x => x.MaVT == cBGVVaiTroHoiDongVM.MaVT);
                bool isHDExist = await _appDbContext.DbSetHoiDong.AnyAsync(x => x.MaHD == cBGVVaiTroHoiDongVM.MaHD);
                
                if (!isHDExist)
                {
                    TempData["Error"] = "Hội đồng không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                if (!isCBGVExist)
                {
                    TempData["Error"] = "Thành viên không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
                }

                if (!isVTExist)
                {
                    TempData["Error"] = "Vai trò không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
                }

                var cBGVVaiTroHoiDong = await _appDbContext.DbSetCBGVVaiTroHoiDong
                            .FirstOrDefaultAsync(x => x.MaHD == cBGVVaiTroHoiDongVM.MaHD &&
                            x.MaCBGV == cBGVVaiTroHoiDongVM.MaCBGV &&
                            x.MaVT == cBGVVaiTroHoiDongVM.MaVT);
                
                if (cBGVVaiTroHoiDong == null)
                {
                    TempData["Error"] = "Thành viên không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
                }

                _appDbContext.DbSetCBGVVaiTroHoiDong.Remove(cBGVVaiTroHoiDong);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa thành viên thành công";
            } catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVVaiTroHoiDongVM.MaHD });
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var hoiDong = await _appDbContext.DbSetHoiDong.FindAsync(ma);

                if (hoiDong == null)
                {
                    TempData["Error"] = "Hội đồng không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetCBGVVaiTroHoiDong.AnyAsync(x => x.MaHD == ma) ||
                                await _appDbContext.DbSetDanhGia.AnyAsync(x => x.MaHD == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Hội đồng đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetHoiDong.Remove(hoiDong);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa hội đồng thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
