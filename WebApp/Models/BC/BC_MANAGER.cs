using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BC_MANAGER : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MANAGERID { get; set; }
        public string MANAGERNM { get; set; }
        public string USERID { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string MNAME { get; set; }
        public string CNT { get; set; }
        public string DISPLAY_WHNO { get; set; }//白底顯示
        public string TEXT_WHNO { get; set; }//紅底顯示
        public string DISPLAY_MANAGERID { get; set; }//白底顯示
        public string TEXT_MANAGERID { get; set; }//紅底顯示
        public string TEXT_MANAGERNM { get; set; } //紅底顯示
    }
}