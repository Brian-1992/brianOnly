﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BASEUNITCNV : JCLib.Mvc.BaseModel
    {
        public string UI_FROM { get; set; }
        public string UI_TO { get; set; }
        public string COEFF_FROM { get; set; }
        public string COEFF_TO { get; set; }
        public string CNVNOTE { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string TRANS_VALUE { get; set; }



    }
}