using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PH_EQPD : JCLib.Mvc.BaseModel
    {
        public string RECYM { get; set; } //月份
        public string MMCODE { get; set; } //院內碼
        public string WH_NO { get; set; } //庫房別
        public float ADVISEQTY { get; set; } //建議量
        public float AMOUNT { get; set; } //單筆價
        public float ESTQTY { get; set; } //預估量
        public float M_CONTPRICE { get; set; } //合約價
        public string M_PURUN { get; set; } //申購劑量單位
        public int STKQTY { get; set; } //現有存量
        public string CONTRACNO { get; set; } //合約
        public float ADVISEMONEY { get; set; } //建議金額
        public string AGEN_NO { get; set; } //廠商
        public string AGEN_NAMEC { get; set; } //廠商名稱        
        public string MMNAME_E { get; set; } //藥品名稱
        public int FLAG { get; set; } 
        public string CREATE_TIME { get; set; } //建立日期
        public string CREATE_USER { get; set; } //建立人員
        public string UPDATE_IP { get; set; } //異動IP
        public string UPDATE_TIME { get; set; } //異動日期
        public string UPDATE_USER { get; set; } //異動人員
        public string UNIT_SWAP { get; set; } //轉換率

        //惠軒 20180909
        public string BASE_UNIT { get; set; }
        public string INV_QTY { get; set; }

        public Int64 SUM1 { get; set; }
        public Int64 SUM2 { get; set; }
        /////////////////////////////////////
    }
}
