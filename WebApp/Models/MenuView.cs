using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MenuView
    {
        public string FG { get; set; }
        public string PG { get; set; }
        public string FS { get; set; }
        public string FD { get; set; }
        public string V { get; set; }
        public string R { get; set; }
        public string U { get; set; }
        public string P { get; set; }
        public string HV { get; set; }
        public string HR { get; set; }
        public string HU { get; set; }
        public string HP { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string text { get; set; }
        //public string url { get; set; }
        public string iconCls { get; set; }
        public bool expanded { get; set; }
        public bool leaf { get; set; }
        public IEnumerable<MenuView> children { get; set; }
    }
}