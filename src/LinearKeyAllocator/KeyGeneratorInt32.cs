using System;
using System.Threading;
using System.Threading.Tasks;

namespace LinearKeyAllocator
{
    internal class KeyGeneratorInt32 : BaseKeyGenerator
    {
        private static readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        public KeyGeneratorInt32(string key, ushort seedSize, string connectionString)
            : base(key, seedSize, connectionString) { }

        public int Max { get; private set; }
        public int Current { get; private set; }

        public Task<int> GetNext(CancellationToken token = default) => GetNext(null, token);

        public async Task<int> GetNext(ushort? seedSize, CancellationToken token = default)
        {
            await locker.WaitAsync(token);

            try
            {
                if (Current >= Max)
                {
                    Max = Convert.ToInt32(await GetNextMaxFor(Key, seedSize ?? SeedSize, token));
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