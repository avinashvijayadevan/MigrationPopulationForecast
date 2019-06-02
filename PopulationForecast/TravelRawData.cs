using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pensive
{
    public partial class TravelRawData
    {
        public int TravelId { get; set; }
        [StringLength(255)]
        public string Origin { get; set; }
        [StringLength(255)]
        public string Destination { get; set; }
        [StringLength(20)]
        public string TravelDate { get; set; }
        [StringLength(20)]
        public string DateOfBirth { get; set; }
        public int? Mode { get; set; }
        public bool Gender { get; set; }
        public bool IsImported { get; set; }
    }
}