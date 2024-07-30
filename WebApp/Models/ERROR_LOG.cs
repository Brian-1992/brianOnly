using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ERROR_LOG : JCLib.Mvc.BaseModel
    {
        public string LOGTIME { get; set; }
        public string PG { get; set; }
        public string MSG { get; set; }
        public string USERID { get; set; }
    }
}