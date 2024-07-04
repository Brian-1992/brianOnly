using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_AB0071 : JCLib.Mvc.BaseModel
    {
        public string CHARTNO{ get; set; }//病歷號
        public string MEDNO{ get; set; }//病歷號碼
        public string VISITSEQ{ get; set; }//門/住診 
        public string ORDERNO{ get; set; }//醫令序號
        public string DETAILNO{ get; set; }//明細序號
        public string ORDERCODE{ get; set; }//院內碼
        public string USEQTY{ get; set; }//數量
        public string CHINNAME{ get; set; }//開立醫師
        public string SIGNOPID{ get; set; }//給藥人員
        public string CREATEDATETIME{ get; set; }//日期
        public string STOCKCODE{ get; set; }//庫房代碼
        public string INOUTFLAG{ get; set; }//A:買 D:退
        public string ORDERENGNAME{ get; set; }//英文名
        public string RESTRICTCODE{ get; set; }//管制用藥
        public string HIGHPRICEFLAG{ get; set; }//高價 
        public string NRCODE{ get; set; }//病房 
        public string BEDNO{ get; set; }//床位 
        public string NRCODENAME{ get; set; }//病房-床位
        public string DOSE{ get; set; }//劑量
        public string ORDERUNIT{ get; set; }//劑量單位
        public string PATHNO{ get; set; }//途徑
        public string FREQNO{ get; set; }//頻率
        public string PAYFLAG{ get; set; }//自費
        public string BUYFLAG{ get; set; }//自備
        public string BAGSEQNO{ get; set; }//藥袋號
        public string RXNO{ get; set; }//處方箋
    }
}