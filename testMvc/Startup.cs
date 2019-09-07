using System.Data;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Web.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;

[assembly: OwinStartup(typeof(testMvc.Startup))]

namespace testMvc
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] {"Api"},

                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = new[] {"Api"},
                    ValidIssuer = "oauth",
                    IssuerSigningKey = RsaSecurityKey(),
                }
            });
        }

        private static RsaSecurityKey RsaSecurityKey()
        {
            var key = JsonConvert.DeserializeObject<TemporaryRsaKey>(
                File.ReadAllText(HostingEnvironment.MapPath(@"~/App_Data/oauth.rsa") ?? throw new NoNullAllowedException()),
                new JsonSerializerSettings {ContractResolver = new RsaKeyContractResolver()});

            var rsaSecurityKey = new RsaSecurityKey(key.Parameters);
            return rsaSecurityKey;
        }
    }

    internal class TemporaryRsaKey
    {
        public RSAParameters Parameters { get; set; }
    }

    internal class RsaKeyContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            property.Ignored = false;

            return property;
        }
    }
}