using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class UR_DOC : JCLib.Mvc.BaseModel
    {
        public string DK { get; set; }
        public string DN { get; set; }
        public string DD { get; set; }
        public string UK { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}