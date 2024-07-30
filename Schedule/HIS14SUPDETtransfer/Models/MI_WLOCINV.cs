using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIS14SUPDETtransfer
{
    public class MI_WLOCINV
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string STORE_LOC { get; set; }
        public string INV_QTY { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string LOC_NOTE { get; set; }
        public string WH_NAME { get; set; }     //庫別名稱
        public string MMNAME_C { get; set; }    //中文品名
        public string MMNAME_E { get; set; }    //英文品名
        public string WH_NAME_DISPLAY { get; set; } //庫房代碼(白色顯示用)
        public string WH_NAME_TEXT { get; set; } //庫房代碼(紅色顯示用)
        public string MMCODE_DISPLAY { get; set; }  //院內碼(白色顯示用)
        public string MMCODE_TEXT { get; set; }     //院內碼(紅色顯示用)
        public string STORE_LOC_DISPLAY { get; set; } //儲位代碼別名(白色顯示用)
        public string STORE_LOC_TEXT { get; set; } //儲位代碼別名(紅色顯示用)
        public string LOC_NOTE_TEXT { get; set; }
        public string CHECK_RESULT { get; set; } //檢核結果
        public string IMPORT_RESULT { get; set; } //異動結果


        public string STRDEPID { get; set; }
        public string STRDRUGID { get; set; }
        public string STRSTOREPOS { get; set; }
        public string LNGDRUGAMT { get; set; }
    }
}
