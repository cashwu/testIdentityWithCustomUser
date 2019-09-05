using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace testIdentity.Models
{
    public class CustomProfileService : IProfileService
    {
        private readonly IUserRepo _userRepo;

        public CustomProfileService(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subId = context.Subject.GetSubjectId();

            var user = _userRepo.FindById(subId);

            var claims = new List<Claim>
            {
                new Claim("SystemCode", user.SystemCode),
                new Claim("Name", user.UserName)
            };

            context.IssuedClaims = claims;

            await Task.CompletedTask;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            
            var subId = context.Subject.GetSubjectId();
            var user = _userRepo.FindById(subId);

            context.IsActive = user != null;
            
            await Task.CompletedTask;
        }
    }
}