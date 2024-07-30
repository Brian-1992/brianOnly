using System;
using System.Collections.Generic;

namespace WebApp.Models.AA
{
    public class AA0175 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public int INV_QTY { get; set; }
        public string LOC_NOTE { get; set; }
        public string STORE_LOC { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_USER { get; set; }
        public string SaveStatus { get; set; }
        public string UploadMsg { get; set; }
        public string ORI_STORE_LOC { get; set; }
    }
}