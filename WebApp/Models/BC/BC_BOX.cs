using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_BOX : JCLib.Mvc.BaseModel
    {
        public string BOXNO { get; set; }
        public string BARCODE { get; set; }
        public string XCATEGORY { get; set; }
        public string DESCRIPT { get; set; }
        public string STATUS { get; set; }
        public string CREATE_USER { get; set; }
        public string CREATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
    }

}