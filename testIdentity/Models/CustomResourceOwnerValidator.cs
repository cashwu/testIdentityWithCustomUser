using System;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Validation;

namespace testIdentity.Models
{
    public class CustomResourceOwnerValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUserRepo _userRepo;

        public CustomResourceOwnerValidator(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }
        
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var validateCredentials = _userRepo.ValidateCredentials(context.UserName, context.Password);

            if (validateCredentials)
            {
                var user = _userRepo.FindByUsername(context.UserName);
                context.Result = new GrantValidationResult(user.Id, OidcConstants.AuthenticationMethods.Password);
            }

            return Task.FromResult(0);
        }
    }
}