using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0139 : JCLib.Mvc.BaseModel
    {
        public string JBIDSEQ { get; set; } // 國軍藥品聯標決標品項異動記錄流水號
        public string TRANSCODE { get; set; } // 異動代碼
        public string JBID_STYR { get; set; } // 聯標生效起年
        public string JBID_EDYR { get; set; } // 聯標生效迄年
        public string BID_NO { get; set; } // 投標項次
        public string INGR { get; set; } // 招標成分
        public string INGR_CONTENT { get; set; } // 成分含量
        public string SPEC { get; set; } // 規格量
        public string DOSAGE_FORM { get; set; } // 劑型
        public string MMNAME_E { get; set; } // 英文品名
        public string MMNAME_C { get; set; } // 中文品名
        public string PACKQTY { get; set; } // 包裝
        public string ORIG_BRAND { get; set; } // 原廠牌
        public string LICENSE_NO { get; set; } // 許可證字號
        public string ISWILLING { get; set; } // 單次訂購達優惠數量折讓意願
        public string DISCOUNT_QTY { get; set; } // 單次採購優惠數量
        public string INSU_CODE { get; set; } // 健保代碼
        public string INSU_RATIO { get; set; } // 健保價(健保品項)/上月預算單價(非健保品項)
        public string K_UPRICE { get; set; } // 決標契約單價
        public string COST_UPRICE { get; set; } // 決標成本單價
        public string DISC_COST_UPRICE { get; set; } // 單次訂購達優惠數量成本價
        public string UNIFORM_NO { get; set; } // 廠商統編
        public string AGEN_NAME { get; set; } // 廠商名稱
        public string UPDATE_YMD { get; set; } // 修改年月日
        public string UPADTEUSER { get; set; } // 異動人員帳號
        public string UPDATETIME { get; set; } // 異動時間
        public string UPDATEIP { get; set; } // 異動IP
        public string CREATETIME { get; set; } // 建立時間
        public string MMCODELIST { get; set; }
    }
}