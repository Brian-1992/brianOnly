using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CD0005MD : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string PICK_DATE { get; set; }
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string MAT_CLASS { get; set; }
        public string ACT_PICK_USERID { get; set; }
        public string ACT_PICK_QTY { get; set; }
        public string ACT_PICK_TIME { get; set; }
    }
}
