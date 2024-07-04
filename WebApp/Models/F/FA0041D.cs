using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0041D : JCLib.Mvc.BaseModel
    {
        public string INID { get; set; }
        public string INID_NAME { get; set; }
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }


        public string CHK_YM { get; set; }  // 盤點年月
        public string UNIT_CLASS { get; set; }
        public string IS_CHK { get; set; }  // 盤點單位 (有:Y 未:N)
        public string IS_MED { get; set; }  // 含口服、非口服、管制藥 (有:Y 未:N)
        public string UNIT_CLASS_NAME { get; set; }

        public string CHK_PERIOD { get; set; }  //  盤點期
        public string CHK_TYPE { get; set; }    // 盤點類別
        public string CHK_STATUS { get; set; }  // 盤點單狀態
        public string CHK_LEVEL { get; set; }   //最後盤點階段
        public string CHK_NO { get; set; }  // 盤點單號

        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string STORE_QTY { get; set; }
        public string CHK_QTY { get; set; }
        public string GAP_T { get; set; }
    }
}