using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class FA0046 : JCLib.Mvc.BaseModel
    {

        public string MMCODE { get; set; }             //院內碼    
        public string MMNAME_C { get; set; }           //中文品名
        public string MMNAME_E { get; set; }           //英文品名
        public string BASE_UNIT { get; set; }          //單位
        public string PMN_AVGPRICE { get; set; }       //上期結存單價
        public string PMN_INVQTY { get; set; }         //上月結存
        public string PMN_AMOUNT { get; set; }         //上月金額
        public string CONT_PRICE { get; set; }         //合約單價
        public string MN_INQTY { get; set; }           //本月進貨
        public string IN_AMOUNT { get; set; }          //進貨金額
        public string AVG_PRICE { get; set; }          //庫存成本單價
        public string USE_QTY { get; set; }            //本月消耗
        public string USE_AMOUNT { get; set; }         //消耗金額

        //public string AVG_PRICE { get; set; }         //期未單價

        public string STORE_QTY { get; set; }          //本月結存
        public string STORE_AMOUNT { get; set; }       //結存金額
        public string DIFF_QTY { get; set; }           //本月盤盈虧
        public string DIFF_AMOUNT { get; set; }        //盤盈虧金額
        public string TURNOVER { get; set; }           //周轉率


    }
}



