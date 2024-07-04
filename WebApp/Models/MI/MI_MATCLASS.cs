using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MI_MATCLASS : JCLib.Mvc.BaseModel
    {
        public string MAT_CLASS { get; set; }
        public string MAT_CLSNAME { get; set; }        
        public string MAT_CLSID { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}