using Microsoft.AspNetCore.Mvc;

namespace ql_sang_kien_kinh_nghiem.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Error/{type}")]
        public IActionResult Index(string type)
        {
            if (type == "server") 
            {
                ViewData["ErrorMessage"] = "Lỗi máy chủ. Vui lòng thử lại";
            }
            else
            {
                ViewData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại";
            }

            return View();
        }
    }
}
