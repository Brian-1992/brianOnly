using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BG0003 : JCLib.Mvc.BaseModel
    {
        ///////////////超過十萬品項///////////////
        public string YYYMM { get; set; } //1_年月
        public string MMCODE { get; set; } //1_院內碼
        public string MMNAME_E { get; set; } //1_英文品名
        public string MMNAME_C { get; set; } //1_中文品名
        public string BASE_UNIT { get; set; } //1_計量單位
        public string UPRICE { get; set; } //1_單價
        public string M_AGENNO { get; set; } //1_廠商碼
        public string AGEN_NAMEC { get; set; } //1_廠商名稱
        public string MAT_CLASS { get; set; } //1_物料類別
        public string APPQTY { get; set; } //1_數量
        public string ESTPAY { get; set; } //1_預估申購金額

        ///////////////超過十萬單位///////////////
        public string YYYMM_2 { get; set; } //2_年月
        public string INID { get; set; } //2_責任中心
        public string DOCNO { get; set; } //2_申請單號
        public string MMCODE_2 { get; set; } //2_院內碼
        public string MMNAME_E_2 { get; set; } //2_英文品名
        public string MMNAME_C_2 { get; set; } //2_中文品名
        public string BASE_UNIT_2 { get; set; } //2_計量單位
        public string UPRICE_2 { get; set; } //2_單價
        public string APPQTY_2 { get; set; } //2_數量
        public string ESTPAY_2 { get; set; } //2_預估申購金額

        ///////////////尚未開單單位///////////////
        public string APPDEPT { get; set; } //3_責任中心
        public string INID_NAME { get; set; } //3_責任中心名稱
    }
}