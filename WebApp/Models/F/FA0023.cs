using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0023 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }           //單位代碼,
        public string WH_NAME { get; set; }         //單位名稱,
        public string DATA_YM { get; set; }         //成本年月,
        public string MMCODE { get; set; }          //院內碼,
        public string MMNAME_C { get; set; }            //中文品名,
        public string MMNAME_E { get; set; }            //英文品名,
        public string BASE_UNIT { get; set; }           //計量單位,
        public string PMN_AVGPRICE { get; set; }            //期初單價,
        public string P_INV_QTY { get; set; }           //期初結存,
        public string SUM1 { get; set; }            //期初金額,
        public string IN_PRICE { get; set; }            //進貨單價,
        public string IN_QTY { get; set; }          //本月進貨,
        public string SUM2 { get; set; }            //進貨金額,
        public string AVG_PRICE { get; set; }           //消耗單價,本月單價,
        public string OUT_QTY { get; set; }         //本月消耗,
        public string SUM3 { get; set; }            //消耗金額,
        public string INV_QTY { get; set; }         //本月結存,
        public string SUMTOT { get; set; }          //結存金額,
        public string INVENTORYQTY { get; set; }            //盤盈虧數量,
        public string SUM4 { get; set; }			//盤盈虧金額,
        public string HIGH_QTY { get; set; }	    //基準量,
        public string SUB_QTY { get; set; }	        //超量,
        public string SUB_PRICE { get; set; }	    //超量金額,
        public string TURNOVER { get; set; }	    //週轉率,
        public string M_CONTID { get; set; }	    //小採否,
        public string M_STOREID { get; set; }	    //庫備否,
        public string M_PAYKIND { get; set; }	    //給付類別,
        public string M_CONSUMID { get; set; }	    //是否半消耗品,
        public string M_TRNID { get; set; }	    //是否扣庫,
        public string M_PAYID { get; set; }	    //是否計價,
        public string EXPLOT { get; set; }	    //效期,


        public string TRN_QTY { get; set; }
        public string ADJ_QTY { get; set; }
        public string BAK_QTY { get; set; }
        public string EXG_QTY { get; set; }
        public string MIL_QTY { get; set; }
        public string REJ_QTY { get; set; }
        public string DIS_QTY { get; set; }

        public string WH_TURNOVER { get; set; }

        public string CANCEL_ID { get; set; }
        public string IS_SOURCE_C { get; set; }
        public string WHNO_CANCEL { get; set; }
    }
}