using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using testIdentity.Models;

namespace testIdentity
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySQL(Configuration.GetConnectionString("AppDbContext"));
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            });

            var identityServerBuilder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.IssuerUri = "oauth";
                    options.PublicOrigin = "http://localhost:32354/";
                    options.Endpoints.EnableIntrospectionEndpoint = false;
                })
                .AddConfigurationStore<AppDbContext>()
                .AddOperationalStore<AppDbContext>(options => { options.EnableTokenCleanup = true; })
                .AddProfileService<CustomProfileService>()
                .AddResourceOwnerValidator<CustomResourceOwnerValidator>();

            IdentityModelEventSource.ShowPII = true;

            var key = JsonConvert.DeserializeObject<TemporaryRsaKey>(
                File.ReadAllText(Path.Combine(Environment.ContentRootPath, "oauth.rsa")),
                new JsonSerializerSettings {ContractResolver = new RsaKeyContractResolver()});

            var rsaSecurityKey = new RsaSecurityKey(key.Parameters);
            
            identityServerBuilder.AddSigningCredential(rsaSecurityKey);
            
//            identityServerBuilder.AddDeveloperSigningCredential();

            services.AddAuthentication();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
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
}