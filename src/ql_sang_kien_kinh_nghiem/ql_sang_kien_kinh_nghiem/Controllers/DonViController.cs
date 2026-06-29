using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class DonViController : Controller
    {
        private readonly ILogger<DonViController> _logger;
        private readonly AppDbContext _appDbContext;

        public DonViController(ILogger<DonViController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        [Authorize(Roles = "BGD")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _appDbContext.DbSetDonVi
                .OrderBy(x => x.TenDV)
                .Select(x => new DonViVM
                {
                    MaDV = x.MaDV,
                    TenDV = x.TenDV,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    TenDVCapTren = x.MaDVCha != null ? _appDbContext.DbSetDonVi
                        .Where(y => y.MaDV == x.MaDVCha)
                        .Select(y => y.TenDV)
                        .FirstOrDefault() ?? string.Empty : string.Empty,
                    DaSuDung = _appDbContext.DbSetCBGVChucVuDonVi.Any(y => y.MaDV == x.MaDV) ||
                               _appDbContext.DbSetDonVi.Any(y => y.MaDVCha == x.MaDV)
                })
                .ToListAsync();

            return View(data);
        }

        [Authorize(Roles = "BGD")]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int? ma)
        {
            var donViVM = new DonViVM();

            donViVM.DSDonViCha = await _appDbContext.DbSetDonVi
                .OrderBy(x => x.TenDV)
                .Select(x => new SelectListItem
                {
                    Value = x.MaDV.ToString(),
                    Text = x.TenDV
                })
                .ToListAsync();

            if (ma != null)
            {
                donViVM.DSDonViCha = await _appDbContext.DbSetDonVi
                    .Where(x => x.MaDV != ma && x.MaDVCha != ma)
                    .OrderBy(x => x.TenDV)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaDV.ToString(),
                        Text = x.TenDV
                    })
                    .ToListAsync();
                
                donViVM.DSDonViCon = await _appDbContext.DbSetDonVi
                    .Where(x => x.MaDVCha == ma)
                    .OrderBy(x => x.TenDV)
                    .Select(x => new DonViVM
                    {
                        MaDV = x.MaDV,
                        TenDV = x.TenDV,
                        Email = x.Email,
                        SoDienThoai = x.SoDienThoai,
                        MaDVCha = x.MaDVCha,
                        DaSuDung = _appDbContext.DbSetCBGVChucVuDonVi.Any(y => y.MaDV == x.MaDV) ||
                                   _appDbContext.DbSetDonVi.Any(y => y.MaDVCha == x.MaDV)
                    })
                    .ToListAsync();

                donViVM.DSDonViConSelect = await _appDbContext.DbSetDonVi
                    .Where(x => x.MaDVCha == null && x.MaDV != ma)
                    .OrderBy(x => x.TenDV)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaDV.ToString(),
                        Text = x.TenDV
                    })
                    .ToListAsync();

                var donVi = await _appDbContext.DbSetDonVi
                    .FindAsync(ma);

                if (donVi == null)
                {
                    TempData["Error"] = "Đơn vị không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                donViVM.MaDV = donVi.MaDV;
                donViVM.TenDV = donVi.TenDV;
                donViVM.Email = donVi.Email;
                donViVM.SoDienThoai = donVi.SoDienThoai;
                donViVM.MaDVCha = donVi.MaDVCha;
                donViVM.DaSuDung = _appDbContext.DbSetCBGVChucVuDonVi.Any(y => y.MaDV == donVi.MaDV) ||
                                   _appDbContext.DbSetDonVi.Any(y => y.MaDVCha == donVi.MaDV);
            }

            return View(donViVM);
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(DonViVM donViVM)
        {
            try
            {
                ModelState.Remove(nameof(donViVM.MaDVCon));

                if(!ModelState.IsValid)
                {
                    donViVM.DSDonViCha = await _appDbContext.DbSetDonVi
                    .OrderBy(x => x.TenDV)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaDV.ToString(),
                        Text = x.TenDV
                    })
                    .ToListAsync();

                    if (donViVM.MaDV > 0)
                    {
                        donViVM.DSDonViCha = await _appDbContext.DbSetDonVi
                            .Where(x => x.MaDV != donViVM.MaDV && x.MaDVCha != donViVM.MaDV)
                            .OrderBy(x => x.TenDV)
                            .Select(x => new SelectListItem
                        {
                            Value = x.MaDV.ToString(),
                            Text = x.TenDV
                        })
                        .ToListAsync();
                    }

                    return View(donViVM);
                }

                string tenDonVi = donViVM.TenDV!.Trim();
                string email = donViVM.Email!.Trim();
                string soDienThoai = donViVM.SoDienThoai!.Trim();

                bool isTenDonViExist = await _appDbContext.DbSetDonVi
                    .AnyAsync(x => x.TenDV == tenDonVi && x.MaDV != donViVM.MaDV);

                bool isEmailExist = await _appDbContext.DbSetDonVi
                    .AnyAsync(x => x.Email == email && x.MaDV != donViVM.MaDV);

                bool isSoDienThoaiExist = await _appDbContext.DbSetDonVi
                    .AnyAsync(x => x.SoDienThoai == soDienThoai && x.MaDV != donViVM.MaDV);

                if (isTenDonViExist)
                {
                    ModelState.AddModelError(
                        nameof(donViVM.TenDV), 
                        "Tên đơn vị đã tồn tại"
                    );
                }

                if (isEmailExist)
                {
                    ModelState.AddModelError(
                        nameof(donViVM.Email),
                        "Email đã tồn tại"
                    );
                }

                if (isSoDienThoaiExist)
                {
                    ModelState.AddModelError(
                        nameof(donViVM.SoDienThoai),
                        "Số điện thoại đã tồn tại"
                    );
                }

                if (isTenDonViExist || isEmailExist || isSoDienThoaiExist) {
                    donViVM.DSDonViCha = await _appDbContext.DbSetDonVi
                    .OrderBy(x => x.TenDV)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaDV.ToString(),
                        Text = x.TenDV
                    })
                    .ToListAsync();

                    if (donViVM.MaDV > 0)
                    {
                        donViVM.DSDonViCha = await _appDbContext.DbSetDonVi
                            .Where(x => x.MaDV != donViVM.MaDV && x.MaDVCha != donViVM.MaDV)
                            .OrderBy(x => x.TenDV)
                            .Select(x => new SelectListItem
                        {
                            Value = x.MaDV.ToString(),
                            Text = x.TenDV
                        })
                        .ToListAsync();
                    }

                    return View(donViVM);
                }

                if (donViVM.MaDV <= 0)
                {
                    var donVi = new DonVi
                    {
                        TenDV = tenDonVi,
                        Email = email,
                        SoDienThoai = soDienThoai,
                        MaDVCha = donViVM.MaDVCha
                    };

                    _appDbContext.DbSetDonVi.Add(donVi);

                    TempData["Success"] = "Thêm đơn vị thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = donVi.MaDV });
                }
                else
                {
                    var donVi = await _appDbContext.DbSetDonVi
                        .FindAsync(donViVM.MaDV);

                    if (donVi == null)
                    {
                        TempData["Error"] = "Đơn vị không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    donVi.TenDV = tenDonVi;
                    donVi.Email = email;
                    donVi.SoDienThoai = soDienThoai;
                    donVi.MaDVCha = donViVM.MaDVCha;

                    TempData["Success"] = "Cập nhật đơn vị thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = donVi.MaDV });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(donViVM);
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> LuuDonViCon(DonViVM donViVM)
        {
            try
            {
                var donViCha = await _appDbContext.DbSetDonVi.FindAsync(donViVM.MaDVCha);
                var donViCon = await _appDbContext.DbSetDonVi.FindAsync(donViVM.MaDVCon);

                if (donViCha == null)
                {
                    TempData["Error"] = "Đơn vị quản lý không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = donViVM.MaDVCha });
                }

                if (donViCon == null)
                {
                    TempData["Error"] = "Đơn vị trực thuộc không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = donViVM.MaDVCha });
                }

                donViCon.MaDVCha = donViCha.MaDV;

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Thêm đơn vị trực thuộc thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = donViVM.MaDVCha });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
                return RedirectToAction(nameof(ChiTiet), new { ma = donViVM.MaDVCha });
            }
        }

        [Authorize(Roles = "BGD")]
        [HttpPost]
        public async Task<IActionResult> XoaDonViCon(int maDVCon)
        {
            try
            {
                var donViCon = await _appDbContext.DbSetDonVi.FindAsync(maDVCon);

                if (donViCon == null)
                {
                    TempData["Error"] = "Đơn vị trực thuộc không tồn tại";
                    return RedirectToAction(nameof(ChiTiet), new { ma = maDVCon });
                }

                donViCon.MaDVCha = null;

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa đơn vị trực thuộc thành công";

                return RedirectToAction(nameof(ChiTiet), new { ma = maDVCon });
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
                return RedirectToAction(nameof(ChiTiet), new { ma = maDVCon });
            }
        }

        public async Task<IActionResult> Xoa(int ma)
        {
            try
            {
                var donVi = await _appDbContext.DbSetDonVi.FindAsync(ma);

                if (donVi == null)
                {
                    TempData["Error"] = "Đơn vị không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                bool daSuDung = await _appDbContext.DbSetCBGVChucVuDonVi.AnyAsync(x => x.MaDV == ma) ||
                                await _appDbContext.DbSetDonVi.AnyAsync(x => x.MaDVCha == ma);

                if (daSuDung)
                {
                    TempData["Error"] = "Đơn vị đã được sử dụng, không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                _appDbContext.DbSetDonVi.Remove(donVi);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa đơn vị thành công";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
