using FreeSql.DataAnnotations;

using System;

namespace FreesqlTypeHandlerBug
{
    public class TestEntity
    {
        [Column(IsPrimary = true, IsIdentity = false)]
        public int Id { get; set; }

#if DateTimeTicksHandler
        [Column(MapType = typeof(long))]
#else
        [Column(MapType = typeof(string))]
#endif
        public DateTime CreateTime { get; set; }

    }

}