# LinearKeyAllocator

LinearKeyAllocator allows to generate incremental keys (int, long) based
on last seed value stored in database.

Works likes the Hilo algorithm (from NHibernate) but better becouse
do not create big holes when application restarts.

It aims to be a tool for generating database keys without having to
rely on RDBMS identity column.

LinearKeyAllocator works like database 'Sequence'.
It means does not attach to any ambient transaction.

Once generated chunk of keys are available until application restarts.
After restart it will start from last saved max value.

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
