using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_UDI_LOG
    {
        public string LOG_TIME { get; set; }    // 讀取時間
        public string WMMID { get; set; }       // 資材碼
        public string WMCMPY { get; set; }      // 公司
        public string WMWHS { get; set; }       // 倉庫
        public string WMORG { get; set; }       // 成本中心
        public string CRVMPY { get; set; }      // 供應商料號1
        public string CRITM { get; set; }       // 供應商料號2
        public string WMREFCODE { get; set; }   // 供應商料號3
        public string WMBOX { get; set; }       // 材料盒
        public string WMLOC { get; set; }       // 儲位
        public string WMSRV { get; set; }       // 技術碼
        public string WMSKU { get; set; }       // 規格碼
        public string WMMIDNAME { get; set; }   // 院內俗品
        public string WMMIDNAMEH { get; set; }  // 院內品名
        public string WMSKUSPEC { get; set; }   // 規格
        public string WMBRAND { get; set; }     // 廠牌
        public string WMMDL { get; set; }       // 型號
        public string WMMIDCTG { get; set; }    // 類別
        public string WMEFFCDATE { get; set; }  // 效期
        public string WMLOT { get; set; }       // 批號
        public string WMSENO { get; set; }      // 序號
        public string WMPAK { get; set; }       // 包裝名稱
        public string WMQY { get; set; }        // 包裝量
        public string THISBARCODE { get; set; } // 本次讀取的條碼
        public string UDIBARCODES { get; set; } // 該單項累計讀取的UDI條碼用 該單項累計讀取的UDI條碼用<;>做分隔
        public string GTINSTRING { get; set; }  // 該單項累計讀取的GTIN 該單項累計讀取的GTIN 用<;>做分隔
        public string NHIBARCODE { get; set; }  // 傳回健保局要的條碼形態GS1用 傳回健保局要的條碼形態GS1用()號
        public string NHIBARCODES { get; set; } // 該單項累計健保局要的條碼形態 該單項累計健保局要的條碼形態<;>做分隔
        public string BARCODETYPE { get; set; } // 解碼後填入條碼別 解碼後填入條碼別UDI/AHOP/SKU/BOX/LOC/SRV
        public string GTININSTRING { get; set; }    // 解碼取得GTIN_IN字串 
        public string RESULT { get; set; }      // 結果
        public string ERRMSG { get; set; }      // 錯誤訊息

    }
}