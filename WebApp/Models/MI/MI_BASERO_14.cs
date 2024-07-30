using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.MI
{
    public class MI_BASERO_14
    {
        public string MMCODE { get; set; }
        public string RO_WHTYPE { get; set; }
        public string RO_TYPE { get; set; }
        public string NOW_RO { get; set; }
        public string DAY_USE_10 { get; set; }
        public string DAY_USE_14 { get; set; }
        public string DAY_USE_90 { get; set; }
        public string MON_USE_1 { get; set; }
        public string MON_USE_2 { get; set; }
        public string MON_USE_3 { get; set; }
        public string MON_USE_4 { get; set; }
        public string MON_USE_5 { get; set; }
        public string MON_USE_6 { get; set; }
        public string MON_AVG_USE_3 { get; set; }
        public string MON_AVG_USE_6 { get; set; }
        public string G34_MAX_APPQTY { get; set; }
        public string SUPPLY_MAX_APPQTY { get; set; }
        public string PHR_MAX_APPQTY { get; set; }
        public string WAR_QTY { get; set; }
        public string SAFE_QTY { get; set; }
        public string NORMAL_QTY { get; set; }
        public string DIFF_PERC { get; set; }
        public string SAFE_PERC { get; set; }
        public string DAY_RO { get; set; }
        public string MON_RO { get; set; }
        public string G34_PERC { get; set; }
        public string SUPPLY_PERC { get; set; }
        public string PHR_PERC { get; set; }
        public string NORMAL_PERC { get; set; }
        public string WAR_PERC { get; set; }
        public string WH_NO { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        // 前端 Excel 使用
        public string CHECK_RESULT { get; set; } //檢核結果
    }
}