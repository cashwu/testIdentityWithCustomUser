using System.Threading;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;

[assembly: OwinStartup(typeof(testMvc.Startup))]

namespace testMvc
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            IdentityModelEventSource.ShowPII = true;

            var openIdConfig = OpenIdConfig();
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] {"Api"},

                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = new[] {"Api"},
                    ValidIssuer = "oauth",
                    IssuerSigningKeys = openIdConfig.SigningKeys,
                },
            });
        }

        private static OpenIdConnectConfiguration OpenIdConfig()
        {
            var url = $"http://localhost:32354/.well-known/openid-configuration";
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(url,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever
                {
                    RequireHttps = false
                });
            var openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter()
                .GetResult();

            return openIdConfig;
        }
    }
}