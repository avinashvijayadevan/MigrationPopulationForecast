using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pensive
{
    [Table("DestinationAggregation")]
    public partial class DestinationAggregation
    {
        [StringLength(255)]
        public string Destination { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Count { get; set; }
        public int DestinationAggregationId { get; set; }
    }
}