using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace testIdentity.Models.Configurations
{
    public class ApiScopeConfiguration : IEntityTypeConfiguration<ApiScope>
    {
        public void Configure(EntityTypeBuilder<ApiScope> builder)
        {
            builder.Property(a => a.Required).HasConversion<int>();
            builder.Property(a => a.Emphasize).HasConversion<int>();
            builder.Property(a => a.ShowInDiscoveryDocument).HasConversion<int>();
        }
    }
}