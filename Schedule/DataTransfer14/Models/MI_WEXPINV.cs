using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer14
{
    public class MI_WEXPINV
    {
        public string WH_NO { get; set; } // 庫房代碼
        public string MMCODE { get; set; } // 院內碼
        public string EXP_DATE { get; set; } // 效期
        public string LOT_NO { get; set; } // 批號
        public string INV_QTY { get; set; } // 庫存數量
        public string CREATE_USER { get; set; } // 建立人員代碼
        public string UPDATE_TIME { get; set; } // 異動時間
        public string UPDATE_USER { get; set; } // 異動人員代碼
        public string UPDATE_IP { get; set; } //	 異動IP
        public string CREATE_TIME { get; set; } // 建立時間
        public string DOCNO { get; set; } //	單據號碼(申請單號)

        public string STRDEPID { get; set; }
        public string STRDRUGID { get; set; }
        public string STRDATELIMIT { get; set; }
        public string STRMADEBATCHNO { get; set; }
        public string LNGDRUGAMT { get; set; }

        public string DIFF { get; set; } //庫存差異值(校正用)
    }
}
