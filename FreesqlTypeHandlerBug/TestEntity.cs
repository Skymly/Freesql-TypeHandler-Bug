using FreeSql.DataAnnotations;

using System;

namespace FreesqlTypeHandlerBug
{
    public class TestEntity
    {
        [Column(IsPrimary = true, IsIdentity = false)]
        public int Id { get; set; }

        [Column(MapType = typeof(long))]
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

    }

}