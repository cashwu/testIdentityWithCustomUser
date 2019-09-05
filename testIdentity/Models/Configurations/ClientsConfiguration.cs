using System;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace testIdentity.Models.Configurations
{
    public class ClientsConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.Property(a => a.Enabled).HasConversion<int>();
            builder.Property(a => a.RequireClientSecret).HasConversion<int>();
            builder.Property(a => a.RequireConsent).HasConversion<int>();
            builder.Property(a => a.AllowRememberConsent).HasConversion<int>();
            builder.Property(a => a.AlwaysIncludeUserClaimsInIdToken).HasConversion<int>();
            builder.Property(a => a.RequirePkce).HasConversion<int>();
            builder.Property(a => a.AllowPlainTextPkce).HasConversion<int>();
            builder.Property(a => a.AllowAccessTokensViaBrowser).HasConversion<int>();
            builder.Property(a => a.FrontChannelLogoutSessionRequired).HasConversion<int>();
            builder.Property(a => a.BackChannelLogoutSessionRequired).HasConversion<int>();
            builder.Property(a => a.AllowOfflineAccess).HasConversion<int>();
            builder.Property(a => a.UpdateAccessTokenClaimsOnRefresh).HasConversion<int>();
            builder.Property(a => a.EnableLocalLogin).HasConversion<int>();
            builder.Property(a => a.IncludeJwtId).HasConversion<int>();
            builder.Property(a => a.AlwaysSendClientClaims).HasConversion<int>();
            builder.Property(a => a.NonEditable).HasConversion<int>();
        }
    }
}