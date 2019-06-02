using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pensive
{
    [Table("OrignAggregation")]
    public partial class OrignAggregation
    {
        [StringLength(255)]
        public string Origin { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Count { get; set; }
        public int OriginAggregation { get; set; }
    }
}