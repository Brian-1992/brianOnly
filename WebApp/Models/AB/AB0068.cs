using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0068 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }      // 院內碼
        public string MMNAME_E { get; set; }    // 英文品名
        public string BF_QTY { get; set; }      // 上日結存(A)
        public string IN_SUM { get; set; }      // 入帳(B)
        public string OUT_SUM { get; set; }     // 出帳(C)
        public string CHK { get; set; }         
        public string TD_QTY { get; set; }


        public string WH_NO { get; set; }       // 庫房代碼
        public string WH_NAME { get; set; }     // 庫房名稱


        public string USEO_QTY { get; set; }    // 醫令消耗(D)
        public string RS_QTY { get; set; }      // 醫令退藥(E)
        public string CHIO_QTY { get; set; }    // 盤點(F)
        public string ADJ_QTY { get; set; }     // 調帳(G)
        public string AF_QTY { get; set; }      // 本日結存(H), 當日結存(日報), 結存(月報)

        public string LOSS_QTY { get; set; }    // 耗損(I)

        public string E_ORDERDCFLAG { get; set; }
        public string CTDMDCCODE { get; set; }


        public string ORI_USEO_QTY { get; set; }
        public string NEW_USEO_QTY { get; set; }

        public string ORI_AF_QTY { get; set; }
    }
}

