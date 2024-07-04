using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CE0028 : JCLib.Mvc.BaseModel
    {
        public string CHK_STATUS { get; set; } //盤點狀態

        ///////////////找本次所有盤點項目(ALL)///////////////
        public string CHK_NO_A { get; set; } //盤點單號_A
        public string MMCODE_A { get; set; } //院內碼_A
        public string MMNAME_C_A { get; set; } //中文品名_A
        public string MMNAME_E_A { get; set; } //英文品名_A
        public string BASE_UNIT_A { get; set; } //計量單位代碼_A
        public string STORE_QTY_A { get; set; } //總電腦量_A
        public string CHK_QTY1_A { get; set; } //盤點量(初)_A
        public string CHK_QTY2_A { get; set; } //盤點量(複)_A
        public string CHK_QTY3_A { get; set; } //盤點量(三)_A
        public string CHK_QTY_A { get; set; } //盤點量_A
        public string CHK_UID_NAME_A { get; set; } //盤點人員_A
        public string STATUS_TOT_A { get; set; } //盤點階段(1:初盤, 2.複盤, 3:三盤)_A
        public string GAP_T_A { get; set; } //總誤差量_A

        ///////////////用初盤chk_no找三盤的項目 ///////////////
        public string CHK_NO_3 { get; set; } //盤點單號_3
        public string MMCODE_3 { get; set; } //院內碼_3
        public string MMNAME_C_3 { get; set; } //中文品名_3
        public string MMNAME_E_3 { get; set; } //英文品名_3
        public string BASE_UNIT_3 { get; set; } //計量單位_3
        public string CHK_QTY_3 { get; set; } //盤點量_3

    }
}