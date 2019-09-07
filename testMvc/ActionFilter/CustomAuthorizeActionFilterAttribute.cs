using System.Web.Mvc;
using testMvc.Controllers;
using testMvc.Extension;
using testMvc.Models;

namespace testMvc.ActionFilter
{
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
            var token = bearerString.Split(' ')[1];

            var systemCode = context.HttpContext.User.UserClaims("SystemCode");
            
            var userName = context.HttpContext.User.UserClaims("Name");

            var jwtToken = Store.TokenStore[$"{systemCode}-{userName}"];

            if (jwtToken == null || jwtToken.AccessToken != token)
            {
                context.Result = new HttpUnauthorizedResult();
            }
        }
    }
}