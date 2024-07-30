using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CHK_MNSET : JCLib.Mvc.BaseModel
    {
        public string CHK_YM { get; set; }
        public string SET_CTIME { get; set; }
        public string SET_ATIME { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }


        public string PRE_YM { get; set; }
        public string POST_YM { get; set; }
    }
}