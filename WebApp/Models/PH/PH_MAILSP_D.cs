using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PH_MAILSP_D : JCLib.Mvc.BaseModel
    {
        public string INID { get; set; }
        public string MSGRECNO { get; set; }
        public string MSGNO { get; set; }
        public string AGEN_NO { get; set; }
        public string M_CONTID { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string CONTRACNO { get; set; }
        
    }
}