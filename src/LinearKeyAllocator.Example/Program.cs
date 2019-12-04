using System;
using System.Threading.Tasks;
using System.Transactions;

namespace LinearKeyAllocator.Example
{
    class Program
    {
        private static string cnn =
            "Server=(local); Database=TimeHarmony.Admin; Trusted_Connection=True; MultipleActiveResultSets=true";

        static async Task Main(string[] args)
        {
            var generator = await new KeyAllocator(cnn).Initialize();

            const string type1 = "Type1";
            const string type2 = "Type2";

            using var scope =
                new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

            for (var i = 0; i < 5000; i++)
            {
                var key = (i % 2 == 0) ? type1 : type2;

                var value = await generator.GenerateInt32(key);
                Console.WriteLine("{0}: {1}", key, value);
            }

            scope.Complete();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
