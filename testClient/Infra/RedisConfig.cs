using Cashwu.AspNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace testClient.Infra
{
    [ConfigurationSection("Redis", ServiceLifetime.Singleton)]
    public class RedisConfig
    {
        public bool IsEnable { get; set; }

        public string Password { get; set; }

        public int SyncTimeout { get; set; }

        public int ConnectTimeout { get; set; }

        public string Prefix { get; set; }

        public string Ip { get; set; }

        public int Port { get; set; }

        public string ConnectionString
        {
            get => string.IsNullOrEmpty(Password) ? $"{Ip}:{Port}" : $"{Ip}:{Port},password={Password}";
        }
    }
}