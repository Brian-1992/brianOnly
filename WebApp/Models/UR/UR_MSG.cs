using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UR_MSG : JCLib.Mvc.BaseModel
    {
        public string RECEIVE_USER { get; set; }
        public string UR_MSG_SEQ { get; set; }
        public string MSG_CONTENT { get; set; }
        public DateTime MSG_DATE { get; set; }
        public string SEND_USER { get; set; }
        public string SEND_USER_NAME { get; set; }
        public string READ_FLAG { get; set; }
        public string ALERT_FLAG { get; set; }
    }
}