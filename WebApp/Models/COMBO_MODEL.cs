using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class COMBO_MODEL : JCLib.Mvc.BaseModel
    {
        public string VALUE { get; set; }
        public string TEXT { get; set; }
        public string COMBITEM { get; set; }
        public string EXTRA1 { get; set; }
        public string EXTRA2 { get; set; }
        public string HOSP_CODE { get; set; } //醫院代碼
    }
}