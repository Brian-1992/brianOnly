using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0084M : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }       //--庫房別
        public string WH_NAME { get; set; }     //--庫房名稱
        public string FRWH { get; set; }        //--轉出庫房
        public string TOWH { get; set; }        //--轉入庫房
        public string MMCODE { get; set; }      //--院內碼
        public string MMNAME_E { get; set; }    //--藥品名稱
        public string APPQTY { get; set; }      //--數量
        public string BASE_UNIT { get; set; }   //--計量單位
        public string AMT { get; set; }         //--金額
        //public string '減帳' { get; set; }
        public string AD_FLAG { get; set; }     //--加減帳 
        public string UPDATE_TIME { get; set; } //--處理日期/時間
        public string UPDATE_USER { get; set; } //--處理人
        //public string '軍品調帳', { get; set; }   //--調帳類別
        public string APPLY_NOTE { get; set; }  //--備註

        // 
        public string ADJ_TYPE { get; set; }    //--調帳類別
    }
}