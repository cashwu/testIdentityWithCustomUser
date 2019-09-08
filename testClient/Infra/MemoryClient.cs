using System;
using System.Collections.Generic;

namespace testClient.Infra
{
    public class MemoryClient : IRedisClient
    {
        private static Dictionary<string, object> dic = new Dictionary<string, object>();
        
        public bool Set<T>(string key, T t) where T : class
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = t;
            }
            else
            {
                dic.Add(key, t); 
            }

            return true;
        }

        public bool Set<T>(string key, T t, TimeSpan expiry) where T : class
        {
            return Set(key, t);
        }

        public T Get<T>(string key) where T : class
        {
            if (dic.ContainsKey(key))
            {
                return dic[key] as T;
            }

            return default;
        }
    }
}