using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeltaCalculator
{
    [Table("PassengerInfo_New")]
    public partial class PassengerInfoNew
    {
        public int? Age { get; set; }
        public bool? Gender { get; set; }
        [StringLength(255)]
        public string Orign { get; set; }
        [StringLength(255)]
        public string Destination { get; set; }
        [StringLength(255)]
        public string Mode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? TravelDate { get; set; }
        public int? TravelYear { get; set; }
        public int? TravelMonth { get; set; }
        public int? TravelDay { get; set; }
        public int PassengerInfoId { get; set; }
    }
}