using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.AA
{
    public class AA0140 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }                 //院內碼  
        public string SELF_CONT_BDATE { get; set; }        //藥品契約生效起日  
        public string SELF_CONT_EDATE { get; set; }        //藥品契約生效迄日  
        public string SELF_CONTRACT_NO { get; set; }       //合約案號  
        public string SELF_PUR_UPPER_LIMIT { get; set; }   //採購上限金額  
        public string CreateUser { get; set; }
        public string CreateTime { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateTime { get; set; }
        public string UpdateIp { get; set; }
        public string SELF_CONT_BDATE_virtual { get; set; } //藥品契約生效起日(虛擬) 
        public string SELF_CONT_EDATE_virtual { get; set; } //藥品契約生效迄日(虛擬)

        public int? Seq { get; set; }
        public string SaveStatus { get; set; }  // 1: 新增 2:更新
        public string UploadMsg { get; set; }
        public AA0140()
        {
            Seq = 0;
        }

    }
}