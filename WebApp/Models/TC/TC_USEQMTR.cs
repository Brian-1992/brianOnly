using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TC_USEQMTR
    {
        public string DATA_YM { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MUSE_QTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string CREATE_TIME { get; set; }

        public IEnumerable<TC_USEQMTR> ITEMS { get; set; }
    }
}