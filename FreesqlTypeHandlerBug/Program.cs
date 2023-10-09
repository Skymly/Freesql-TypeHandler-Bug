using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Sqlite;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreesqlTypeHandlerBug
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{@Level:u3}] {Message:lj}{NewLine}{Exception}", theme:AnsiConsoleTheme.Sixteen)
                .WriteTo.File( "Logs/.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            FreeSql.Internal.Utils.TypeHandlers.TryAdd(typeof(DateTime), new DateTimeTicksHandler());

            IFreeSql fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=test.db", typeof(SqliteProvider<>))
                .UseAutoSyncStructure(true)
                .Build();

            fsql.Aop.CurdBefore+=Aop_CurdBefore;
            fsql.Aop.CurdAfter+=Aop_CurdAfter;
            Random random = new Random(Guid.NewGuid().GetHashCode());
            fsql.Delete<TestEntity>().Where(_ => true).ExecuteAffrows();

            TestEntity[] entities = Enumerable.Range(1, 10).Select(x => new TestEntity
            {
                Id = x,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            }).ToArray();

            fsql.Insert<TestEntity>().AppendData(entities).ExecuteAffrows();

            /// 此处不论是否使用异步
            /// 都无法按照预期进入<see cref="DateTimeTicksHandler.Deserialize(object)"/>方法
            /// var list = fsql.Select<TestEntity>().ToList();
            var list = await fsql.Select<TestEntity>().ToListAsync();
            Log.Information("查库:{@list}", list);

        }

        private static void Aop_CurdAfter(object sender, FreeSql.Aop.CurdAfterEventArgs e)
        {
            Log.Information("{@method} {@type} {@sql}", "CurdAfter", e.CurdType, e.Sql);
        }

        private static void Aop_CurdBefore(object sender, FreeSql.Aop.CurdBeforeEventArgs e)
        {
            Log.Debug("{@method} {@type} {@sql}", "CurdBefore", e.CurdType, e.Sql);
        }
    }

}