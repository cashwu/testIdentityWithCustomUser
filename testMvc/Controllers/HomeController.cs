using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using IdentityModel.Client;
using testMvc.Extension;

namespace testMvc.Controllers
{
    public class HomeController : Controller
    {
        private static readonly HttpClient Client = new HttpClient();

        [HttpPost]
        public async Task<ActionResult> Auth(AuthReq req)
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

            return Json(new JwtToken(result.AccessToken, result.RefreshToken));
        }

        [HttpPost]
        public async Task<ActionResult> Refresh(RefreshReq req)
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

            return Json(new JwtToken(result.AccessToken, result.RefreshToken));
        }

//        [CustomAuthorizeActionFilter]
        [Authorize]
        public ActionResult Data()
        {
            return Json(new
            {
                User.Identity.Name,
                SystemCode = User.UserClaims("SystemCode"),
                RefId = User.UserClaims("RefId")
            }, JsonRequestBehavior.AllowGet);
        }

        private void SaveTokenToRedis(string key, TokenResponse result)
        {
            if (result.IsError == false && !string.IsNullOrEmpty(result.AccessToken))
            {
                if (Store.TokenStore.ContainsKey(key))
                {
                    Store.TokenStore[key] = new JwtToken(result.AccessToken, result.RefreshToken);
                }
                else
                {
                    Store.TokenStore.Add(key, new JwtToken(result.AccessToken, result.RefreshToken));
                }
            }
        }
    }

    public class CustomAuthorizeActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new HttpUnauthorizedResult();
                return;
            }

            var bearerString = context.HttpContext.Request.Headers.Get("Authorization");
//            var bearerString = context.HttpContext.Request.Headers.FirstOrDefault(a => a.Key == "Authorization").Value.ToString();
            var token = bearerString.Split(' ')[1];

            var systemCode = context.HttpContext.User.UserClaims("SystemCode");
            var userName = context.HttpContext.User.Identity.Name;

            var jwtToken = Store.TokenStore[$"{systemCode}-{userName}"];

            if (jwtToken == null || jwtToken.AccessToken != token)
            {
                context.Result = new HttpUnauthorizedResult();
            }
        }
    }

    public class Store
    {
        public static Dictionary<string, JwtToken> TokenStore = new Dictionary<string, JwtToken>();
    }

    public class JwtToken
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public JwtToken()
        {
        }

        public JwtToken(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }

    public class RefreshReq
    {
        public string Token { get; set; }
    }

    public class AuthReq
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public string SystemCode { get; set; }
    }
}