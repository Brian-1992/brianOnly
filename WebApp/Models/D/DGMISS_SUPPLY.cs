using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.D
{
    public class DGMISS_SUPPLY : JCLib.Mvc.BaseModel
    {
        public string SUPPLY_INID { get; set; }
        public string SUPPLY_INID_T { get; set; }
        public string APP_INID { get; set; }
        public string APP_INID_T { get; set; }
    }
}