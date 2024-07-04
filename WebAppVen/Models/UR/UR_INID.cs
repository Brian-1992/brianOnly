using System;
using System.Collections.Generic;

namespace WebAppVen.Models
{
    public class UR_INID : JCLib.Mvc.BaseModel
    {
        public string INID { get; set; }
        public string INID_O { get; set; }
        public string INID_NAME { get; set; }
        public string INID_OLD { get; set; }
        public string INID_FLAG { get; set; }
        public string INID_FLAG_NAME { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}