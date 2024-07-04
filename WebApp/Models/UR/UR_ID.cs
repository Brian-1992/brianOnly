using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UR_ID : JCLib.Mvc.BaseModel
    {
        public string TUSER { get; set; }
        //public string DEPT_NO { get; set; }
        public string INID { get; set; }
        public string PA { get; set; }
        public string SL { get; set; }
        public string UNA { get; set; }
        public string IDDESC { get; set; }
        public string EMAIL { get; set; }
        public string EXT { get; set; }
        public string BOSS { get; set; }
        public string TITLE { get; set; }
        public string FAX { get; set; }
        public Nullable<decimal> FL { get; set; }
        public string TEL { get; set; }
        public string ADUSER { get; set; }
        public string INID_NAME { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string WHITELIST_IP1 { get; set; }
        public string WHITELIST_IP2 { get; set; }
        public string WHITELIST_IP3 { get; set; }
        public string EM_EMPNAME { get; set; }
        public string EM_DEPTNO { get; set; }
    }
}