using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class PH_BANK_AF : JCLib.Mvc.BaseModel
    {
        public string AGEN_BANK_14 { get; set; }
        public string BANKNAME { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string CHECK_RESULT { get; set; } //檔案檢核用
    }
}