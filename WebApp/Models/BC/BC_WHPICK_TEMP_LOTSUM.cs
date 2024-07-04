using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_WHPICK_TEMP_LOTSUM : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string CALC_TIME { get; set; }
        public string LOT_NO { get; set; }
        public string COMPLEXITY_SUM { get; set; }
        public string DOCNO_SUM { get; set; }
        public string APPITEM_SUM { get; set; }
        public string CREATE_USER { get; set; }
        public string CREATE_DATE { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string CALC_TIME_TEST_String { get; set; }
        public DateTime CALC_TIME_TEST_DateTime { get; set; }

        public string DOCNOS_0X { get; set; }
        public string DOCNOS_02 { get; set; }
        public string MATCLS_GRP { get; set; }
        public string DOCNO { get; set; }
    }
}