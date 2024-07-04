using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BG0006M : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }       //訂單號碼
        public string MMCODE { get; set; }      //院內碼
        public string PO_QTY { get; set; }      //訂單數量(包裝單位數量)
        public string PO_PRICE { get; set; }        //訂單單價(合約單價)
        public string M_PURUN { get; set; }     //申購計量單位
        public string M_AGENLAB { get; set; }       //廠牌
        public string PO_AMT { get; set; }      //總金額
        public string M_DISCPERC { get; set; }      //折讓比
        public string DELI_QTY { get; set; }        //已交數量
        public string BW_SQTY { get; set; }     //借貨數量
        public string DELI_STATUS { get; set; }     //交貨狀態 C-已交貨
        public string CREATE_TIME { get; set; }     //建立時間
        public string CREATE_USER { get; set; }     //建立人員代碼
        public string UPDATE_TIME { get; set; }     //異動時間
        public string UPDATE_USER { get; set; }     //異動人員代碼
        public string UPDATE_IP { get; set; }       //異動IP
        public string MEMO { get; set; }        //備註
        public string PR_NO { get; set; }       //申購單號
        public string UNIT_SWAP { get; set; }       //轉換率
        public string INVOICE { get; set; }     //發票號碼
        public string INVOICE_DT { get; set; }      //發票號碼日期
        public string CKIN_QTY { get; set; }        //發票驗證數量
        public string CHK_USER { get; set; }        //發票驗證人員
        public string CHK_DT { get; set; }      //發票驗證日期
        public string ACCOUNTDATE { get; set; }     //入帳日期
        public string STATUS { get; set; }      //default 'N' , D-作廢,    N-申購
        public string UPRICE { get; set; }      //最小單價(計量單位單價)
        public string DISC_CPRICE { get; set; }     //優惠合約單價
        public string DISC_UPRICE { get; set; }		//優惠最小單價(計量單位單價)
        public DateTime PO_DATE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string DISC_AMOUNT { get; set; }
        public string PAY_AMOUNT { get; set; }        
        public string DELI_DT { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string M_PHCTNCO { get; set; }
        public string CONTRACNO { get; set; }
        public string M_MATID { get; set; }
        public string PRICE_AMOUNT { get; set; }        //申購金額
        public string ACC_TIME { get; set; }        //進貨日期
        public string ACC_QTY { get; set; }        //數量
        public string LOT_NO { get; set; }        //數量
        public string EXP_DATE { get; set; }        //數量
    }
}