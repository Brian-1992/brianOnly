using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MI_WINVCTL : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string SAFE_DAY { get; set; }
        public string OPER_DAY { get; set; }
        public string SHIP_DAY { get; set; }
        public int SAFE_QTY { get; set; }
        public int OPER_QTY { get; set; }
        public int SHIP_QTY { get; set; }
        public int DAVG_USEQTY { get; set; }
        public int HIGH_QTY { get; set; }
        public int LOW_QTY { get; set; }
        public int MIN_ORDQTY { get; set; }
        public string SUPPLY_WHNO { get; set; }
        public string IS_AUTO { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}