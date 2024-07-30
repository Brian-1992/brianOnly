using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0052 : JCLib.Mvc.BaseModel
    {
        public string OrderHospName { get; set; }//別名(院內名稱)
        public string OrderEasyName { get; set; }//簡稱
        public string ScientificName { get; set; }//簡稱
        public string InsuOrderCode { get; set; }//健保碼
        public string MMNAME_C { get; set; }//中文名稱
        public string MMNAME_E { get; set; }//英文名稱
        public DateTime E_CODATE { get; set; }//合約效期
        public string M_PURUN { get; set; }//進貨(採購)單位
        public string E_DRUGAPLTYPE { get; set; }//藥品請領類別
        public string E_PURTYPE { get; set; }//藥品採購案別
        public string PackType { get; set; }//藥品包裝
        public string WEXP_ID { get; set; }//需有批號效期品
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string MMCODE { get; set; }
        public string ORDERCODE { get; set; }

        

    }
}