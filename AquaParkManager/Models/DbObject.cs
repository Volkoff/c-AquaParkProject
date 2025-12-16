using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace AquaParkManager.Models
{
    // Mapuje pohled V_DB_OBJECTS (Systémový katalog)
    public class DbObject
    {
        [Column("OBJECT_TYPE")]
        public string? ObjectType { get; set; }

        [Column("OBJECT_NAME")]
        public string? ObjectName { get; set; }

        [Column("STATUS")]
        public string? Status { get; set; }

        [Column("LAST_DDL_TIME")]
        public DateTime? LastDdlTime { get; set; }
    }
}