using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC
{
    public class updateMedOrdRqBody
    {
        [Required]
        public string ADCNO { get; set; }
        [Required]
        public string DOCNO { get; set; }
        [Required]
        public decimal SEQ { get; set; }
        [Required]
        public decimal APPQTY { get; set; }
        [Required]
        public decimal APVQTY { get; set; }
        [Required]
        public string APVID { get; set; }
        [Required]
        public string LOTNO { get; set; }
        [Required]
        public string EXPDATE { get; set; }
        [Required]
        public string STOCKCODE { get; set; }

    }
}