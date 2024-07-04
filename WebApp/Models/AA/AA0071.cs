using System;

namespace WebApp.Models
{
    public class AA0071 : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; } //單據號碼
        public string APPTIME { get; set; } //申請日期
        public string DOCTYPE_N { get; set; } //異動代碼
        public string MAT_CLASS_N { get; set; } //物料分類
        public string MMCODE { get; set; } //院內碼
        public string MMNAME { get; set; } //品名
        public string APPQTY { get; set; } //調帳數量
        public string BASE_UNIT { get; set; } //單位
        public string M_CONTPRICE { get; set; } //合約單價
    }
}