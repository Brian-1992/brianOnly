using System;
using System.Collections.Generic;

namespace WebApp.Models.F
{
    public class FA0017 : JCLib.Mvc.BaseModel
    {
        public string YYYMM { get; set; }         
        public string MAT_CLASS { get; set; }  
        public string M_CONTID { get; set; }      
        public string AGEN_NO { get; set; }       
        public string M_DISCPERC { get; set; }      
        public string AGEN_NAMEC { get; set; }     
        public string AGEN_ACC { get; set; }   
        public string UNI_NO { get; set; }        
        public double FULLSUM { get; set; }     
        public double PAYSUM { get; set; }        
        public double DISCSUM { get; set; }     
        public double TXFEE { get; set; }               
        public string AGEN_BANK { get; set; }               
        public string AGEN_SUB { get; set; }               
        public double PAY { get; set; }
    }
}

