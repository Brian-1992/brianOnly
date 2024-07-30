////using System;
////using System.Collections.Generic;

//using System;
//using JCLib.DB;
//using Dapper;
//using WebApp.Models;
//using System.Collections.Generic;
//using System.Text;
//using JCLib.Mvc;
//using Oracle.ManagedDataAccess.Client;
//using Oracle.ManagedDataAccess.Types;
//using System.Data;

//using System.Linq;
//using System.Web;

using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0092 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public int INV_QTY { get; set; }
        public string VACKIND { get; set; }
    }
}