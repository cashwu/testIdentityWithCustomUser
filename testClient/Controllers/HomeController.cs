using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace testClient.Controllers
{
    public class HomeController : Controller
    {
        private static readonly HttpClient Client = new HttpClient();
        private ClaimsPrincipal _httpContextUser;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextUser = httpContextAccessor.HttpContext.User;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Auth()
        {
            Client.DefaultRequestHeaders.Add("SystemCode", "AB001");
            var result = await Client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = "http://localhost:32354/connect/token",
                ClientId = "api",
                ClientSecret = "6CD30DB681204AFA85CEFF8F157FE28E",
                UserName = "cash",
                Password = "o0yGCpkJ3S7YsispuTd7jG1I++w=",
                Scope = "Api offline_access"
            }, CancellationToken.None);

            return Ok(result.Json);
        }

        [Authorize]
        public IActionResult Data()
        {
            return Ok(new
            {
                User.Identity.Name,
                SystemCode = UserClaims("SystemCode"),
                RefId = UserClaims("RefId")
            });
        }

        private string UserClaims()
        {
            return User.Claims.FirstOrDefault(a => a.Type == "SystemCode")?.Value;
        }

        private string UserClaims(string claim)
        {
            return User.Claims.FirstOrDefault(a => a.Type == claim)?.Value;
        }
    }
}