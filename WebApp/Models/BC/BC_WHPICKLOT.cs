using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_WHPICKLOT : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string PICK_DATE { get; set; }
        public string LOT_NO { get; set; }
        public string PICK_USERID { get; set; }
        public string PICK_STATUS { get; set; }
        public string CREATE_USER { get; set; }
        public string CREATE_DATE { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string PICK_USERNAME { get; set; }
        public string APPITEM_SUM { get; set; }
        public string APPQTY_SUM { get; set; }

        public string APP_CNT { get; set; }
    }
}