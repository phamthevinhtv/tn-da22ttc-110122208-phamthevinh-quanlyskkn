using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;
using ql_sang_kien_kinh_nghiem.Models;
using ql_sang_kien_kinh_nghiem.Services;
using ql_sang_kien_kinh_nghiem.ViewModels;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ILogger<TaiKhoanController> _logger;
        private readonly AppDbContext _appDbContext;
        private readonly EmailService _emailService;

        public TaiKhoanController(ILogger<TaiKhoanController> logger, AppDbContext appDbContext, EmailService emailService)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _emailService = emailService;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _appDbContext.DbSetTaiKhoan
                .OrderBy(x => x.TenDangNhap)
                .Select(x => new TaiKhoanVM
                {
                    MaTK = x.MaTK,
                    MaCBGV = x.MaCBGV,
                    TenDangNhap = x.TenDangNhap,
                    Quyen = x.Quyen,
                    TrangThaiTK = x.TrangThaiTK,
                    HoTen = x.CanBoGiangVien != null ? x.CanBoGiangVien.HoTen : string.Empty
                })
                .ToListAsync();

            return View(data);
        }

        [HttpGet]
        public IActionResult DangNhap()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home"); 
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(DangNhapVM dangNhapVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(dangNhapVM);
                }

                var taiKhoan = await _appDbContext.DbSetTaiKhoan
                    .Include(x => x.CanBoGiangVien)
                    .FirstOrDefaultAsync(x => x.TenDangNhap.ToLower() == dangNhapVM.TenDangNhap!.Trim().ToLower());
            
                if (taiKhoan == null)
                {
                    ModelState.AddModelError(
                        nameof(dangNhapVM.TenDangNhap),
                        "Sai tên đăng nhập"
                    );

                    return View(dangNhapVM);
                }

                if (taiKhoan.TrangThaiTK == 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Tài khoản đã bị khóa"
                    );

                    return View(dangNhapVM);
                }

                var hasher = new PasswordHasher<TaiKhoan>();

                var result = hasher.VerifyHashedPassword(
                    taiKhoan,
                    taiKhoan.MatKhau,
                    dangNhapVM.MatKhau!.Trim()
                );

                if (result == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(
                        nameof(dangNhapVM.MatKhau),
                        "Sai mật khẩu"
                    );

                    return View(dangNhapVM);
                }

                var maVaiTroHD = string.Empty;
                var tenVaiTroHD = string.Empty;

                var now = DateTime.Now;
                var activeHoiDong = await _appDbContext.DbSetHoiDong
                    .FirstOrDefaultAsync(h => h.NgayLap <= now && h.NgayKetThuc >= now);

                if (activeHoiDong != null && !string.IsNullOrEmpty(taiKhoan.MaCBGV))
                {
                    var cbgvVaiTro = await _appDbContext.DbSetCBGVVaiTroHoiDong
                        .Include(x => x.VaiTro)
                        .FirstOrDefaultAsync(x => x.MaHD == activeHoiDong.MaHD && x.MaCBGV == taiKhoan.MaCBGV);

                    if (cbgvVaiTro != null)
                    {
                        maVaiTroHD = cbgvVaiTro.MaVT.ToString();
                        tenVaiTroHD = cbgvVaiTro.VaiTro?.TenVT ?? string.Empty;
                    }
                }

                var claims = new List<Claim>
                {
                    new Claim(
                        ClaimTypes.Name,
                        taiKhoan.CanBoGiangVien?.HoTen ?? taiKhoan.TenDangNhap
                    ),

                    new Claim(
                        ClaimTypes.Role,
                        taiKhoan.Quyen
                    ),

                    new Claim(
                        "MaCBGV",
                        taiKhoan.MaCBGV ?? string.Empty
                    ),

                    new Claim(
                        "MaVaiTroHD",
                        maVaiTroHD
                    ),

                    new Claim(
                        "TenVaiTroHD",
                        tenVaiTroHD
                    ),

                    new Claim(
                        "MaTK",
                        taiKhoan.MaTK.ToString()
                    )
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false
                    }
                );

                if (taiKhoan.Quyen == "CBGV" || taiKhoan.Quyen == "ADMIN")
                {
                    return RedirectToAction("Index", "SangKien");
                } else
                {
                    return RedirectToAction("Index", "Home");
                }
            } catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(dangNhapVM);
            }
        }

        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(nameof(DangNhap), "TaiKhoan");
        }

        private Task NapFormChiTiet(TaiKhoanVM taiKhoanVM)
        {
            taiKhoanVM.DSQuyen = new List<SelectListItem>
            {
                new()
                {
                    Value = "ADMIN",
                    Text = "Quản trị viên"
                },
                new()
                {
                    Value = "BGD",
                    Text = "Ban giám đốc"
                },
                new()
                {
                    Value = "DVTT",
                    Text = "Đơn vị trực thuộc"
                },
                new()
                {
                    Value = "CBGV",
                    Text = "Cán bộ/giảng viên"
                }
            };

            taiKhoanVM.DSCanBoGiangVien = _appDbContext.DbSetCanBoGiangVien
                .Where(x => x.TaiKhoan == null || x.TaiKhoan!.MaTK == taiKhoanVM.MaTK)
                .OrderBy(x => x.MaCBGV)
                .Select(x => new SelectListItem
                {
                    Value = x.MaCBGV.ToString(),
                    Text = $"{x.MaCBGV} - {x.HoTen}"
                })
                .ToList();

            return Task.CompletedTask;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int? ma)
        {
            var taiKhoanVM = new TaiKhoanVM();

            await NapFormChiTiet(taiKhoanVM);

            if(ma != null)
            {
                var taiKhoan = await _appDbContext.DbSetTaiKhoan
                    .Include(x => x.CanBoGiangVien)
                    .FirstOrDefaultAsync((x => x.MaTK == ma));

                if (taiKhoan == null)
                {
                    TempData["Error"] = "Tài khoản không tồn tại";
                    return RedirectToAction(nameof(Index));
                }
                
                taiKhoanVM.MaTK = taiKhoan.MaTK;
                taiKhoanVM.MaCBGV = taiKhoan.MaCBGV;
                taiKhoanVM.HoTen = taiKhoan.CanBoGiangVien!.HoTen;
                taiKhoanVM.TenDangNhap = taiKhoan.TenDangNhap;
                taiKhoanVM.Quyen = taiKhoan.Quyen;
                taiKhoanVM.TrangThaiTK = taiKhoan.TrangThaiTK;
            }

            return View(taiKhoanVM);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> ChiTiet(TaiKhoanVM taiKhoanVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await NapFormChiTiet(taiKhoanVM);

                    return View(taiKhoanVM);
                }

                var canBoGiangVien = await _appDbContext.DbSetCanBoGiangVien
                    .FirstOrDefaultAsync(x => x.MaCBGV == taiKhoanVM.MaCBGV);

                bool isTaiKhoanExist = await _appDbContext.DbSetCanBoGiangVien
                    .AnyAsync(x =>
                        x.MaCBGV == taiKhoanVM.MaCBGV &&
                        x.TaiKhoan != null &&
                        x.TaiKhoan.MaTK != taiKhoanVM.MaTK
                    );

                if (canBoGiangVien == null)
                {
                    ModelState.AddModelError(
                        nameof(taiKhoanVM.MaCBGV),
                        "Cán bộ/giảng viên không tồn tại"
                    );

                    await NapFormChiTiet(taiKhoanVM);

                    return View(taiKhoanVM);
                }

                if (isTaiKhoanExist)
                {
                    ModelState.AddModelError(
                        nameof(taiKhoanVM.MaCBGV),
                        "Cán bộ/giảng viên đã có tài khoản"
                    );

                    await NapFormChiTiet(taiKhoanVM);

                    return View(taiKhoanVM);
                } 

                if (isTaiKhoanExist)
                {
                    ModelState.AddModelError(
                        nameof(taiKhoanVM.MaCBGV),
                        "Cán bộ/giảng viên đã có tài khoản"
                    );

                    await NapFormChiTiet(taiKhoanVM);

                    return View(taiKhoanVM);
                } 
                
                string tenDangNhap = canBoGiangVien.Email;
                string matKhau = canBoGiangVien.Email.Split('@')[0].PadRight(6, '@');;

                var taiKhoan = new TaiKhoan
                {
                    MaCBGV = taiKhoanVM.MaCBGV,
                    TenDangNhap = tenDangNhap,
                    Quyen = taiKhoanVM.Quyen!,
                    TrangThaiTK = 1
                };

                var hasher = new PasswordHasher<TaiKhoan>();

                var matKhauHashed = hasher.HashPassword(taiKhoan, matKhau);

                if (taiKhoanVM.MaTK <= 0)
                {
                    taiKhoan.MatKhau = matKhauHashed;

                    _appDbContext.DbSetTaiKhoan.Add(taiKhoan);

                    await _appDbContext.SaveChangesAsync();

                    TempData["Success"] = "Thêm tài khoản thành công";

                    return RedirectToAction(nameof(ChiTiet), new { ma = taiKhoan.MaTK });
                }
                else
                {
                    var taiKhoanDB = await _appDbContext.DbSetTaiKhoan
                        .FindAsync(taiKhoanVM.MaTK);

                    if (taiKhoanDB == null)
                    {
                        TempData["Error"] = "Tài khoản không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    taiKhoanDB.TenDangNhap = taiKhoanVM.TenDangNhap ?? taiKhoanDB.TenDangNhap;
                    taiKhoanDB.Quyen = taiKhoanVM.Quyen!;

                    TempData["Success"] = "Cập nhật tài khoản thành công";

                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(ChiTiet), new { ma = taiKhoanDB.MaTK });
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                await NapFormChiTiet(taiKhoanVM);

                return View(taiKhoanVM);
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> KhoaMo(int ma)
        {
            var taiKhoan = await _appDbContext.DbSetTaiKhoan.FindAsync(ma);

            if (taiKhoan == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại";
                return RedirectToAction(nameof(Index));
            }

            taiKhoan.TrangThaiTK = taiKhoan.TrangThaiTK == 1 ? 0 : 1;

            await _appDbContext.SaveChangesAsync();

            TempData["Success"] = $"{(taiKhoan.TrangThaiTK == 1 ? "Mở khóa" : "Khóa")} tài khoản thành công";

           return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DatLaiMatKhau()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DatLaiMatKhau(DatLaiMatKhauVM datLaiMatKhauVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(datLaiMatKhauVM);
                }

                var taiKhoan = await _appDbContext.DbSetTaiKhoan
                    .Include(x => x.CanBoGiangVien)
                    .FirstOrDefaultAsync(x =>
                        x.TenDangNhap.ToLower() == datLaiMatKhauVM.TenDangNhap!.Trim().ToLower() &&
                        x.CanBoGiangVien != null && x.CanBoGiangVien.Email.ToLower() == datLaiMatKhauVM.Email!.Trim().ToLower()
                    );

                if (taiKhoan == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Tên đăng nhập hoặc email không đúng"
                    );

                    return View(datLaiMatKhauVM);
                }

                var newPassword = Guid.NewGuid().ToString("N").Substring(0, 6);

                var hasher = new PasswordHasher<TaiKhoan>();

                await _emailService.SendEmailAsync(
                    datLaiMatKhauVM.Email!.Trim(),
                    "Đặt lại mật khẩu",
                    $"<p style=\"margin: 6px 0 0 0\">Xin chào {taiKhoan.CanBoGiangVien!.HoTen}</p><p style=\"margin: 6px 0 0 0\">Bạn đã yêu cầu đặt lại mật khẩu vào lúc {DateTime.Now.ToString("HH:mm - dd/MM/yyyy")}</p><p style=\"margin: 6px 0 0 0\">Tên đăng nhập: {taiKhoan.TenDangNhap}</p><p style=\"margin: 6px 0 0 0\">Mật khẩu mới của bạn là: {newPassword}</p>"
                );

                taiKhoan.MatKhau = hasher.HashPassword(taiKhoan, newPassword);

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Vui lòng kiểm tra email của bạn";

                return View();
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(datLaiMatKhauVM);
            }   
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN, CBGV, DVTT, BGD")]
        public IActionResult DoiMatKhau()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, CBGV, DVTT, BGD")]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauVM doiMatKhauVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(doiMatKhauVM);
                }

                var maTK = User.FindFirstValue("MaTK");

                var taiKhoan = await _appDbContext.DbSetTaiKhoan.FindAsync(int.Parse(maTK!));

                if (taiKhoan == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Tài khoản không tồn tại"
                    );

                    return RedirectToAction(nameof(DangXuat));
                }

                var hasher = new PasswordHasher<TaiKhoan>();

                var result = hasher.VerifyHashedPassword(
                    taiKhoan,
                    taiKhoan.MatKhau,
                    doiMatKhauVM.MatKhauCu!.Trim()
                );

                if (result == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(
                        nameof(doiMatKhauVM.MatKhauCu),
                        "Sai mật khẩu"
                    );

                    return View(doiMatKhauVM);
                }

                taiKhoan.MatKhau = hasher.HashPassword(taiKhoan, doiMatKhauVM.MatKhauMoi!.Trim());

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Đổi mật khẩu thành công";

                return View();
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(doiMatKhauVM);
            }
        }

        [HttpGet]
        [Authorize(Roles = "CBGV, DVTT, BGD, ADMIN")]
        public IActionResult CaNhan()
        {
            var maTK = User.FindFirstValue("MaTK");

            var caNhanVM = _appDbContext.DbSetTaiKhoan
                .Where(x => x.MaTK == int.Parse(maTK!))
                .Include(x => x.CanBoGiangVien)
                .Select(x => new CaNhanVM
                {
                    Ten = x.CanBoGiangVien!.HoTen,
                    TenDangNhap = x.TenDangNhap,
                    Email = x.CanBoGiangVien.Email,
                    SoDienThoai = x.CanBoGiangVien.SoDienThoai
                })
                .FirstOrDefault();

            return View(caNhanVM);
        }

        [HttpPost]
        [Authorize(Roles = "CBGV, DVTT, BGD, ADMIN")]
        public async Task<IActionResult> CaNhan(CaNhanVM caNhanVM)
        {
            try
            {
                var maTK = User.FindFirstValue("MaTK");
                var quyen = User.FindFirstValue(ClaimTypes.Role);

                if (!ModelState.IsValid)
                {
                    return View(caNhanVM);
                }

                var taiKhoan = await _appDbContext.DbSetTaiKhoan
                    .Where(x => x.MaTK == int.Parse(maTK!))
                    .Include(x => x.CanBoGiangVien)
                    .FirstOrDefaultAsync();

                if (taiKhoan == null)
                {                    
                    ModelState.AddModelError(
                        "",
                        "Tài khoản không tồn tại"
                    );

                    return RedirectToAction(nameof(DangXuat));
                }

                var isTenDangNhapExist = await _appDbContext.DbSetTaiKhoan
                    .AnyAsync(x =>
                        x.MaTK != int.Parse(maTK!) &&
                        x.TenDangNhap.ToLower() == caNhanVM.TenDangNhap!.Trim().ToLower()
                    );

                var isEmailExist = await _appDbContext.DbSetTaiKhoan
                    .Include(x => x.CanBoGiangVien)
                    .AnyAsync(x =>
                        x.MaTK != int.Parse(maTK!) &&
                        x.CanBoGiangVien != null && x.CanBoGiangVien.Email.ToLower() == caNhanVM.Email!.Trim().ToLower()
                    );

                var isSDTExist = await _appDbContext.DbSetTaiKhoan
                    .Include(x => x.CanBoGiangVien)
                    .AnyAsync(x =>
                        x.MaTK != int.Parse(maTK!) &&
                        x.CanBoGiangVien != null && x.CanBoGiangVien.SoDienThoai == caNhanVM.SoDienThoai!.Trim()
                    );

                if (isTenDangNhapExist)
                {
                    ModelState.AddModelError(
                        nameof(caNhanVM.TenDangNhap),
                        "Tên đăng nhập đã tồn tại"
                    );

                    return View(caNhanVM);
                }

                if (isEmailExist)
                {
                    ModelState.AddModelError(
                        nameof(caNhanVM.Email),
                        "Email đã tồn tại"
                    );

                    return View(caNhanVM);
                }

                if (isSDTExist)
                {
                    ModelState.AddModelError(
                        nameof(caNhanVM.SoDienThoai),
                        "Số điện thoại đã tồn tại"
                    );

                    return View(caNhanVM);
                }

                taiKhoan.TenDangNhap = caNhanVM.TenDangNhap!;
                taiKhoan.CanBoGiangVien!.Email = caNhanVM.Email!;
                taiKhoan.CanBoGiangVien.SoDienThoai = caNhanVM.SoDienThoai!;

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thông tin cá nhân thành công";

                return RedirectToAction(nameof(CaNhan));
            } catch
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra. Vui lòng thử lại"
                );

                return View(caNhanVM);
            }
        }
    }
}
