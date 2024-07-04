using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UR_FUNC_LOG_M : JCLib.Mvc.BaseModel
    {
        public string IDNO { get; set; }
        public string TUSER { get; set; }
        public string CTRL { get; set; }
        public string ACT { get; set; }
        public DateTime CALL_TIME { get; set; }
        public string IP { get; set; }
        public string UNA { get; set; }
    }
}