using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNet.Identity;

namespace testMvc.Extension
{
    public static class Extension
    {
        public static string UserClaims(this IPrincipal user, string claim)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            return claimsIdentity.FindFirstValue(claim);
        }
    }
}