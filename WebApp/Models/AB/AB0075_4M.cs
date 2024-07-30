using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AB0075_4M : JCLib.Mvc.BaseModel
    {
        public string VISITDATE { get; set; } //日期
        public string CNTVISITSEQ { get; set; } //人數
        public string CNTORDERNO { get; set; } //藥品筆數
    }
}