using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.C
{
    public class CB0011 : JCLib.Mvc.BaseModel
    {
        public String NO { get; set; }
        public String BOXNO { get; set; }
        public String BARCODE { get; set; }
        public String XCATEGORY { get; set; }
        public String DESCRIPT { get; set; }
        public String STATUS { get; set; }
        public String CREATE_USER { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public String UPDATE_USER { get; set; }
        public String UPDATE_IP { get; set; }
        public string BARCODE_IMAGE_STR { get; set; }
    }
}