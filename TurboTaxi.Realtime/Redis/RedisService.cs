using StackExchange.Redis;
using System.Linq;
using System.Collections.Generic;

namespace TurboTaxi.Realtime.Redis
{
    public interface IRedisService
    {
        // Hash operations
        Task HashSetAsync(string key, HashEntry[] entries);
        Task<HashEntry[]> HashGetAllAsync(string key);
        Task<string?> HashGetAsync(string key, string field);
        Task HashDeleteAsync(string key, string field);
        Task<bool> HashExistsAsync(string key, string field);

        // String operations
        Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
        Task<bool> SetStringIfNotExistsAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetStringAsync(string key);
        Task<bool> DeleteKeyAsync(string key);
        Task<bool> KeyExistsAsync(string key);

        // Set operations
        Task<bool> SetAddAsync(string key, string value);
        Task<bool> SetRemoveAsync(string key, string value);
        Task<string[]> SetMembersAsync(string key);
        Task<long> SetLengthAsync(string key);
        Task<bool> SetContainsAsync(string key, string value);

        // Expiry operations
        Task<bool> SetExpiryAsync(string key, TimeSpan ttl);
        Task<TimeSpan?> GetTTLAsync(string key);

        // Geo operations
        Task GeoAddAsync(string key, double longitude, double latitude, string member);
        Task<GeoRadiusResult[]> GeoRadiusAsync(string key, double longitude, double latitude, double radiusKm);
        Task<GeoPosition?> GeoPositionAsync(string key, string member);
        Task<bool> GeoRemoveAsync(string key, string member);

        // Lock operations
        Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiry);
        Task<bool> ReleaseLockAsync(string key, string value);

        // Debug helpers
        Task<IEnumerable<string>> ListKeysAsync(string pattern);
    }

    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _mux;
        public RedisService(IConnectionMultiplexer mux) => _mux = mux;
        private IDatabase Db => _mux.GetDatabase();

        // Hash operations
        public Task HashSetAsync(string key, HashEntry[] entries) => Db.HashSetAsync(key, entries);

        public Task<HashEntry[]> HashGetAllAsync(string key) => Db.HashGetAllAsync(key);

        public async Task<string?> HashGetAsync(string key, string field)
        {
            var val = await Db.HashGetAsync(key, field);
            return val.HasValue ? val.ToString() : null;
        }

        public Task HashDeleteAsync(string key, string field) => Db.HashDeleteAsync(key, field);

        public Task<bool> HashExistsAsync(string key, string field) => Db.HashExistsAsync(key, field);

        // String operations
        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (expiry.HasValue)
                return await Db.StringSetAsync(key, value, expiry.Value);
            return await Db.StringSetAsync(key, value);
        }

        public async Task<bool> SetStringIfNotExistsAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (expiry.HasValue)
                return await Db.StringSetAsync(key, value, expiry.Value, When.NotExists);
            return await Db.StringSetAsync(key, value, when: When.NotExists);
        }

        public async Task<string?> GetStringAsync(string key)
        {
            var val = await Db.StringGetAsync(key);
            return val.HasValue ? val.ToString() : null;
        }

        public Task<bool> DeleteKeyAsync(string key) => Db.KeyDeleteAsync(key);

        public Task<bool> KeyExistsAsync(string key) => Db.KeyExistsAsync(key);

        // Set operations
        public Task<bool> SetAddAsync(string key, string value) 
            => Db.SetAddAsync(key, value);

        public Task<bool> SetRemoveAsync(string key, string value) 
            => Db.SetRemoveAsync(key, value);

        public async Task<string[]> SetMembersAsync(string key)
        {
            var members = await Db.SetMembersAsync(key);
            return members.Select(m => m.ToString()).ToArray();
        }

        public Task<long> SetLengthAsync(string key) 
            => Db.SetLengthAsync(key);

        public Task<bool> SetContainsAsync(string key, string value) 
            => Db.SetContainsAsync(key, value);

        // Expiry operations
        public Task<bool> SetExpiryAsync(string key, TimeSpan ttl) 
            => Db.KeyExpireAsync(key, ttl);

        public Task<TimeSpan?> GetTTLAsync(string key) 
            => Db.KeyTimeToLiveAsync(key);

        // Geo operations
        public Task GeoAddAsync(string key, double longitude, double latitude, string member) 
            => Db.GeoAddAsync(key, longitude, latitude, member);

        public Task<GeoRadiusResult[]> GeoRadiusAsync(string key, double longitude, double latitude, double radiusKm) 
            => Db.GeoRadiusAsync(key, longitude, latitude, radiusKm, GeoUnit.Kilometers);

        public Task<GeoPosition?> GeoPositionAsync(string key, string member) 
            => Db.GeoPositionAsync(key, member);

        public Task<bool> GeoRemoveAsync(string key, string member) 
            => Db.GeoRemoveAsync(key, member);

        // Lock operations (Distributed Lock Pattern)
        public Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiry)
        {
            return Db.StringSetAsync(key, value, expiry, When.NotExists);
        }

        public async Task<bool> ReleaseLockAsync(string key, string value)
        {
            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            var result = await Db.ScriptEvaluateAsync(script, new RedisKey[] { key }, new RedisValue[] { value });
            return (int)result == 1;
        }

        // Debug helpers
        public Task<IEnumerable<string>> ListKeysAsync(string pattern)
        {
            var endpoint = _mux.GetEndPoints().First();
            var server = _mux.GetServer(endpoint);
            var keys = server.Keys(pattern: pattern).Select(k => k.ToString());
            return Task.FromResult(keys);
        }
    }
}
