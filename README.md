# LinearKeyAllocator

**Example**
``` c#
class Program
{
    private static string cnn =
        "Server=(local); Database=Test; Trusted_Connection=True; MultipleActiveResultSets=true";

    static async Task Main(string[] args)
    {
        var generator = await new KeyAllocator(cnn).Initialize();

        const string type1 = "Type1";
        const string type2 = "Type2";

        using var scope =
            new TransactionScope(
                TransactionScopeOption.Required,
                TransactionScopeAsyncFlowOption.Enabled);

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
```

**Sql Script**
``` SQL
CREATE TABLE [dbo].[LinearKeyAllocator](
    [Key] [varchar](255) NOT NULL,
    [NextMax] [bigint] NOT NULL,
    CONSTRAINT [PK_LinearKeyAllocator] PRIMARY KEY CLUSTERED ([Key] ASC)
) ON [PRIMARY]
```
