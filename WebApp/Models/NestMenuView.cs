using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class NestMenuView
    {
        public MenuView item { get; set; }
        public IEnumerable<NestMenuView> child { get; set; }

    }
}