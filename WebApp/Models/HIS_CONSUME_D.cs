using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class HIS_CONSUME_D : JCLib.Mvc.BaseModel
    {
        public string DATA_DATE { get; set; }
        public string DATA_BTIME { get; set; }
        public string DATA_ETIME { get; set; }
        public string STOCKCODE { get; set; }
        public string WH_NAME { get; set; }
        public string ORDERCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string VISIT_KIND { get; set; }
        public string STOCKFLAG { get; set; }
        public string ORDERDR { get; set; }
        public string MEDNO { get; set; }
        public string VISITDATE { get; set; }
        public string RID { get; set; }
        public string WORKDATE { get; set; }
        public string WORKTIME { get; set; }
        public string MODIFYDATE { get; set; }
        public string MODIFYTIME { get; set; }
        public string CANCELDATETIME { get; set; }
        public string SECTIONNO { get; set; }
        public string VSDR { get; set; }
        public string INSULOOKSEQ { get; set; }
        public string DOSE { get; set; }
        public string FREQNO { get; set; }
        public string PATHNO { get; set; }
        public string DAYS { get; set; }
        public string SUMQTY { get; set; }
        public string EMGFLAG { get; set; }
        public string REGION { get; set; }
        public string PAYFLAG { get; set; }
        public string COMPUTECODE { get; set; }
        public string ADDRATIO1 { get; set; }
        public string ADDRATIO2 { get; set; }
        public string ADDRATIO3 { get; set; }
        public string INSUCHARGEID { get; set; }
        public string HOSPCHARGEID { get; set; }
        public string INSUAMOUNT { get; set; }
        public string PAYAMOUNT { get; set; }
        public string CANCELFLAG { get; set; }
        public string CANCELOPID { get; set; }
        public string PROCOPID { get; set; }
        public string PROCDATETIME { get; set; }
        public string CREATEOPID { get; set; }
        public string CREATEDATETIME { get; set; }
        public string DRUGNO { get; set; }
        public string SLOWCARD { get; set; }
        public string BAGSEQNO { get; set; }
        public string SENDDATE { get; set; }
        public string TOTALQTY { get; set; }
        public string ORDERUNIT { get; set; }
        public string ATTACHUNIT { get; set; }
        public string ID { get; set; }
        public string CHARNO { get; set; }
        public string BATCHNUM { get; set; }
        public string EXPIREDDATE { get; set; }
        public string PARENT_ORDERCODE { get; set; }
        public string PARENT_CONSUME_QTY { get; set; }
    }
}