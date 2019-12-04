using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LinearKeyAllocator
{
    public class KeyAllocator
    {
        private readonly ConcurrentDictionary<string, KeyGeneratorInt32> _cacheInt32 = new ConcurrentDictionary<string, KeyGeneratorInt32>();
        private readonly ConcurrentDictionary<string, KeyGeneratorInt64> _cacheInt64 = new ConcurrentDictionary<string, KeyGeneratorInt64>();

        private readonly string _connectionString;

        public KeyAllocator(string connectionString, ushort defaultSeedSize = 100)
        {
            _connectionString = connectionString;
            DefaultSeedSize = defaultSeedSize;
        }

        public ushort DefaultSeedSize { get; }

        public Task<int> GenerateInt32(string key, ushort? seedSize = null) =>
            _cacheInt32.GetOrAdd(
                key,
                k => new KeyGeneratorInt32(k, DefaultSeedSize, _connectionString))
                .GetNext(seedSize);

        public Task<long> GenerateInt64(string key, ushort? seedSize = null) =>
            _cacheInt64.GetOrAdd(
                key,
                k => new KeyGeneratorInt64(k, DefaultSeedSize, _connectionString))
                .GetNext(seedSize);

        public async Task<KeyAllocator> Initialize()
        {
            await BaseKeyGenerator.Initialize(_connectionString);
            return this;
        }
    }
}