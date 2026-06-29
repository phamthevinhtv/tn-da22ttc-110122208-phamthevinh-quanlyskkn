using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Data;

namespace ql_sang_kien_kinh_nghiem.Middlewares
{
    public class KiemTraTaiKhoanMiddleware
    {
        private readonly RequestDelegate _next;

        public KiemTraTaiKhoanMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext appDbContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {

                string? maTKClaim = context.User.FindFirst("MaTK")?.Value;

                if (int.TryParse(maTKClaim, out int maTK))
                {
                    var taiKhoan = await appDbContext.DbSetTaiKhoan
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.MaTK == maTK);

                    if (taiKhoan == null || taiKhoan.TrangThaiTK == 0)
                    {
                        await context.SignOutAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme
                        );

                        context.Response.Redirect("/TaiKhoan/DangNhap");

                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
