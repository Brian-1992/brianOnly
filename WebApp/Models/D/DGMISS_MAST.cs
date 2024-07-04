using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.D
{
    public class DGMISS_MAST : JCLib.Mvc.BaseModel
    {
        public string INID { get; set; }
        public string INID_T { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}