using System;
using System.Collections.Generic;

namespace WebApp.Models
{ 
    public class PHRSDPT : JCLib.Mvc.BaseModel
    {
        public string RXTYPE { get; set; }
        public string RXDATEKIND { get; set; }
        public string DEADLINETIME { get; set; }
        public string WORKFLAG { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}

