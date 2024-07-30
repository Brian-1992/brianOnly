using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CD_DCLUICNV : JCLib.Mvc.BaseModel
    {
        public string DCLYR { get; set; }
        public string MMCODE { get; set; }
        public string MED_LICENSE { get; set; }
        public string DECLARE_UI { get; set; }
        public string CNV_RATE { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}