using System;
using System.Collections.Generic;

namespace WebApp.Models.AB
{
    public class AB0132 : JCLib.Mvc.BaseModel
    {
        public string DATA_DATE { get; set; }//日期
        public string ORDERCODE { get; set; }//藥品代碼
        public string MMNAME_C { get; set; }//藥品名稱
        public string ORDERDR { get; set; }//開立醫師
        public string MEDNO { get; set; }//病歷號碼
        public string SECTIONNO { get; set; }//科別代碼
        public string SECTIONNAME { get; set; }//科別名稱
        public string DOSE { get; set; }//開立劑量
        public string SUMQTY { get; set; }//總量
        public string CREATEOPID { get; set; }//建立人員
        public string CREATEDATETIME { get; set; }//建立日期時間
        public string ORDERUNIT { get; set; }//單位
        public string ATTACHUNIT { get; set; }//劑型單位
        public string VISIT_KIND { get; set; }//門急住診別
        public string DET_COST_14 { get; set; }//成本
        public string DET_DEPTCENTER_14 { get; set; }//成本中心部門
        public string DET_NRCODE_14 { get; set; }//護理站代碼
        public string DET_BEDNO_14 { get; set; }//床位號
        public string STOCKCODE { get; set; }//扣庫地點
    }
}
