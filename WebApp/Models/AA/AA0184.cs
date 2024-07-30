using System;
using System.Collections.Generic;

namespace WebApp.Models.AB
{
    public class AA0184 : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }//申請單號
        public string MAT_CLASS { get; set; }//物料分類
        public string COMBITEM { get; set; }//狀態
        public string FRWH { get; set; }//出庫庫房
        public string TOWH { get; set; }//入庫庫房
        public string APPTIME { get; set; }//申請時間
        public string MMCODE { get; set; }//院內碼
        public string MMNAME_C { get; set; }//中文名稱
        public string MMNAME_E { get; set; }//英文名稱
        public string BASE_UNIT { get; set; }//單位
        public string APPQTY { get; set; }//申請數量
        public string APVQTY { get; set; }//核可數量
        public string DIS_TIME { get; set; }//核撥日期
    }
}
