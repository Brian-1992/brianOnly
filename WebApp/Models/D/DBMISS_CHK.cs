using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.D
{
    public class DGMISS_CHK: JCLib.Mvc.BaseModel
    {
        public string DATA_YM { get; set; }
        public string INID { get; set; }
        public string MMCODE { get; set; }
        public string INV_QTY { get; set; }
        public string CHK_QTY { get; set; }
        public string CHK_UID { get; set; }
        public string CHK_TIME { get; set; }
        public string MEMO { get; set; }
        public string CHK_STATUS { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}