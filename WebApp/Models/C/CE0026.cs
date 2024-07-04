using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.C
{
    public class CE0026 : JCLib.Mvc.BaseModel


        {
            public string WH_NO { get; set; }
            public string WH_NAME { get; set; }
            public string MMCODE { get; set; }
            public string MMNAME_C { get; set; }
            public string MMNAME_E { get; set; }
            public string BASE_UNIT { get; set; }
            public string STORE_QTY { get; set; }
            public string CHK_QTY { get; set; }
            public string STORE_COST { get; set; }
            public string CHK_COST { get; set; }
            public string diff_P { get; set; }
            public string DIFF_COST { get; set; }

        public string MEMO { get; set; }

        }

    }