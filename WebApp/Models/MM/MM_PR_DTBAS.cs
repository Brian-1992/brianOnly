using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MM_PR_DTBAS : JCLib.Mvc.BaseModel
    {
        public string MAT_CLSID { get; set; }
        public string M_STOREID { get; set; }
        public string DATEBAS { get; set; }
        public string BEGINDATE { get; set; }
        public string ENDDATE { get; set; }
        public string SUMDATE { get; set; }
        public string MTHBAS { get; set; }
        public string LASTDELI_MTH { get; set; }
        public string LASTDELI_DT { get; set; }
        public string MMPRDTBAS_SEQ { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}