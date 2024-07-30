using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CHK_MAST : JCLib.Mvc.BaseModel
    {
        public string CHK_NO { get; set; }
        public string CHK_YM { get; set; }
        public string CHK_WH_NO { get; set; }
        public string CHK_WH_GRADE { get; set; }
        public string CHK_WH_KIND { get; set; }
        public string CHK_PERIOD { get; set; }
        public string CHK_TYPE { get; set; }
        public string CHK_LEVEL { get; set; }
        public string CHK_NUM { get; set; }
        public string CHK_TOTAL { get; set; }
        public string CHK_STATUS { get; set; }
        public string CHK_KEEPER { get; set; }
        public string CHK_NO1 { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string WH_NAME { get; set; }
        public string WH_GRADE_NAME { get; set; }
        public string WH_KIND_NAME { get; set; }
        public string CHK_PERIOD_NAME { get; set; }
        public string CHK_TYPE_NAME { get; set; }
        public string CHK_LEVEL_NAME { get; set; }
        public string CHK_STATUS_NAME { get; set; }
        public string CHK_KEEPER_NAME { get; set; }

        public string ITEM_STRING { get; set; }

        public string QTY { get; set; }
        public string CHK_YMD { get; set; }     // 盤點日期

        public string CHK_CLASS { get; set; }
        public string CHK_CLASS_NAME { get; set; }

        public string FINAL_LEVEL { get; set; }
        public string ING_LEVEL { get; set; }
        public string IS_BATCH { get; set; }

        public string CHK_UID_NAMES { get; set; }

        //------CE0040顯示尚未盤點項目------//
        public string MMCODE { get; set; }

        public string MMNAME { get; set; }
    }
}