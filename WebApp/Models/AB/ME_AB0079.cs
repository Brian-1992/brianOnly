using System;
using System.Collections.Generic;

namespace WebApp.Models.AB
{
    public class ME_AB0079 : JCLib.Mvc.BaseModel
    {
        public string ORDERCODE { get; set; }      // 院內碼           
        public string ORDERENGNAME { get; set; }   // 英文名稱      
        public string CREATEYM { get; set; }       // 查詢月份       
        public string ORDERDR { get; set; }        // 醫師代碼       
        public string CHINNAME { get; set; }       // 醫師姓名     
        public string SECTIONNO { get; set; }      // 科室    
        public string SECTIONNAME { get; set; }    // 科室名       
        public string SUMQTY { get; set; }         // 醫囑(住)消耗量    
        public string SUMAMOUNT { get; set; }      // 醫囑(住)消耗金額  
        public string OPDQTY { get; set; }         // 醫囑(門)消耗量    
        public string OPDAMOUNT { get; set; }      // 醫囑(門)消耗金額  
        public string DSM { get; set; }            // D:醫師 S:科室 M:藥品

    }
}

