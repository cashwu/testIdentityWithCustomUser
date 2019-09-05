using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace testIdentity.Models.Configurations
{
    public class IdentityResourcesConfiguration : IEntityTypeConfiguration<IdentityResource>
    {
        public void Configure(EntityTypeBuilder<IdentityResource> builder)
        {
            builder.Property(a => a.Enabled).HasConversion<int>();
            builder.Property(a => a.Required).HasConversion<int>();
            builder.Property(a => a.Emphasize).HasConversion<int>();
            builder.Property(a => a.ShowInDiscoveryDocument).HasConversion<int>();
            builder.Property(a => a.NonEditable).HasConversion<int>();
        }
    }
}