using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CallHisApiData
    {
        // for AB0012a
        // 
        public string StartDateTime { get; set; }
        // 
        public string EndDateTime { get; set; }
        // 
        public string OrderCode { get; set; }
        // 
        public string StockCode { get; set; }
        public string ParTest { get; set; } //測試用參數

        public CallHisApiData()
        { }
    }
}