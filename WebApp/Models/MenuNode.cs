using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TSGH.Models
{
    public class MenuNode
    {
        public string id { get; set; }
        public string text { get; set; }
        public string url { get; set; }
        public string iconCls { get; set; }
        public bool expanded { get; set; }
        public bool leaf { get; set; }
        public MenuNode[] children { get; set; }
    }
}