using System;
using System.Collections.Generic;

namespace WebApp.Models.F
{
    public class FA0030 : JCLib.Mvc.BaseModel
    {
        public string APPDEPT { get; set; } //成本碼
        public string APPDEPT_NAME { get; set; } //單位名稱
        public string SUM_APV_VOLUME { get; set; } //總材積
    }
}