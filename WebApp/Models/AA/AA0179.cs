using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0179 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }//院內碼
        public string MMNAME_C { get; set; }//品名
        public string UNITRATE { get; set; }//出貨單位
        public string TRUTRATE { get; set; }//轉換量
        public string MAT_CLSNAME { get; set; }//類別
        public string RO_TYPE { get; set; }//基準量模式
        public string NOW_RO { get; set; }//現用基準量
        public string DAY_USE_10 { get; set; }//日平均消耗10天
        public string DAY_USE_14 { get; set; }//日平均消耗14天
        public string DAY_USE_90 { get; set; }//日平均消耗90天
        public string MON_USE_1 { get; set; }//前第一個月消耗
        public string MON_USE_2 { get; set; }//前第二個月消耗
        public string MON_USE_3 { get; set; }//前第三個月消耗
        public string MON_USE_4 { get; set; }//前第四個月消耗
        public string MON_USE_5 { get; set; }//前第五個月消耗
        public string MON_USE_6 { get; set; }//前第六個月消耗
        public string MON_AVG_USE_3 { get; set; }//三個月平均消耗量
        public string MON_AVG_USE_6 { get; set; }//六個月平均消耗量
        public string G34_MAX_APPQTY { get; set; }//護理病房最大請領量
        public string SUPPLY_MAX_APPQTY { get; set; }//供應中心最大請領量
        public string PHR_MAX_APPQTY { get; set; }//藥局請領最大量
        public string WAR_QTY { get; set; }//戰備存量
        public string SAFE_QTY { get; set; }//安全庫存量
        public string NORMAL_QTY { get; set; }//正常庫存量
        public string DIFF_PERC { get; set; }//誤差百分比
        public string SAFE_PERC { get; set; }//安全存量比值百分比
        public string DAY_RO { get; set; }//日基準量
        public string MON_RO { get; set; }//月基準量
        public string WARBAK { get; set; }//是否戰備
        public string BASE_UNIT { get; set; }//單位
        public string INV_QTY_1 { get; set; }//藥材庫現有存量
        public string INV_QTY_2 { get; set; }//藥局現有存量
        public string INV_QTY_3 { get; set; }//供應中心存量
        public string NORMAL_PERC { get; set; }//正常庫存百分比
        public string RO_WHTYPE { get; set; }//程式編碼
        public string G34_PERC { get; set; }//護理病房最大請領RO倍數
        public string WH_NO { get; set; } // 庫房編號
        public string UPDATE_IP { get; set; } //上次更新ip位址
        public string UPDATE_USER { get; set; }//上次更新人員
        public string UPDATE_TIME { get; set; }//上次更新時間
    }
}