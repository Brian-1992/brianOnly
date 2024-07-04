using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.MI
{
    public class MI_MNSET : JCLib.Mvc.BaseModel
    {
        public string SET_YM { get; set; }

        public string SET_BTIME { get; set; }

        public string SET_ETIME { get; set; }

        public string SET_STATUS { get; set; }

        public string SET_MSG { get; set; }
        public string SET_CTIME { get; set; }
    }
}