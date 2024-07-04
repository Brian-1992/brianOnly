using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Controllers.FA;

namespace WebApp.Models
{
    public class FA0077 : JCLib.Mvc.BaseModel
    {

        public string GRP_NO { get; set; }
        public string GRP_NAME { get; set; }
        public string INID { get; set; }
        public string INID_NAME { get; set; }

        public string AVG_CONSUME_AMOUNT { get; set; }
        public string TARGET { get; set; }
        public IEnumerable<FA0077ColumnItem> COLUMNS { get; set; }

        public IEnumerable<FA0077Item> ITEMS { get; set; }

        public string M1_AMOUNT { get; set; }
        public string M1_AMOUNT_DIFF { get; set; }
        public string M1_AMOUNT_RATE { get; set; }
        public string M1_REACH { get; set; }

        public string M2_AMOUNT { get; set; }
        public string M2_AMOUNT_DIFF { get; set; }
        public string M2_AMOUNT_RATE { get; set; }
        public string M2_REACH { get; set; }

        public string M3_AMOUNT { get; set; }
        public string M3_AMOUNT_DIFF { get; set; }
        public string M3_AMOUNT_RATE { get; set; }
        public string M3_REACH { get; set; }

        public string C_AMOUNT_1 { get; set; }
        public string C_AMOUNT_2 { get; set; }
        public string C_AMOUNT_3 { get; set; }
        public string C_AVG { get; set; }
        public string PRE_Y_AMOUNT_1 { get; set; }
        public string PRE_Y_AMOUNT_2 { get; set; }
        public string PRE_Y_AMOUNT_3 { get; set; }
        public string PRE_Y_AVG { get; set; }
        public string PRE_S_AMOUNT_1 { get; set; }
        public string PRE_S_AMOUNT_2 { get; set; }
        public string PRE_S_AMOUNT_3 { get; set; }
        public string PRE_S_AVG { get; set; }
    }

    public class FA0077Item : JCLib.Mvc.BaseModel
    {
        public string ABS_YM { get; set; }
        public string ABS_AMOUNT { get; set; }
        public string ABS_AMOUNT_DIFF { get; set; }
        public string ABS_AMOUNT_RATE { get; set; }
        public string REACH { get; set; }
    }

    public class FA0077ColumnItem : JCLib.Mvc.BaseModel
    {
        public string TEXT { get; set; }
        public string DATAINDEX { get; set; }
    }

    public class FA0077DataYmItem : JCLib.Mvc.BaseModel
    {
        public string MinDataym { get; set; }   //  上個月所屬季的起始月
        public string MaxDataym { get; set; }   // --上個月
        public string PreMinDataym { get; set; }    //--去年上個月所屬季的起始月
        public string PreMaxDataym { get; set; }    // --去年上個月所屬季的結束月
        public string Q { get; set; }   // 季
        public string Pre_y { get; set; }   //  去年年分
        public string Y { get; set; }      // 今年年分

        public string WK_DATE { get; set; }


        public string C_MIN_QYM { get; set; }         // 本季起始月
        public string C_MAX_QYM { get; set; }         // 本季結束月
        public string C_Y { get; set; }
        public string C_Q { get; set; }
        public string PRE_Y_MIN_QYM { get; set; }   // 去年同季起始月
        public string PRE_Y_MAX_QYM { get; set; }   // 去年同季結束月
        public string PRE_Y_Y { get; set; }
        public string PRE_Y_Q { get; set; }
        public string PRE_S_MIN_QYM { get; set; }   // 前季起始月
        public string PRE_S_MAX_QYM { get; set; }   // 前季結束月
        public string PRE_S_Y { get; set; }
        public string PRE_S_Q { get; set; }
    }

    public class FA0077Detail : JCLib.Mvc.BaseModel
    {
        public string DATA_YM { get; set; }
        public string GRP_NO { get; set; }
        public string GRP_NAME { get; set; }
        public string INID { get; set; }
        public string INID_NAME { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string DISC_CPRICE { get; set; }
        public string USE_QTY { get; set; }
        public string USE_AMOUNT { get; set; }
        public string TUNROVER { get; set; }
        public string MSTOREID { get; set; }
    }
}