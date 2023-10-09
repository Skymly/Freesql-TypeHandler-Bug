#define SQLITE
using FreeSql;
using FreeSql.Sqlite;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

using System;
using System.Linq;
using System.Threading.Tasks;

using static FreeSql.Internal.GlobalFilter;

namespace FreesqlTypeHandlerBug
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{@Level:u3}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Sixteen)
                .WriteTo.File("Logs/.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //此编译符号在项目文件.csproj中设置
#if DateTimeTicksHandler
            FreeSql.Internal.Utils.TypeHandlers.TryAdd(typeof(DateTime), new DateTimeTicksHandler());//抛出异常
#else
            FreeSql.Internal.Utils.TypeHandlers.TryAdd(typeof(DateTime), new DateTimeMillisecondsHandler());
#endif
            IFreeSql fsql = new FreeSqlBuilder()
#if SQLITE
                .UseConnectionString(DataType.Sqlite, "Data Source=TypeHandlerTest.db", typeof(SqliteProvider<>))
#else
                .UseConnectionString(DataType.SqlServer, "Server=.;Database=TypeHandlerTest;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False")
#endif
                .UseAutoSyncStructure(true)
                .UseNoneCommandParameter(true)
                .Build();

            fsql.Aop.CurdBefore+=Aop_CurdBefore;
            fsql.Aop.CurdAfter+=Aop_CurdAfter;
            Random random = new Random(Guid.NewGuid().GetHashCode());
            fsql.Delete<TestEntity>().Where(_ => true).ExecuteAffrows();

            TestEntity[] entities = Enumerable.Range(1, 5).Select(x => new TestEntity
            {
                Id = x,
                CreateTime = DateTime.Now,
            }).ToArray();

            fsql.Insert<TestEntity>().AppendData(entities).ExecuteAffrows();


            //直接读表查看数据
            var list = fsql.Select<object>().AsTable((_, _) => nameof(TestEntity)).ToList();
            list.ForEach(x => Log.Information("{@item}", x));


            /// 此处不论是否使用异步
            /// 都无法按照预期进入<see cref="DateTimeTicksHandler.Deserialize(object)"/>方法
            /// var list2 = fsql.Select<TestEntity>().ToList();
            /// var list2 = fsql.Select<object>().AsType(typeof(TestEntity)).ToList();
            var list2 = await fsql.Select<TestEntity>().ToListAsync();
            list2.ForEach(x => Log.Information("{@item}", x));

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