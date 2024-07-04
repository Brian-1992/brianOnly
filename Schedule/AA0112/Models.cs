using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace AA0112
{
    public class ChkWh
    {
        public string WH_NO { get; set; }
        public string WH_GRADE { get; set; }
        public int CHK_TOTAL { get; set; }
        public string INID { get; set; }
        public string WH_KIND { get; set; }
        public string CHK_YM { get; set; }
        public string WH_NAME { get; set; }
    }

    public class SelectType
    {
        public string CodeStart { get; set; }
        public string CodeEnd { get; set; }
    }

    public class CHK_MAST_EXTEND
    {
        public string CHK_NO1 { get; set; }
        public string MAX_CHK_LEVEL { get; set; }
        public string MAX_CHK_LEVEL_STATUS { get; set; }
        public CHK_MAST LAST_CHK_MAST { get; set; }
        public IEnumerable<CHK_MAST> masts { get; set; }
    }

    public class DGMISS_ITEM {
        public string APP_INID { get; set; }
        public string MMCODE { get; set; }
        public string INV_QTY { get; set; }
        public string APVQTY { get; set; }
        public string DATA_YM { get; set; }
        public string IP { get; set; }
    }

    public class CE0040 
    {
        public string CHK_YM { get; set; }
        public string CHK_NO { get; set; }
        public string CHK_WH_NO { get; set; }
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string MAT_CLASS { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string CHK_STATUS { get; set; }
        public string CHK_TOTAL { get; set; }
        public string CHK_NUM { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string STORE_QTYC { get; set; }
        public string CHK_QTY { get; set; }
        public string USE_QTY { get; set; }
        public string INVENTORY { get; set; }
        public string DIFF_AMOUNT { get; set; }
        public string CHK_TIME { get; set; }
        public string CHK_ENDTIME { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        //結束全院盤點
        public string CHK_WH_KIND { get; set; }
        public string CHK_WH_GRADE { get; set; }
        public string ALTERED_USE_QTY { get; set; }
        public string USE_QTY_AF_CHK { get; set; }
        public string INV_QTY { get; set; }
        public string SET_YM { get; set; }
    }

    public class CHK_EXPLOC {
        public string CHK_NO { get; set; }
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string EXP_DATE { get; set; }
        public string LOT_NO { get; set; }
        public string STORE_LOC {get;set;}
        public string INV_QTY { get; set; }
        public string ORI_INV_QTY { get; set; }
        public string USE_QTY { get; set; }
        public string IsExpired { get; set; }
        public string TRN_QTY { get; set; }
    }
}
