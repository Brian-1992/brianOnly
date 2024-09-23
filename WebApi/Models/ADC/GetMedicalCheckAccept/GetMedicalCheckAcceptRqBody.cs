using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC.GetMedicalCheckAccept
{
    public class GetMedicalCheckAcceptRqBody
    {
        [Required]
        public DateTime SDate { get; set; }
        [Required]
        public DateTime EDate { get; set; }

        public string DOCNO { get; set; }
        [Required]
        public string ADCNO { get; set; }
        [Required]
        public string TOWH { get; set; }
    }
}