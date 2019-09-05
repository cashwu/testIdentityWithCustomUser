using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;

namespace testIdentity.Models
{
    public class CustomResourceOwnerValidator : IResourceOwnerPasswordValidator
    {
        private readonly AppDbContext _appDbContext;
        private readonly HttpRequest _httpContextRequest;

        public CustomResourceOwnerValidator(IHttpContextAccessor httpContextAccessor,
            AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _httpContextRequest = httpContextAccessor.HttpContext.Request;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var systemCode = _httpContextRequest.Headers.FirstOrDefault(a => a.Key == "SystemCode");
            if (string.IsNullOrEmpty(systemCode.Value))
            {
                return Task.FromResult(0);
            }

            var user = ValidateCredentials(context.UserName, context.Password, systemCode.Value);

            if (user != null)
            {
                context.Result = new GrantValidationResult(user.Id, OidcConstants.AuthenticationMethods.Password);
            }

            return Task.FromResult(0);
        }

        private User ValidateCredentials(string username, string password, string systemCode)
        {
            var user = _appDbContext.User.FirstOrDefault(a => a.UserName == username && a.SystemCode == systemCode);

            if (user == null)
            {
                return null;
            }

            if (user.Password == password)
            {
                return user;
            }
            
            return null;
        }
    }
    
    static class Cryptography
    {
        /// <summary>
        /// 用SHA1的方式加密 - 转换为Base-64位數編碼
        /// </summary>
        /// <param name="text">要被加密的字串</param>
        /// <returns>加密过後的字串</returns>
        public static string EncryptBySHA1(string text)
        {
            // string to byte[]
            var bytes = Encoding.UTF8.GetBytes(text);

            // encrypt
            var sha1 = new SHA1CryptoServiceProvider();
            var result = sha1.ComputeHash(bytes);

            // byte[] to string
            var resultString = Convert.ToBase64String(result);

            return resultString;
        }
    }
}