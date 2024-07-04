using System;
using System.Collections.Generic;

namespace WebApp.Models.F
{
    public class FA0026 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }         
        public string MMNAME_C { get; set; }  
        public string MMNAME_E { get; set; }      
        public string BASE_UNIT { get; set; }       
        public string M_PURUN { get; set; }      
        public double UPRICE { get; set; }     
        public double M_CONTPRICE { get; set; }   
        public string EXCH_RATIO { get; set; }        
        public string M_AGENNO { get; set; }     
        public string AGEN_NAMEC { get; set; }        
        public string M_STOREID { get; set; }     
        public string M_CONTID { get; set; }               
        public string M_MATID { get; set; }               
        public double DISC_UPRICE { get; set; }               
        public double DISC_CPRICE { get; set; }
        public string M_APPLYID { get; set; }
        public string M_VOLL { get; set; }
        public string M_VOLW { get; set; }
        public string M_VOLH { get; set; }
        public string M_VOLC { get; set; }
        public string M_SWAP { get; set; }
        public string M_PHCTNCO { get; set; }
    }
}

