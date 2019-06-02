using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pensive
{
    [Table("DeltaPopulation")]
    public partial class DeltaPopulation
    {
        public int DeltaPopulationId { get; set; }
        [StringLength(255)]
        public string Place { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? DeltaCount { get; set; }
        public bool IsPredicted { get; set; }
    }
}