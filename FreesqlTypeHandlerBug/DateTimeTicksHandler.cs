using FreeSql.Internal.Model;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
