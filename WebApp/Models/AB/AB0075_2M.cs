using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AB0075_2M : JCLib.Mvc.BaseModel
    {
        public string PORC_DATE { get; set; } //日期
        public string PAT_CNT { get; set; } //病人數
        public string TOT_CNT { get; set; } //藥品筆數
    }
}