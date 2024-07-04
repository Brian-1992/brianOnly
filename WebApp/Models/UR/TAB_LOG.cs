using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TSGH.Models
{
    public class TAB_LOG : JCLib.Mvc.BaseModel
    {
        public string FIELD_NAME { get; set; }
        public string NEW_VALUE { get; set; }
        public string OLD_VALUE { get; set; }
        public string PROC_DT { get; set; }
        public string PROC_USER { get; set; }
        public string PROC_IP { get; set; }
    }
}