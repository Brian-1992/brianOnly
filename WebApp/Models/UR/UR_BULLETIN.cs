using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class UR_BULLETIN : JCLib.Mvc.BaseModel
    {
        public string ID { get; set; }
        public string TITLE { get; set; }
        public string CONTENT { get; set; }
        public string TARGET { get; set; }
        public DateTime? ON_DATE { get; set; }
        public DateTime? OFF_DATE { get; set; }
        public string VALID { get; set; }
        public string CREATE_BY { get; set; }
        public DateTime? CREATE_DT { get; set; }
        public string UPDATE_BY { get; set; }
        public DateTime? UPDATE_DT { get; set; }

        public string UPLOAD_KEY { get; set; }
        public IEnumerable<UR_UPLOAD> ATTACHMENTS { get; set; }

        public string IS_TODAY { get; set; }
    }
}