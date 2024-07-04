using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0008M : JCLib.Mvc.BaseModel
    {
        public string ROWORDER { get; set; }       // 排序用
        public string MAT { get; set; }            // 類別
        public string PIQ_PA_G1 { get; set; }      // 期初存貨成本 - 中央庫房
        public string PIQ_PA_G2 { get; set; }      // 期初存貨成本 - 衛星庫房
        public string IQ_IP { get; set; }          // 進貨成本
        public string OQ_A_AP_G1 { get; set; }     // 內湖消耗成本 - 中央庫房
        public string OQ_A_AP_G2 { get; set; }     // 內湖消耗成本 - 衛星庫房
        public string I_AP_G1 { get; set; }      // 盤盈虧 - 中央庫房
        public string I_AP_G2 { get; set; }      // 盤盈虧 - 衛星庫房
        public string A_PA { get; set; }           // 台北門診應收帳款,
        public string IQ_PA_G1 { get; set; }       // 期末庫存成本 - 中央庫房
        public string IQ_PA_G2 { get; set; }       // 期末庫存成本 - 中央庫房
        public string A_PC { get; set; }           // 寄售衛材消耗成本


        public string I_AP_G1_P { get; set; }       // 盤盈-中央庫房
        public string I_AP_G1_N { get; set; }       // 盤虧-中央庫房
        public string I_AP_G2_P { get; set; }       // 盤盈-衛星庫房
        public string I_AP_G2_N { get; set; }       // 盤虧-衛星庫房
    }
}