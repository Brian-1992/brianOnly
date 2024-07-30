using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0067 : JCLib.Mvc.BaseModel
    {
        public string NRCode { get; set; }
        public string BedNo { get; set; }
        public string MedNo { get; set; }
        public string ChinName { get; set; }
        public string acktime { get; set; }
        public string mmname_e { get; set; }
        public double appqty { get; set; }
        public double apvqty { get; set; }
        public string GTAPL_RESON { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public double AVG_PRICE { get; set; }
        public double MONEY { get; set; }
        public string FRWH_D { get; set; }
        public string ConfirmSwitch { get; set; }
        public string APPID { get; set; }
        public string CREATE_TIME { get; set; }
        public string UPDATE_TIME { get; set; }
    }
}

