using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ChkWh : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string WH_GRADE { get; set; }
        public int CHK_TOTAL { get; set; }
        public string INID { get; set; }
    }
}