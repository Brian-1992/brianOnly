using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_CONSUME_DATE : JCLib.Mvc.BaseModel
    {
        public string DATA_DATE { get; set; }
        public string DATA_BTIME { get; set; }
        public string DATA_ETIME { get; set; }
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string VISIT_KIND { get; set; }
        public string CONSUME_QTY { get; set; }
        public string STOCK_UNIT { get; set; }
        public string INSU_QTY { get; set; }
        public string HOSP_QTY { get; set; }
        public string PARENT_ORDERCODE { get; set; }
        public string PARENT_CONSUME_QTY { get; set; }
        public string CREATEDATETIME { get; set; }
        public string PROC_ID { get; set; }
        public string PROC_MSG { get; set; }
        public string PROC_TYPE { get; set; }


        public string WH_NAME { get; set; }
        public string MMNAME_E { get; set; }


    }
}