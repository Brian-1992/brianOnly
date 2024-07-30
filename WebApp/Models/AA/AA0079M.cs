using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0079M
    {
        public string DOCTYPE { get; set; }     // 異動類別
        public string POST_TIME { get; set; }
        public string TOWH { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string FRWH { get; set; }
        public string BASE_UNIT { get; set; }
        public string APPQTY { get; set; }
        public string APPID { get; set; }
        public string AD { get; set; }          // 出入別
        public float SUM { get; set; }
        public List<AA0079M> ITEMS { get; set; }
    }
}