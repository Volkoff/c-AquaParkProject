using System.ComponentModel.DataAnnotations.Schema;

namespace AquaParkManager.Models
{
    // Mapuje pohled V_AREA_HIERARCHY (Hierarchický dotaz CONNECT BY)
    public class AreaHierarchy
    {
        [Column("AREA_ID")]
        public int AreaId { get; set; }

        [Column("NAME")]
        public string? Name { get; set; }

        [Column("PARENT_AREA_ID")]
        public int? ParentAreaId { get; set; }

        [Column("LVL")]
        public int Level { get; set; }

        [Column("PATH_TXT")]
        public string? PathText { get; set; }
    }
}