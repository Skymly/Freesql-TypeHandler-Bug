using FreeSql.Internal.Model;

using Serilog;

using System;

namespace FreesqlTypeHandlerBug
{
    public class DateTimeTicksHandler : TypeHandler<DateTime>
    {
        public override DateTime Deserialize(object value)
        {
            var result = DateTime.FromBinary(Convert.ToInt64(value));
            Log.Debug("{method}: [{value}]=>[{result}]", nameof(Deserialize), value, result);
            return result;
        }

        public override object Serialize(DateTime value)
        {
            var result = value.Ticks;
            Log.Debug("{method}: [{value}]=>[{result}]", nameof(Serialize), value, result);
            return result;
        }
    }

    //我试图使用DateTime到其Ticks字符串的转换
    //查询时没有抛出异常 也没有符合预期的执行Deserialize方法 而是得到了DateTime的默认值
    public class DateTimeMillisecondsHandler : TypeHandler<DateTime>
    {
        public override DateTime Deserialize(object value)
        {
            var result = DateTime.FromBinary(Convert.ToInt64(value));
            Log.Debug("{method}: [{value}]=>[{result}]", nameof(Deserialize), value, result);
            return result;
        }

        public override object Serialize(DateTime value)
        {
            var result = value.Ticks.ToString();
            Log.Debug("{method}: [{value}]=>[{result}]", nameof(Serialize), value, result);
            return result;
        }
    }

}
