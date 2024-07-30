using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CD0003M : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string WH_USERID { get; set; }
        public string IS_DUTY { get; set; }
        public string TUSER { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string ITEM_STRING { get; internal set; }
        public string WH_KIND { get; set; }
        public string WH_GRADE { get; set; }
        public string WH_NAME { get; set; }
        public string UNA { get; set; }
        public string INID { get; set; }
        public string INID_NAME { get; set; }
        public string WH_GRADE_D { get; set; }
        public string WH_KIND_D { get; set; }

    }
}