﻿namespace WebApp.Models
{
    public class PARAM_D : JCLib.Mvc.BaseModel
    {
        public string GRP_CODE { get; set; }
        public int DATA_SEQ_O { get; set; }
        public int DATA_SEQ { get; set; }
        public string DATA_NAME { get; set; }
        public string DATA_VALUE { get; set; }
        public string DATA_DESC { get; set; }
        public string DATA_REMARK { get; set; }
    }
}