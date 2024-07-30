using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_CSTM : JCLib.Mvc.BaseModel
    {
        public string CSTM { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_ID { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string UPDATE_IP { get; set; }
    }
}

