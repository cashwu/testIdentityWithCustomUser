using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using testClient.Infra;
using testClient.Models;

namespace testClient.Controllers
{
    public class HomeController : Controller
    {
        private const string UserName = "cash";
        private const string SystemCode = "AB001";
        private readonly IRedisClient _redisClient;
        private static readonly HttpClient Client = new HttpClient();

        public HomeController(IRedisClient redisClient)
        {
            _redisClient = redisClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Auth()
        {
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("SystemCode", SystemCode);
            var result = await Client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = "http://localhost:32354/connect/token",
                ClientId = "api",
                ClientSecret = "6CD30DB681204AFA85CEFF8F157FE28E",
                UserName = "cash",
                Password = "o0yGCpkJ3S7YsispuTd7jG1I++w=",
                Scope = "Api offline_access"
            }, CancellationToken.None);

            if (result.IsError == false && !string.IsNullOrEmpty(result.AccessToken))
            {
                _redisClient.Set($"{SystemCode}-{UserName}", new JwtToken(result.AccessToken, result.RefreshToken));
            }

            return Ok(result.Json);
        }

        public IActionResult GetToken()
        {
            var jwtToken = _redisClient.Get<JwtToken>($"{SystemCode}-{UserName}");

            return Ok(jwtToken);
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

        private string UserClaims(string claim)
        {
            return User.Claims.FirstOrDefault(a => a.Type == claim)?.Value;
        }
    }
}