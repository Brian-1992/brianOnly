using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class FA0055 : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }      // 月份 DATA_YM
        public string F2 { get; set; }      // 上月結存成本 P_INV_COST
        public string F3 { get; set; }      // 本月買進成本 IN_COST-TPEO_INCOST+OTH_INCOST+OTH_INCOST_2+OTH_INCOST_3
        public string F4 { get; set; }      // 本月醫令消耗成本 USE_COST_O+USE_COST_H
        public string F5 { get; set; }      // 藥局盤盈虧調整金額 INVENT_COSTINVENT_COST_O
        public string F6 { get; set; }      // 銷毀過效期品 DIS_COST
        public string F7 { get; set; }      // 退貨金額 REJ_COST
        public string F8 { get; set; }      // 2-4級庫調整金額 ADJ_COST_24
        public string F9 { get; set; }      // 本月結存成本 INV_COST
        public string F10 { get; set; }     // 調整金額 ADJ_COST_OTH
        public string F11 { get; set; }     // 藥材成本 USE_COST_O+USE_COST_H+USE_COST_OTH+ADJ_COST_24+DIS_COST
        public string F12 { get; set; }     // 住院藥費消耗成本 USE_COST_H
        public string F13 { get; set; }     // 門診藥費消耗成本 USE_COST_O
        public string F14 { get; set; }     // 藥材總收入 INCOME_AMT
        public string F15 { get; set; }     // 台北門診中心買藥金額 TPEO_INCOST
        public string F16 { get; set; }     // 本院買(管制藥、抗瘧藥、蛇毒血清)金額 OTH_INCOST+OTH_INCOST_2+OTH_INCOST_3
        public string F17 { get; set; }     // 其他消耗成本 USE_COST_OTH
        public string F18 { get; set; }     // 藥局盤盈調整金額 INVENT_COST
        public string F19 { get; set; }     // 藥局盤虧調整金額 INVENT_COST_O
        public string F20 { get; set; }     // 本院買(管制藥)金額 OTH_INCOST
        public string F21 { get; set; }     // 本院買(抗瘧藥)金額 OTH_INCOST_2
        public string F22 { get; set; }     // 本院買(蛇毒血清)金額 OTH_INCOST_3
        public string F23 { get; set; }     // 一般藥材費用 IN_COST
        public string F24 { get; set; }     // 急診藥費消耗成本 USE_COST_E
    }
}