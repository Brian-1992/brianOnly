using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC.UpdateMedicalCheckAccept
{
    public class UpdateMedicalCheckAcceptRqBody
    {
        [Required]
        public string ADCNO { get; set; }
        [Required]
        public string DOCNO { get; set; }
        [Required]
        public int SEQ { get; set; }
        [Required]
        public int APPQTY { get; set; }
        [Required]
        public int ACKQTY { get; set; }
        [Required]
        public string ACKID { get; set; }
        [Required]
        public string LOTNO { get; set; }
        [Required]
        public string EXPDATE { get; set; }
        [Required]
        public string STOCKCODE { get; set; }
    }
}