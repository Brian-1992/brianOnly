using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC
{
    public class getMedicalAllocateRqBody
    {
        [Required]
        public DateTime SDate { get; set; }
        [Required]
        public DateTime EDate { get; set; }

        public string DOCNO { get; set; }
        [Required]
        public string ADCNO { get; set; }

        public string FRWH { get; set; }

        public string TOWH { get; set; }
        [Required]
        public int ExecuteType { get; set; }





    }
}