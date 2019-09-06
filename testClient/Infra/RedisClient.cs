using System;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace testClient.Infra
{
    public class RedisClient : IRedisClient
    {
        private static ConfigurationOptions _redisConfigOptions;
        private readonly RedisConfig _config;

        private readonly Lazy<ConnectionMultiplexer> _lazyConnection =
            new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_redisConfigOptions));

        public RedisClient(RedisConfig config)
        {
            _config = config;
            ConfigurationOptions();
        }

        private IDatabase DB
        {
            get
            {
                if (!_config.IsEnable)
                {
                    return null;
                }

                return Connection?.GetDatabase(0).WithKeyPrefix(_config.Prefix);
            }
        }

        private ConnectionMultiplexer Connection => _lazyConnection.Value;

        public bool Set<T>(string key, T t)
            where T : class
        {
            var val = JsonConvert.SerializeObject(t);
            return DB.StringSet((RedisKey)key, (RedisValue)val);
        }

        public bool Set<T>(string key, T t, TimeSpan expiry)
            where T : class
        {
            var val = JsonConvert.SerializeObject(t);
            return DB.StringSet((RedisKey)key, (RedisValue)val, expiry);
        }

        public T Get<T>(string key)
            where T : class
        {
            var val = DB.StringGet((RedisKey)key).ToString();
            return JsonConvert.DeserializeObject<T>(val);
        }
        
        private void ConfigurationOptions()
        {
            if (!_config.IsEnable)
            {
                return;
            }

            _redisConfigOptions = new ConfigurationOptions
            {
                CommandMap = CommandMap.Default,
                EndPoints = { { _config.Ip, _config.Port } },
                Password = _config.Password,
                AllowAdmin = true,
                SyncTimeout = _config.SyncTimeout,
                ConnectTimeout = _config.ConnectTimeout,
                DefaultVersion = new Version(4, 0),
                AbortOnConnectFail = false
            };
        }
    }
}