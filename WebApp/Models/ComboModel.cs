using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TSGH.Models
{
    public class ComboModel : JCLib.Mvc.BaseModel
    {
        public string KEY_CODE { get; set; }
        public string VALUE { get; set; }
        public string COMBITEM { get; set; }
    }
}