using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using testClient.Extension;
using testClient.Infra;
using testClient.Models;

namespace testClient.ActionFilter
{
    public class CustomAuthorizeActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
            }

            var bearerString = context.HttpContext.Request.Headers.FirstOrDefault(a => a.Key == "Authorization").Value.ToString();
            var token = bearerString.Split(' ')[1];

            var systemCode = context.HttpContext.User.UserClaims("SystemCode");
            var userName = context.HttpContext.User.Identity.Name;

            var redisClient = context.HttpContext.RequestServices.GetRequiredService<IRedisClient>();
            var jwtToken = redisClient.Get<JwtToken>($"{systemCode}-{userName}");

            if (jwtToken == null || jwtToken.AccessToken != token)
            {
                context.Result = new UnauthorizedResult(); 
            }
        }
    }
}