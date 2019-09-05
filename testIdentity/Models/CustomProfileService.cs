using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace testIdentity.Models
{
    public class CustomProfileService : IProfileService
    {
        private readonly AppDbContext _appDbContext;

        public CustomProfileService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subId = context.Subject.GetSubjectId();

            var user = FindById(subId);

            var claims = new List<Claim>
            {
                new Claim("SystemCode", user.SystemCode),
                new Claim("Name", user.UserName),
                new Claim("RefId", user.RefId.ToString())
            };

            context.IssuedClaims = claims;

            await Task.CompletedTask;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            
            var subId = context.Subject.GetSubjectId();
            var user = FindById(subId);

            context.IsActive = user != null;
            
            await Task.CompletedTask;
        }

        private User FindById(string subId)
        {
            return _appDbContext.User.FirstOrDefault(a => a.Id == subId);
        }
    }
}