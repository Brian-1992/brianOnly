using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TC_PURCH_DL
    {
        public string PUR_NO { get; set; }
        public string MMCODE { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string MMNAME_C { get; set; }
        public string PUR_QTY { get; set; }
        public string PUR_UNIT { get; set; }
        public string IN_PURPRICE { get; set; }
        public string PUR_AMOUNT { get; set; }


        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }



        public string BASE_UNIT { get; set; }       // 計量單位
        public string M6AVG_USEQTY { get; set; }    // 前6個月平均消耗量
        public string INV_DAY { get; set; }         // 庫存天數
        public string ORI_AGEN_NAMEC { get; set; }         // 原始廠商
    }
}