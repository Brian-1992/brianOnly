using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC.UpdateMedicalReturn
{
    public class UpdateMedicalReturnRqbody
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
        public int USERID { get; set; }
        [Required]
        public string LOTNO { get; set; }
        [Required]
        public string EXPDATE { get; set; }
        [Required]
        public string FRWH { get; set; }
        [Required]
        public string TOWH { get; set; }
        [Required]
        public int ExecuteType { get; set; }
    }
}