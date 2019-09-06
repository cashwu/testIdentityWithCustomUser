using System;

namespace testClient.Infra
{
    public interface IRedisClient
    {
        bool Set<T>(string key, T t)
            where T : class;

        bool Set<T>(string key, T t, TimeSpan expiry)
            where T : class;

        T Get<T>(string key)
            where T : class;
    }
}