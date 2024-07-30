using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BC_STLOC : JCLib.Mvc.BaseModel
    {
        public string STORE_LOC { get; set; }
        public string WH_NO { get; set; }
        public string BARCODE { get; set; }
        public string XCATEGORY { get; set; }
        public string FLAG { get; set; }
        public string MEMO { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_USER { get; set; }
        public string OLD_WH_NO { get; set; }
        public string OLD_STORE_LOC { get; set; }
        public string BARCODE_IMAGE_STR { get; set; }
    }
}