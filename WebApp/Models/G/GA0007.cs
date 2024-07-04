using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class GA0007 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string IS_USE { get; set; }
        public string CREATE_TIME_T { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

    }
}