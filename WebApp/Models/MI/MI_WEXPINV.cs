using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_WEXPINV : JCLib.Mvc.BaseModel
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

        public string STATUS_DISPLAY { get; set; } //使用代碼(紅底顯示)


    }
}