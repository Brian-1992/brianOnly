using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AB0057M : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string EXP_DATE { get; set; }
        public string LOT_NO { get; set; }
        public string INV_QTY { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string WH_NAME { get; set; }         //庫別名稱
        public string MMNAME_C { get; set; }        //中文品名
        public string MMNAME_E { get; set; }        //英文品名
        public string MMCODE_DISPLAY { get; set; }  //品名(顯示用)
        public string WH_NAME_DISPLAY { get; set; } //庫別名(顯示用)
        public string MMCODE_TEXT { get; set; }     //品名(顯示用)
        public string ORI_EXP_DATE { get; set; }

        public string MMNAME { get; set; }
        public string MAT_CLASS { get; set; }
        public string BASE_UNIT { get; set; }
        public string FRWH { get; set; }
        public string M_AGENNO { get; set; }
        public string M_CONTPRICE { get; set; }        
        public string TITLE { get; set; }        
    }
}