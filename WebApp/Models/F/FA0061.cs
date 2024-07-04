using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0061 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }                 //院內碼   
        public string MMNAME_E { get; set; }               //藥品英文名稱
        public string SELF_CONTRACT_NO { get; set; }       //合約案號
        public string SELF_PUR_UPPER_LIMIT { get; set; }   //採購上限金額
        public string SELF_CONT_BDATE { get; set; }        //合約生效起日
        public string SELF_CONT_EDATE { get; set; }        //合約生效迄日
        public string sum_PAY_AMT { get; set; }            //累計結報金額
        public string inqym { get; set; }                  //查詢年月


        public string ACCOUNTDATE { get; set; }   //進貨日期
        public string AGEN_NO { get; set; }       //廠商代碼
        public string M_PURUN { get; set; }       //單位
        public string PO_PRICE { get; set; }      //發票單價
        public string FLAG { get; set; }          //類別
        public string DELI_QTY { get; set; }      //進貨量
        public string PO_AMT { get; set; }        //發票金額
        public string MEMO { get; set; }          //說明
        public string DISC_AMT { get; set; }      //折讓金額
        public string DISC_CPRICE { get; set; }   //優惠價
        public string PAY_AMT { get; set; }       //優惠金額

    }
}