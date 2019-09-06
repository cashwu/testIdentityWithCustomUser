using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using testClient.ActionFilter;
using testClient.Extension;
using testClient.Infra;
using testClient.Models;
using testClient.Models.Req;

namespace testClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRedisClient _redisClient;
        private static readonly HttpClient Client = new HttpClient();

        public HomeController(IRedisClient redisClient)
        {
            _redisClient = redisClient;
        }

        [HttpPost]
        public async Task<IActionResult> Auth([FromBody] AuthReq req)
        {
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("SystemCode", req.SystemCode);
            var result = await Client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = "http://localhost:32354/connect/token",
                ClientId = "api",
                ClientSecret = "6CD30DB681204AFA85CEFF8F157FE28E",
                UserName = req.Name,
                Password = req.Password,
                Scope = "Api offline_access"
            }, CancellationToken.None);

            SaveTokenToRedis($"{req.SystemCode}-{req.Name}", result);

            return Ok(result.Json);
        }

        [HttpPost]
        public async Task<IActionResult> Refresh([FromBody] RefreshReq req)
        {
            var result = await Client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = "http://localhost:32354/connect/token",
                ClientId = "api",
                ClientSecret = "6CD30DB681204AFA85CEFF8F157FE28E",
                RefreshToken = req.Token
            }, CancellationToken.None);

            if (result.IsError == false && !string.IsNullOrEmpty(result.AccessToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(result.AccessToken) as JwtSecurityToken;

                var systemCode = token.Claims.FirstOrDefault(a => a.Type == "SystemCode").Value.ToString();
                var UserName = token.Claims.FirstOrDefault(a => a.Type == "Name").Value.ToString();

                SaveTokenToRedis($"{systemCode}-{UserName}", result);
            }

            return Ok(result.Json);
        }

//        [HttpGet]
//        public IActionResult GetToken()
//        {
//            var jwtToken = _redisClient.Get<JwtToken>($"{SystemCode}-{UserName}");
//
//            return Ok(jwtToken);
//        }

        [CustomAuthorizeActionFilter]
        public IActionResult Data()
        {
            return Ok(new
            {
                User.Identity.Name,
                SystemCode = User.UserClaims("SystemCode"),
                RefId = User.UserClaims("RefId")
            });
        }

        private void SaveTokenToRedis(string key, TokenResponse result)
        {
            if (result.IsError == false && !string.IsNullOrEmpty(result.AccessToken))
            {
                _redisClient.Set(key, new JwtToken(result.AccessToken, result.RefreshToken));
            }
        }
    }
}