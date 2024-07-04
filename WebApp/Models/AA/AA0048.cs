using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0048 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string SAFE_DAY { get; set; }
        public string OPER_DAY { get; set; }
        public string SHIP_DAY { get; set; }
        public string SAFE_QTY { get; set; }
        public string OPER_QTY { get; set; }
        public string SHIP_QTY { get; set; }
        public string DAVG_USEQTY { get; set; }
        public string HIGH_QTY { get; set; }
        public string LOW_QTY { get; set; }
        public string MIN_ORDQTY { get; set; }
        public string INV_QTY { get; set; }
        public string SUPPLY_WHNO { get; set; }
        public string IS_AUTO { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string STORE_LOC { get; set; }
        public string STOCKUNIT { get; set; }
        public string CTDMDCCODE { get; set; }
        public string CTDMDCCODE_N { get; set; }
        public string MMNAME { get; set; }
        public string E_RESTRICTCODE { get; set; }
        public string E_RESTRICTCODE_N { get; set; }
        public string DANGERDRUGFLAG { get; set; }
        public string DANGERDRUGFLAG_N { get; set; }
        public string NOWCONSUMEFLAG { get; set; }
        public string RESERVEFLAG { get; set; }
        public string STATUS_DISPLAY { get; set; }
        public string maxEXP_DATE { get; set; }


        // 醫令扣庫規整
        public string USEADJ_CLASS { get; set; }
        public string USEADJ_CLASS_NAME { get; set; }

        public string WH_KIND { get; set; }

        public string WH_NO_DISPLAY { get; set; }
        public string MMCODE_DISPLAY { get; set; }
        public string SAFE_DAY_DISPLAY { get; set; }
        public string OPER_DAY_DISPLAY { get; set; }
        public string SHIP_DAY_DISPLAY { get; set; }
        public string HIGH_QTY_DISPLAY { get; set; }
        public string LOW_QTY_DISPLAY { get; set; }
        public string MIN_ORDQTY_DISPLAY { get; set; }
        public string IS_AUTO_DISPLAY { get; set; }
        public string CTDMDCCODE_DISPLAY { get; set; }
        public string USE_ADJCLASS_DISPLAY { get; set; }
        public string WH_GRADE { get; set; }

        public string ISSPLIT { get; set; }
        public string ISSPLIT_DISPLAY { get; set; }

        #region 2022-05-20 秀甄修改AA0048新增 
          public string SAFE_QTY_90 { get; set; } //安全量_90
        public string FSTACKDATE { get; set; } //第一次接收日期
        public string DAVG_USEQTY_90 { get; set; } //90天日平均消耗量
        public string OPER_QTY_90 { get; set; } //作業量90
        public string SHIP_QTY_90 { get; set; } //運補量90
        public string HIGH_QTY_90 { get; set; } //基準量90
        #endregion
    }
}