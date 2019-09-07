using System.Linq;
using System.Security.Principal;
using IdentityServer4.Extensions;

namespace testMvc.Extension
{
    public static class Extension
    {
        public static string UserClaims(this IPrincipal user, string claim)
        {
            return user.GetAuthenticationMethods().FirstOrDefault(a => a.Type == claim)?.Value;
        }
    }
}