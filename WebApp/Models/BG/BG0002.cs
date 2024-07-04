////using System;
////using System.Collections.Generic;

//using System;
//using JCLib.DB;
//using Dapper;
//using WebApp.Models;
//using System.Collections.Generic;
//using System.Text;
//using JCLib.Mvc;
//using Oracle.ManagedDataAccess.Client;
//using Oracle.ManagedDataAccess.Types;
//using System.Data;

//using System.Linq;
//using System.Web;

using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BG0002 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; } // 00.庫房代碼
        public string MMCODE { get; set; } // 01.院內碼
        public string APL_INQTY { get; set; } // 02.本期增加(入庫總量)
        public string DATA_YM { get; set; } // 03.年月
        public string DATA_YM_START { get; set; } // 03.年月
        public string DATA_YM_END { get; set; } // 03.年月
        public string MMNAME_C { get; set; } // 04.中文品名
        public string MMNAME_E { get; set; } // 05.英文品名
        public string MAT_CLASS { get; set; } // 06.物料分類代碼
        public string MAT_CLASS_NAME { get; set; } // 06.物料分類名稱
        public string BASE_UNIT { get; set; } // 07.計量單位代碼
        public string M_STOREID { get; set; } // 08.庫備識別碼
        public string M_CONTID { get; set; } // 09.合約識別碼
        public string M_APPLYID { get; set; } // 10.申請申購識別碼
        public string DISC_UPRICE { get; set; } // 11.優惠最小單價
        public string WH_NAME { get; set; } // 12.庫房名稱
        public string WH_KIND { get; set; } // 13.庫別分類
        public string WH_GRADE { get; set; } // 14.庫別級別
        public string MAT_CLSNAME { get; set; } // 15.物料分類名稱
        public string MAT_CLSID { get; set; } // 16.物料分類屬性
        public string TOT { get; set; } // 17.總價

        // -- 延伸 -- 
        public string ROWNUMBERER { get; set; } // 項次
        public string RADIO_BUTTON { get; set; } // 16.物料分類屬性  0-庫備品、1-非庫備品(排除鎖E品項) 、2-庫備品(管控項目)。        
        public string ACCTOT { get; set; } // 累計進貨金額


        // -- excel
        public string 項次 { get; set; } // 
        public string 院內碼 { get; set; } // 
        public string 英文品名 { get; set; } // 
        public string ACCT中文品名OT { get; set; } // 
        public string 計量單位 { get; set; } // 
        public string 年月 { get; set; } // 
        public string 本期增加 { get; set; } // 
        public string 單價 { get; set; } // 
        public string 總價 { get; set; } // 
        public string 累積進貨金額 { get; set; } // 

    }
}