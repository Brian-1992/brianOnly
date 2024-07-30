using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0090 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; } //庫別代碼
        public string WH_NAME { get; set; } //庫別名
        public string LOW_QTY { get; set; } //最低庫存
        public string SAFE_DAY { get; set; } //安全存量天數
        public string SAFE_QTY { get; set; } //安全存量
        public string OPER_DAY { get; set; } //基準天數
        public string OPER_QTY { get; set; } //基準量
        public string CANCEL_ID { get; set; } //各庫停用
        public string MIN_ORDQTY { get; set; } //最小包裝
        public string STORE_LOC { get; set; } //儲位
        public string INV_QTY { get; set; } //庫存量
        public string PWH_NO { get; set; } //上級庫
    }
}