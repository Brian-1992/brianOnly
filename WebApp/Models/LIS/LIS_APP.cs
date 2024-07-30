using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class LIS_APP : JCLib.Mvc.BaseModel
    {
        public string PURCHNO { get; set; }
        public string MMCODE { get; set; }
        public string APPQTY { get; set; }
        public string BASE_UNIT { get; set;}
        public string APPUSR { get; set; }
        public string APPTIME { get; set; }
        public string APPLY_NOTE { get; set; }
        public string INSTIME { get; set; }
        public string RDTIME { get; set; }
        public string DOCNO { get; set; }
        public string REJ_NOTE { get; set; }
    }
}