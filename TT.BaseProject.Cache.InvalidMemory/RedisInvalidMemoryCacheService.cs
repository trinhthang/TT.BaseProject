using StackExchange.Redis;
using TT.BaseProject.Cache.Models;

namespace TT.BaseProject.Cache.InvalidMemory
{
    public class RedisInvalidMemoryCacheService : IInvalidMemoryCacheService
    {
        private readonly IMemCached _memCached;
        private readonly InvalidMemoryCacheRedisConfig _invalidMemoryCacheRedis;
        private readonly ISubscriber _subscriber;

        public RedisInvalidMemoryCacheService(IMemCached memCached, InvalidMemoryCacheRedisConfig invalidMemoryCacheRedis)
        {
            _memCached = memCached;
            _invalidMemoryCacheRedis = invalidMemoryCacheRedis;
            _subscriber = this.GetSubscriber();
        }

        /// <summary>
        /// Gửi thông báo hủy cache mem cho các host khác
        /// </summary>
        public async Task InvalidAsync(string key)
        {
            await _subscriber.PublishAsync(_invalidMemoryCacheRedis.Channel, key);
        }

        /// <summary>
        /// Thực hiện lắng nghe thông báo hủy mem cache từ host khác
        /// </summary>
        public void StartSubcribe()
        {
            _subscriber.Subscribe(_invalidMemoryCacheRedis.Channel, (channel, message) =>
            {
                //Xóa cache mem
                _memCached.Remove(message);
            });
        }

        public ISubscriber GetSubscriber()
        {
            // connect to the server
            var connection = ConnectionMultiplexer.Connect(_invalidMemoryCacheRedis.Connection);

            // grab an instance of subscriber
            var subscriber = connection.GetSubscriber();
            return subscriber;
        }
    }
}