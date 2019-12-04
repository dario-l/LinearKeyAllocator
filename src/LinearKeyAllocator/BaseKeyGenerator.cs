using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace LinearKeyAllocator
{
    internal abstract class BaseKeyGenerator
    {
        private const byte keyMaxLength = 255;
        private readonly string _connectionString;

        protected BaseKeyGenerator(string key, ushort seedSize, string connectionString)
        {
            Key = key;
            SeedSize = seedSize;

            _connectionString = connectionString;
        }

        public string Key { get; }
        public ushort SeedSize { get; }

        protected async Task<object> GetNextMaxFor(string key, ushort seedSize, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key can not be null or empty.", nameof(key));

            if (key.Length > keyMaxLength)
                throw new ArgumentOutOfRangeException($"Key can be max {keyMaxLength} long.", nameof(key));

            using var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
            await using var cnn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(queryGetNextMaxFor, cnn) { CommandType = CommandType.Text };
            cmd.Parameters.Add("@Key", SqlDbType.VarChar, keyMaxLength).Value = key;
            cmd.Parameters.Add("@SeedSize", SqlDbType.Int).Value = seedSize;

            await cnn.OpenAsync(token);
            var result = await cmd.ExecuteScalarAsync(token);

            if (result == DBNull.Value || result == null)
                throw new Exception($"Result of next value for {key} was calculated as null.");
            scope.Complete();

            Debug.WriteLine("Saved next max for {0} with result {1}.", key, result);

            return result;
        }

        private const string queryGetNextMaxFor = @"
DECLARE @Value bigint = @SeedSize
MERGE [dbo].[LinearChunkAllocator] WITH (ROWLOCK) AS [Target]
USING (SELECT @Key AS [Key]) AS [Source] ON [Target].[Key] = [Source].[Key]
WHEN MATCHED THEN UPDATE SET NextMax = (NextMax + @SeedSize), @Value = (NextMax + @SeedSize)
WHEN NOT MATCHED THEN INSERT([Key], NextMax)VALUES(@Key, @SeedSize);SELECT @Value;";

        public static async Task Initialize(string connectionString, CancellationToken token = default)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
            await using var cnn = new SqlConnection(connectionString);
            await using var cmd = new SqlCommand(queryInitialize, cnn) { CommandType = CommandType.Text };
            await cnn.OpenAsync(token);
            await cmd.ExecuteNonQueryAsync(token);
            scope.Complete();
        }

        private const string queryInitialize = @"
IF OBJECT_ID('[dbo].[LinearChunkAllocator]', 'U') IS NULL BEGIN
    CREATE TABLE [dbo].[LinearChunkAllocator](
        [Key] [varchar](255) NOT NULL,
        [NextMax] [bigint] NOT NULL,
        CONSTRAINT [PK_LinearChunkAllocator] PRIMARY KEY CLUSTERED ([Key] ASC)
    ) ON [PRIMARY]
END";
    }
}