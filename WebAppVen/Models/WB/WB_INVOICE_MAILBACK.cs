using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppVen.Models
{
    public class WB_INVOICE_MAILBACK : JCLib.Mvc.BaseModel
    {
       public string INVOICE_BATNO { get; set; }
        public string BACK_DT { get; set; }
        public string STATUS { get; set; }
        public string CREATE_TIME { get; set; }
    }
}