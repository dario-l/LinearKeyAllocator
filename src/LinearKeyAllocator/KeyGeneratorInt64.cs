using System.Threading;
using System.Threading.Tasks;

namespace LinearKeyAllocator
{
    internal class KeyGeneratorInt64 : BaseKeyGenerator
    {
        private static readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        public KeyGeneratorInt64(string key, ushort seedSize, string connectionString)
            : base(key, seedSize, connectionString) { }

        public long Max { get; private set; }
        public long Current { get; private set; }

        public Task<long> GetNext(CancellationToken token = default) => GetNext(null, token);

        public async Task<long> GetNext(ushort? seedSize, CancellationToken token = default)
        {
            await locker.WaitAsync(token);

            try
            {
                if (Current >= Max)
                {
                    Max = (long)(await GetNextMaxFor(Key, seedSize ?? SeedSize, token));
                    Current = (Max - SeedSize) + 1;
                }

                return Current++;
            }
            finally
            {
                locker.Release();
            }
        }
    }
}