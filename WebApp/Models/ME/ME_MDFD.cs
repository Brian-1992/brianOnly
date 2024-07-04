using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_MDFD : JCLib.Mvc.BaseModel
    {
        public string MDFM { get; set; }
        public string MMCODE { get; set; }
        public string MDFD_QTY { get; set; }
        public string MDFD_UNIT { get; set; }
        public string USE_QTY { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_ID { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string UPDATE_IP { get; set; }
        public string MMCODE_DETAIL { get; set; }
        //==========
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        //public string MMCODE { get; set; }
        public string APPQTY { get; set; }
        public string APVQTY { get; set; }
        public string APVTIME { get; set; }
        public string APVID { get; set; }
        public string ACKQTY { get; set; }
        public string ACKID { get; set; }
        public string ACKTIME { get; set; }
        public string STAT { get; set; }
        public string RSEQ { get; set; }
        public string EXPT_DISTQTY { get; set; }
        public string DIS_USER { get; set; }
        public string DIS_TIME { get; set; }
        public string BW_MQTY { get; set; }
        public string BW_SQTY { get; set; }
        public string PICK_QTY { get; set; }
        public string PICK_USER { get; set; }
        public string PICK_TIME { get; set; }
        public string ONWAY_QTY { get; set; }
        public string APLY_CONTIME { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public string AMT { get; set; }
        public string UP { get; set; }
        //public string CREATE_TIME { get; set; }
        //public string CREATE_USER { get; set; }
        //public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        //public string UPDATE_IP { get; set; }

        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string AVG_PRICE { get; set; }
        public string INV_QTY { get; set; }
        public string AVG_APLQTY { get; set; }
        public string ONWAYQTY { get; set; }
        public string AFTERQTY { get; set; }
        public string SAFE_QTY { get; set; }
        public string ACKTIME_T { get; set; }
        public string STAT_N { get; set; }
        public string A_INV_QTY { get; set; }
        public string B_INV_QTY { get; set; }
        public string M_DISCPERC { get; set; }

        // =================== 湘倫 ======================
        public string INV_QTY_FR { get; set; }
        public string INV_QTY_TO { get; set; }
        // ===============================================


        // =================== 家瑋 ======================
        public string AMOUNT { get; set; }
        public string WH_NO { get; set; }
        // ===============================================
    }
}
