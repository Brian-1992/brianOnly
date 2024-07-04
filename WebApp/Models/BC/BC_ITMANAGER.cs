using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_ITMANAGER : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string MANAGERID { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_USER { get; set; }



        public string MAT_CLSNAME { get; set; }         //物料分類名稱
        public string MAT_CLASS { get; set; }           //物料分類代碼
        public string MMNAME_C { get; set; }            //中文品名
        public string MMNAME_E { get; set; }            //英文品名
        public string MANAGERNM { get; set; }           //管理者名稱
        public string USERID { get; set; }              //管理者名稱
        public string USER_NAME { get; set; }           //管理者名稱
        public string OPT { get; set; }                 //選項顯示名稱
        public string OLD_MANAGERID { get; set; }
         //public IEnumerable<BC_ITMANAGER> Items { get; set; }

        public string ITEM_STRING { get; set; }

        public string STORE_LOC { get; set; }
    }
}