using System.Linq;
using System.Security.Claims;

namespace testClient.Extension
{
    public static class Extension
    {
        public static string UserClaims(this ClaimsPrincipal user, string claim)
        {
            return user.Claims.FirstOrDefault(a => a.Type == claim)?.Value;
        }
    }
}