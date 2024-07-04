using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0059M : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string ITEM_STRING { get; internal set; }
        public string WH_KIND { get; set; }
        public string WH_GRADE { get; set; }
        public string WH_NAME { get; set; }
        public string MMNAME_E { get; set; }
        public string MMNAME_C { get; set; }
        public string E_DRUGCLASS { get; set; }
        public string E_DRUGCLASSIFY { get; set; }
        public string E_SOURCECODE { get; set; }
        public string E_DRUGAPLTYPE { get; set; }
        public string E_MANUFACT { get; set; }
        public string WH_GRADE_D { get; set; }
        public string WH_KIND_D { get; set; }
        public string E_DRUGCLASS_D { get; set; }
        public string E_DRUGCLASSIFY_D { get; set; }
        public string E_SOURCECODE_D { get; set; }
        public string E_DRUGAPLTYPE_D { get; set; }
        public string M_AGENLAB { get; set; }
    }
}