using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AB0075_3M : JCLib.Mvc.BaseModel
    {
        public string SDATE { get; set; } //日期
        public string CNT { get; set; } //人數
        public string ORDERCODE { get; set; } //PCA類別
        public string SUMQTY { get; set; } //PCA類別筆數
    }
}