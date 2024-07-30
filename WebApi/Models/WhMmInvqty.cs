using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class WhMmInvqty
    {
        public string WH_NO { get; set; }            // 庫房代碼
        public string WH_NAME { get; set; }         // 庫房名稱
        public string MMCODE { get; set; }          // 院內碼
        public string TOTAL_INV_QTY { get; set; }   // 總庫存量
        public string LOW_QTY { get; set; }         // 最低庫存量
        public string SAFE_DAY { get; set; }        // 安全存量天數
        public string SAFE_QTY { get; set; }        // 安全庫存量
                                                    // 基準天數
        public string HIGH_QTY { get; set; }        // 基準量
        public string MIN_ORDQTY { get; set; }      // 最小包裝量
        public string PARENTSTOCKCODE { get; set; }     // 上級庫
        public string RESERVEFLAG { get; set; }         // 急救車用
        public string NOWCONSUMEFLAG { get; set; }  // 撥發即消耗

        public IEnumerable<string> MMCODES { get; set; }    //院內碼清單


        public IEnumerable<LotExpInv> LOT_EXP_INV { get; set; }

        public string STARTDATE { get; set; }
        public string ENDDATE { get; set; }

        public string INID { get; set; }
        public string WH_KIND { get; set; }

        public WhMmInvqty(){
            WH_NO = string.Empty;
            MMCODE = string.Empty;
            TOTAL_INV_QTY = string.Empty;
            LOW_QTY = string.Empty;
            SAFE_DAY = string.Empty;
            SAFE_QTY = string.Empty;
            HIGH_QTY = string.Empty;
            MIN_ORDQTY = string.Empty;
            PARENTSTOCKCODE = string.Empty;
            RESERVEFLAG = string.Empty;
            NOWCONSUMEFLAG = string.Empty;
            MMCODES = new List<string>();
            LOT_EXP_INV = new List<LotExpInv>();
            STARTDATE = string.Empty;
            ENDDATE = string.Empty;
            INID = string.Empty;
        }
    }

    public class LotExpInv {
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string INV_QTY { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAME { get; set; }
    }
}