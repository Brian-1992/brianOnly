using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TC_PURCH_M
    {
        public string PUR_NO { get; set; }
        public string PUR_DATE { get; set; }
        public string TC_TYPE { get; set; }
        public string PUR_UNM { get; set; }
        public string PURCH_ST { get; set; }
        public string PUR_NOTE { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }


        public string TC_TYPE_NAME { get; set; }
        public string PUR_DATE_DISPLAY { get; set; }
        public string PUR_UNM_NAME { get; set; }
    }
}