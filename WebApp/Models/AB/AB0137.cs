using System;
using System.Collections.Generic;

namespace WebApp.Models.AB
{
    public class AB0137 : JCLib.Mvc.BaseModel
    {
        public string USEDATE { get; set; }//消耗日期
        public string STOCKCODE { get; set; }//扣庫藥局
        public string MMCODE { get; set; }//院內碼
        public string USEQTY { get; set; }//消耗量
        public string BASE_UNIT { get; set; }//計量單位代碼
        public string MMNAME_E { get; set; }//英文品名

        public string MMNAME_C { get; set; }//中文品名
        public string CREATE_TIME { get; set; }//匯入時間
        public string MAT_CLASS { get; set; }

        public int? Seq { get; set; }
        public string CHECK_RESULT { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        
            



    }
}
