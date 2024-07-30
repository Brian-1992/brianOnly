using System;
using System.Collections.Generic;

namespace WebApp.Models.AB
{
    public class AB0046 : JCLib.Mvc.BaseModel
    {
        public string VISITKIND { get; set; }      // 門住診別 (0.不分,1.住院,2.門診,3.急診)         
        public string LOCATION { get; set; }       // 動向碼 (診間/病房 CHAR)         
        public string FREQNO { get; set; }         // 院內頻率       
        public string BEGINTIME { get; set; }      // 起始時間 (例行時間起 CHAR)        
        public string ENDTIME { get; set; }        // 迄止時間 (例行時間迄 CHAR)     
        public string DEFAULTSTOCKCODE { get; set; }  // 預設扣庫別(優先扣庫 CHAR) 
        public string ROUTINESTOCKCODE { get; set; }  // 例行庫扣庫別        
        public string EXCEPTSTOCKCODE { get; set; }   // 非例行庫扣庫別    
        public string TAKEOUTSTOCKCODE { get; set; }  // 出院帶藥扣庫別  
        public string TPNSTOCKCODE { get; set; }      // TPN扣庫別     
        public string PCASTOCKCODE { get; set; }      // PCA扣庫別    
        public string CHEMOSTOCKCODE { get; set; }    // CHEMO扣庫別 (化療扣庫 CHAR)
        public string RESEARCHSTOCKCODE { get; set; } // 研究用藥扣庫別
        public string RETURNSTOCKCODE { get; set; }   // 退藥庫別 
        public string CREATEDATETIME { get; set; }    // 記錄建立日期/時間 
        public string CREATEOPID { get; set; }        // 記錄建立人員 
        public string PROCDATETIME { get; set; }      // 記錄處理日期/時間
        public string PROCOPID { get; set; }          // 記錄處理人員
        public string TEXT { get; set; }              // return SQL combo TEXT
        public string VALUE { get; set; }             // return SQL combo VALUE

    }
}