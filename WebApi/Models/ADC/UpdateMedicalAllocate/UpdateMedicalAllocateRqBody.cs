using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC.UpdateMedicalAllocate
{
    public class UpdateMedicalAllocateRqBody
    {
        [Required]
        public string ADCNO { get; set; }
        public string DOCNO { get; set; }
        public int SEQ { get; set; }
        public int APPQTY { get; set; }
        public int APVQTY { get; set; }
        public int ACKQTY { get; set; }
        public int USERID { get; set; }
        public string LOTNO { get; set; }
        public string EXPDATE { get; set; }
        public string FRWH { get; set; }
        public string TOWH { get; set; }
        public int ExecuteType { get; set; }
    }
}