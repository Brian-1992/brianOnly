using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CC0003 : JCLib.Mvc.BaseModel
    {
        public string VACCINE { get; set; } //疫苗
        public string CONTRACNO { get; set; } //合約碼
        public string E_PURTYPE { get; set; } //案別
        public string PURDATE { get; set; } //採購日期
        public string PO_NO { get; set; } //採購單號
        public string PO_NO_REF { get; set; } //採購單號
        public string AGEN_NO { get; set; } //廠商代碼
        public string AGEN_NO_NAME { get; set; } //廠商名稱
        public string MMCODE { get; set; } //院內碼
        public string MMNAME_C { get; set; } //中文品名
        public string MMNAME_E { get; set; } //英文品名
        public string ACCOUNTDATE { get; set; } //進貨日期
        public string ACCOUNTDATE_REF { get; set; } //原進貨日期
        public string PO_QTY { get; set; } //預計進貨量
        public string DELI_QTY { get; set; } //實際進貨量
        public string DELI_QTY_REF { get; set; } //原實際進貨量
        public string INFLAG { get; set; } //進貨
        public string OUTFLAG { get; set; } //退貨
        public string PO_PRICE { get; set; } //單價
        public string PO_AMT { get; set; } //總金額
        public string M_PURUN { get; set; } //進貨單位
        public string LOT_NO { get; set; } //批號
        public string LOT_NO_REF { get; set; } //原批號
        public string EXP_DATE { get; set; } //效期
        public string EXP_DATE_REF { get; set; } //原效期
        public string MEMO { get; set; } //備註
        public string MEMO_REF { get; set; } //原備註
        public string WH_NO { get; set; } //庫房別
        public string STATUS { get; set; } //狀態
        public string ORI_QTY { get; set; } //進貨量
        public string SEQ { get; set; } //流水號
        public string M_DISCPERC { get; set; }//折讓比
        public string UNIT_SWAP { get; set; }//轉換率
        public string UPRICE { get; set; }//最小單價; 計量單位單價
        public string DISC_CPRICE { get; set; }//優惠合約單價
        public string DISC_UPRICE { get; set; }//優惠最小單價; 計量單位優惠單價
        public string TRANSKIND { get; set; }//異動類別
        public string IFLAG { get; set; }//新增識別

    }
}
