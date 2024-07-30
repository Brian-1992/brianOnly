using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class SEC_MAST : JCLib.Mvc.BaseModel
    {
        public string SECTIONNO { get; set; }
        public string SECTIONNAME { get; set; }
        public string SEC_ENABLE { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}