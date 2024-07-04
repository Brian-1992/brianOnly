using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BD0008 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼
        public string MMNAME_E { get; set; } //英文品名
        public string QTY { get; set; }
        public string TOT { get; set; }
        public string CONTRACNO { get; set; }
        
    }
}
