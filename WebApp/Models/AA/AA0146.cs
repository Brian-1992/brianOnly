using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.AA
{
    public class AA0146 : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string CONTRACNO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string E_SCIENTIFICNAME { get; set; }
        public string E_MANUFACT { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string INV_QTY { get; set; }
        public string TOTAL { get; set; }
        public string AGEN_NAME { get; set; }
        public string WResQty { get; set; }
        public string NOTE { get; set; }

        public string FL_NAME { get; set; }
        public string SEQ_NO { get; set; }
        public string NOB_NO { get; set; }
        public string MAT_NAME { get; set; }
        public string MMNAME { get; set; }
        public string E_SPECNUNIT { get; set; }
        public string E_DRUGFORM { get; set; }
        public string WRESQTY { get; set; }
        public string TRANSQTY { get; set; }
        public string PUR_MMCODE { get; set; }
        public string WRES_MMCODE { get; set; }        
        public string MMNAME_C { get; set; }        
        public string E_ITEMARMYNO { get; set; }        
        public string DISC_CPRICE { get; set; }
        public string PUR_QTY { get; set; }
        public string PUR_AMT { get; set; }
        public string M_STOREID { get; set; }
        public string CreateUser { get; set; }                  
        public string UpdateIp { get; set; }

        public int? Seq { get; set; }
        public string UploadMsg { get; set; }
        public string SaveStatus { get; set; }
        public AA0146()
        {
            Seq = 0;
        }
    }
}