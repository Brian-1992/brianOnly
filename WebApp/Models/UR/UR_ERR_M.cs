using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UR_ERR_M : JCLib.Mvc.BaseModel
    {
        public string IDNO { get; set; }
        public string CTRL { get; set; }
        public string ACT { get; set; }
        public string MSG { get; set; }
        public string ST { get; set; }
        public string MSGV { get; set; }
        public DateTime ED { get; set; }
        public string TUSER { get; set; }
        public string OWNER { get; set; }
        public string IP { get; set; }
        public string UNA { get; set; }
    }
}