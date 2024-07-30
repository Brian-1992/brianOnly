using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CHK_DETAIL_TEMP : JCLib.Mvc.BaseModel
    {
        public string CHK_NO { get; set; }
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string INV_QTY { get; set; }
        public string CHK_QTY { get; set; }
        public string STORE_LOC { get; set; }
        public string STORE_LOC_NAME { get; set; }
        public string CHK_UID { get; set; }
        public string UPLOAD_DATE { get; set; }
        public string CHK_TIME { get; set; }

        public string CHK_UID_NAME { get; set; }

        public string STATUS_INI { get; set; }
        public string STATUS_INI_NAME { get; set; }

        public int? Seq { get; set; }
        public string saveStatus { get; set; }  // 1: 新增 2:更新
        public string STORE_QTYC { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }


        public CHK_DETAIL_TEMP() {
            Seq = 0;
        }
    }
}