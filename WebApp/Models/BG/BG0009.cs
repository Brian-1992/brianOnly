using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebApp.Models
{
    public class BG0009 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼
        public string MMNAME_C { get; set; } //中文名稱
        public string BASE_UNIT { get; set; } //單位
        public string M_CONTPRICE { get; set; } //單價
        public string DISC_CPRICE { get; set; } //優惠單價
        public string P_INV_QTY { get; set; }  //上月結存
        public string IN_QTY { get; set; }  //本月進貨
        public string REJ_OUTQTY { get; set; }  //本月退貨
        public string MIL_QTY { get; set; }  //本月軍用
        public string OUT_QTY { get; set; }  //本月民用
        public string T_OUT_QTY { get; set; }  //總支用
        public string INV_QTY { get; set; }  //本月結存
        public string WRESQTY { get; set; }  //戰備存量
        public string TOT_AMT_3 { get; set; } //聯標契約優惠
        public string EXTRA_DISC_AMOUNT { get; set; } //折讓金額
        public string TOT_AMT_1 { get; set; } //應付總價
        public string TOT_AMT_2 { get; set; } //優惠應付總價
        public string E_SOURCECODE { get; set; } //來源代碼
        public string M_AGENNO { get; set; } //廠商代碼
        public string EASYNAME { get; set; }
        public string AGEN_NAME { get; set; }
        public string PO_NO { get; set; }
        public string UNI_NO { get; set; } //廠商統編
        public string SPXFEE { get; set; } //特材代碼
        public string M_NHIKEY { get; set; } //健保碼
        public string NHI_PRICE { get; set; } //健保價
    }
    public class BG0009Count
    {
        //軍
        public string M_TOT1 { get; set; }    // 寄庫應付款_軍應付
        public string M_TOT2 { get; set; }    // 進貨應付款_軍應付
        public string M_TOT3 { get; set; }    // 小計金額_軍應付
        public string M_TOT4 { get; set; }    // 實應付_軍應付
        // 民
        public string P_TOT1 { get; set; }  // 寄庫應付款_民應付
        public string P_TOT2 { get; set; }  // 進貨應付款_民應付
        public string P_TOT3 { get; set; }  // 小計金額_民應付
        public string P_TOT4 { get; set; }  // 實應付_民應付
        //折讓總金額
        public string EXTRA_DISC_AMOUNT { get; set; }
        //聯標契約優惠
        public string TOT_AMT_3 { get; set; }
        public string MEMO { get; set;}
    }
}
