﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// ME_AB0083(C)_退藥異常次數及金額統計表
    /// </summary>
    public class ME_AB0083CModles
    {
        public string CREATEDATE { get; set; } //1.發生日期
        public string NRNAME { get; set; } //2.病房名稱
        public string ORDERCODE { get; set; } //3.院內碼
        public string ORDERENGNAME { get; set; } //4.藥品英文名
        public string QTY { get; set; } //5.數量變動
        public string MONEY { get; set; } //6.總金額
        public string NRCODE { get; set; } //7.病房代碼

    }
}
