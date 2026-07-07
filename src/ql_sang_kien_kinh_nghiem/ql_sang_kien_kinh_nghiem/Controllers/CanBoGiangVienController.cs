using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class CanBoGiangVienController : Controller
    {
        private readonly ILogger<CanBoGiangVienController> _logger;
        private readonly AppDbContext _appDbContext;

        public CanBoGiangVienController(ILogger<CanBoGiangVienController> logger, AppDbContext appDbContext)
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

            int tongSoDong = await _appDbContext.DbSetCanBoGiangVien.CountAsync();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / soDongTrenTrang);

            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;
            
            var data = await _appDbContext.DbSetCanBoGiangVien
                .OrderBy(x => x.HoTen)
                .Skip((trang - 1) * soDongTrenTrang)
                .Take(soDongTrenTrang)
                .Select(x => new CanBoGiangVienVM
                {
                    MaCBGV = x.MaCBGV,
                    HoTen = x.HoTen,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,

                    DaSuDung = _appDbContext.DbSetTaiKhoan
                                .Any(y => y.MaCBGV == x.MaCBGV) ||
                                _appDbContext.DbSetCBGVChucVuDonVi
                                .Any(y => y.MaCBGV == x.MaCBGV) ||
                                _appDbContext.DbSetCBGVTrinhDoNganh
                                .Any(y => y.MaCBGV == x.MaCBGV) ||
                                _appDbContext.DbSetCBGVSangKien
                                .Any(y => y.MaCBGV == x.MaCBGV)||
                                _appDbContext.DbSetCBGVVaiTroHoiDong
                                .Any(y => y.MaCBGV == x.MaCBGV)||
                                _appDbContext.DbSetDanhGia
                                .Any(y => y.MaCBGV == x.MaCBGV)
                })
                .ToListAsync();

            ViewBag.TrangHienTai = trang;
            ViewBag.TongSoTrang = tongSoTrang;

            return View(data);
        }

        [Authorize(Roles = "BGD")]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(string? ma)
        {
            var canBoGiangVienVM = new CanBoGiangVienVM();

            if (ma != null)
            {

                var canBoGiangVien = await _appDbContext.DbSetCanBoGiangVien
                    .FindAsync(ma);

                if (canBoGiangVien == null)
                {
                    TempData["Error"] = "Cán bộ, giảng viên không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                canBoGiangVienVM.MaCBGV = canBoGiangVien.MaCBGV;
                canBoGiangVienVM.HoTen = canBoGiangVien.HoTen;
                canBoGiangVienVM.Email = canBoGiangVien.Email;
                canBoGiangVienVM.SoDienThoai = canBoGiangVien.SoDienThoai;

                canBoGiangVienVM.DSTrinhDoNganh = await _appDbContext.DbSetCBGVTrinhDoNganh
                .Where(x => x.MaCBGV == ma)
                .Select(x => new CBGVTrinhDoNganhVM
                {
                    MaTD = x.MaTD,
                    TenTD = x.TrinhDo.TenTD,
                    MaNganh = x.MaNganh,
                    TenNganh = x.Nganh.TenNganh
                })
                .ToListAsync();

                canBoGiangVienVM.DSChucVuDonVi = await _appDbContext.DbSetCBGVChucVuDonVi
                .Where(x => x.MaCBGV == ma)
                .Select(x => new CBGVChucVuDonViVM
                {
                    MaCV = x.MaCV,
                    TenCV = x.ChucVu.TenCV,
                    MaDV = x.MaDV,
                    TenDV = x.DonVi.TenDV,
                    ThoiGianBatDau = x.ThoiGianBatDau,
                    ThoiGianKetThuc = x.ThoiGianKetThuc
                })
                .ToListAsync();

                canBoGiangVienVM.DSTrinhDo= await _appDbContext.DbSetTrinhDo
                    .OrderBy(x => x.TenTD)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaTD.ToString(),
                        Text = x.TenTD
                    })
                    .ToListAsync();

                canBoGiangVienVM.DSNganh= await _appDbContext.DbSetNganh
                    .OrderBy(x => x.TenNganh)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaNganh.ToString(),
                        Text = x.TenNganh
                    })
                    .ToListAsync();

                canBoGiangVienVM.DSChucVu= await _appDbContext.DbSetChucVu
                    .Where(x => x.MaCV != 1)
                    .OrderBy(x => x.TenCV)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaCV.ToString(),
                        Text = x.TenCV
                    })
                    .ToListAsync();

                canBoGiangVienVM.DSDonVi= await _appDbContext.DbSetDonVi
                    .OrderBy(x => x.TenDV)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaDV.ToString(),
                        Text = x.TenDV
                    })
                    .ToListAsync();
            }

            return View(canBoGiangVienVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(CanBoGiangVienVM canBoGiangVienVM)
        {
            try
            {
                ModelState.Remove(nameof(canBoGiangVienVM.MaCBGV));

                if (!ModelState.IsValid)
                {
                    return View(canBoGiangVienVM);
                }

                string? maCuoi = await _appDbContext.DbSetCanBoGiangVien
                    .OrderByDescending(x => x.MaCBGV)
                    .Select(x => x.MaCBGV)
                    .FirstOrDefaultAsync();

                int soMoi = 1;

                if (!string.IsNullOrWhiteSpace(maCuoi))
                {
                    soMoi = int.Parse(maCuoi) + 1;
                }

                string maCBGV = soMoi.ToString("D6");
                string hoTen = canBoGiangVienVM.HoTen!.Trim();
                string email = canBoGiangVienVM.Email!.Trim();
                string soDienThoai = canBoGiangVienVM.SoDienThoai!.Trim();

                bool isEmailExist = await _appDbContext.DbSetCanBoGiangVien
                    .AnyAsync(x =>
                        x.Email == email &&
                        x.MaCBGV != canBoGiangVienVM.MaCBGV
                    );

                if (isEmailExist)
                {
                    ModelState.AddModelError(
                        nameof(canBoGiangVienVM.Email),
                        "Email đã tồn tại"
                    );

                    return View(canBoGiangVienVM);
                }

                bool isSDTExist = await _appDbContext.DbSetCanBoGiangVien
                    .AnyAsync(x =>
                        x.SoDienThoai == soDienThoai &&
                        x.MaCBGV != canBoGiangVienVM.MaCBGV
                    );

                if (isSDTExist)
                {
                    ModelState.AddModelError(
                        nameof(canBoGiangVienVM.SoDienThoai),
                        "Số điện thoại đã tồn tại"
                    );

                    return View(canBoGiangVienVM);
                }

                if (string.IsNullOrWhiteSpace(canBoGiangVienVM.MaCBGV))
                {
                    var canBoGiangVien = new CanBoGiangVien
                    {
                        MaCBGV = maCBGV,
                        HoTen = hoTen,
                        Email = email,
                        SoDienThoai = soDienThoai
                    };

                    _appDbContext.DbSetCanBoGiangVien.Add(canBoGiangVien);

                    await _appDbContext.SaveChangesAsync();

                    TempData["Success"] = "Thêm cán bộ, giảng viên thành công";
                    
                    return RedirectToAction(nameof(ChiTiet), new { ma = maCBGV });
                }
                else
                {
                    var canBoGiangVien = await _appDbContext.DbSetCanBoGiangVien
                        .FindAsync(canBoGiangVienVM.MaCBGV);

                    if (canBoGiangVien == null)
                    {
                        TempData["Error"] = "Cán bộ, giảng viên không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    canBoGiangVien.HoTen = hoTen;
                    canBoGiangVien.Email = email;
                    canBoGiangVien.SoDienThoai = soDienThoai;

                    await _appDbContext.SaveChangesAsync();
                
                    TempData["Success"] = "Cập nhật cán bộ, giảng viên thành công";

                    return RedirectToAction(nameof(ChiTiet), new { ma = canBoGiangVienVM.MaCBGV });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(canBoGiangVienVM);
            }
        }

        [HttpPost]
        [Authorize(Roles = "BGD")]
        public async Task<IActionResult> Xoa(string ma)
        {
            try
            {
                var canBoGiangVien = await _appDbContext.DbSetCanBoGiangVien.FindAsync(ma);

                if (canBoGiangVien == null)
                {
                    TempData["Error"] = "Cán bộ, giảng viên không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetTaiKhoan
                                .AnyAsync(x => x.MaCBGV == ma) ||
                                await _appDbContext.DbSetCBGVChucVuDonVi
                                .AnyAsync(x => x.MaCBGV == ma) ||
                                await _appDbContext.DbSetCBGVTrinhDoNganh
                                .AnyAsync(x => x.MaCBGV == ma) ||
                                await _appDbContext.DbSetCBGVSangKien
                                .AnyAsync(x => x.MaCBGV == ma) ||
                                await _appDbContext.DbSetCBGVVaiTroHoiDong
                                .AnyAsync(x => x.MaCBGV == ma) ||
                                await _appDbContext.DbSetDanhGia
                                .AnyAsync(x => x.MaCBGV == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Cán bộ, giảng viên đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetCanBoGiangVien.Remove(canBoGiangVien);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa cán bộ, giảng viên thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }     

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> LuuCBGVTDN(CBGVTrinhDoNganhVM cBGVTrinhDoNganhVM)
        {
            try
            {
                bool isCBGVExist = await _appDbContext.DbSetCanBoGiangVien.AnyAsync(x => x.MaCBGV == cBGVTrinhDoNganhVM.MaCBGV);
                bool isTDExist = await _appDbContext.DbSetTrinhDo.AnyAsync(x => x.MaTD == cBGVTrinhDoNganhVM.MaTD!.Value);
                bool isNganhExist = await _appDbContext.DbSetNganh.AnyAsync(x => x.MaNganh == cBGVTrinhDoNganhVM.MaNganh!.Value);

                if (!isCBGVExist)
                {
                    TempData["Error"] = "Cán bộ, giảng viên không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng nhập đủ thông tin";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVTrinhDoNganhVM.MaCBGV });
                }

                if (!isTDExist)
                {
                    TempData["Error"] = "Trình độ không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVTrinhDoNganhVM.MaCBGV });
                }

                if (!isNganhExist)
                {
                    TempData["Error"] = "Ngành không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVTrinhDoNganhVM.MaCBGV });
                }

                bool isExist = await _appDbContext.DbSetCBGVTrinhDoNganh
                            .AnyAsync(x => x.MaCBGV == cBGVTrinhDoNganhVM.MaCBGV &&
                            x.MaTD == cBGVTrinhDoNganhVM.MaTD &&
                            x.MaNganh == cBGVTrinhDoNganhVM.MaNganh);
                
                if (isExist)
                {
                    TempData["Error"] = "Trình độ - ngành đã tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVTrinhDoNganhVM.MaCBGV });
                }

                var cBGVTrinhDoNganh = new CBGVTrinhDoNganh
                {
                    MaCBGV = cBGVTrinhDoNganhVM.MaCBGV!,
                    MaTD = cBGVTrinhDoNganhVM.MaTD!.Value,
                    MaNganh = cBGVTrinhDoNganhVM.MaNganh!.Value
                };

                _appDbContext.DbSetCBGVTrinhDoNganh.Add(cBGVTrinhDoNganh);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Thêm trình độ - ngành thành công";
            } catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVTrinhDoNganhVM.MaCBGV });
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> LuuCBGVCVDV(CBGVChucVuDonViVM cBGVChucVuDonViVM)
        {
            try
            {
                var maCV = cBGVChucVuDonViVM.MaCV ?? 0;

                if(maCV == 0)
                {
                    maCV = 1;
                }

                bool isCBGVExist = await _appDbContext.DbSetCanBoGiangVien.AnyAsync(x => x.MaCBGV == cBGVChucVuDonViVM.MaCBGV);
                bool isTDExist = await _appDbContext.DbSetChucVu.AnyAsync(x => x.MaCV == maCV);
                bool isNganhExist = await _appDbContext.DbSetDonVi.AnyAsync(x => x.MaDV == cBGVChucVuDonViVM.MaDV!.Value);

                if (!isCBGVExist)
                {
                    TempData["Error"] = "Cán bộ, giảng viên không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng nhập đủ thông tin";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVChucVuDonViVM.MaCBGV });
                }

                if (!isTDExist)
                {
                    TempData["Error"] = "Chức vụ không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVChucVuDonViVM.MaCBGV });
                }

                if (!isNganhExist)
                {
                    TempData["Error"] = "Đơn vị không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVChucVuDonViVM.MaCBGV });
                }

                bool isExist = await _appDbContext.DbSetCBGVChucVuDonVi
                            .AnyAsync(x => x.MaCBGV == cBGVChucVuDonViVM.MaCBGV &&
                            x.MaCV == maCV &&
                            x.MaDV == cBGVChucVuDonViVM.MaDV);
                
                if (isExist)
                {
                    TempData["Error"] = "Chức vụ - đơn vị đã tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVChucVuDonViVM.MaCBGV });
                }

                var cBGVChucVuDonVi = new CBGVChucVuDonVi
                {
                    MaCBGV = cBGVChucVuDonViVM.MaCBGV!,
                    MaCV = maCV,
                    MaDV = cBGVChucVuDonViVM.MaDV!.Value,
                    ThoiGianBatDau = cBGVChucVuDonViVM.ThoiGianBatDau!.Value,
                    ThoiGianKetThuc = cBGVChucVuDonViVM.ThoiGianKetThuc
                };

                _appDbContext.DbSetCBGVChucVuDonVi.Add(cBGVChucVuDonVi);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Thêm chức vụ - đơn vị thành công";
            } catch 
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVChucVuDonViVM.MaCBGV });
        }  

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> XoaCBGVTDN(CBGVTrinhDoNganhVM cBGVTrinhDoNganhVM)
        {
            try
            {
                var cBGVTrinhDoNganh = await _appDbContext.DbSetCBGVTrinhDoNganh
                                        .FirstOrDefaultAsync(x =>
                                            x.MaCBGV == cBGVTrinhDoNganhVM.MaCBGV
                                            && x.MaTD == cBGVTrinhDoNganhVM.MaTD
                                            && x.MaNganh == cBGVTrinhDoNganhVM.MaNganh);

                if (cBGVTrinhDoNganh == null)
                {
                    TempData["Error"] = "Trình độ - ngành không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVTrinhDoNganhVM.MaCBGV });
                }

                _appDbContext.DbSetCBGVTrinhDoNganh.Remove(cBGVTrinhDoNganh);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa trình độ - ngành thành công";
            } catch 
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVTrinhDoNganhVM.MaCBGV });
        }  

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> XoaCBGVCVDV(CBGVChucVuDonViVM cBGVChucVuDonViVM)
        {
            try
            {
                var cBGVChucVuDonVi = await _appDbContext.DbSetCBGVChucVuDonVi
                                        .FirstOrDefaultAsync(x =>
                                            x.MaCBGV == cBGVChucVuDonViVM.MaCBGV
                                            && x.MaCV == cBGVChucVuDonViVM.MaCV
                                            && x.MaDV == cBGVChucVuDonViVM.MaDV);

                if (cBGVChucVuDonVi == null)
                {
                    TempData["Error"] = "Chức vụ - đơn vị không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = cBGVChucVuDonViVM.MaCBGV });
                }

                _appDbContext.DbSetCBGVChucVuDonVi.Remove(cBGVChucVuDonVi);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa chức vụ - đơn vị thành công";
            } catch 
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(ChiTiet), new { ma = cBGVChucVuDonViVM.MaCBGV });
        }  
    }
}
