using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.AB
{
    public class AB0109 : JCLib.Mvc.BaseModel
    {
        public string CRDOCNO { get; set; }         // 緊急醫療出貨單編號
        public string MMCODE { get; set; }          // 院內碼
        public string MMNAME_C { get; set; }        // 中文品名
        public string MMNAME_E { get; set; }        // 英文品名  
        public string APPQTY { get; set; }          // 申請數量
        public string BASE_UNIT { get; set; }         //計量單位(包裝單位)
        public string TOWH { get; set; }            // 入庫庫房
        public string REQDATE { get; set; }         // 要求到貨日期
        public string DRNAME { get; set; }          // 使用醫師
        public string PATIENTNAME { get; set; }     // 病人姓名
        public string CHARTNO { get; set; }         // 病人病歷號
        public string CR_UPRICE { get; set; }       // 單價
        public string M_PAYKIND { get; set; }       // 收費屬性
        public string AGEN_NAME { get; set; }       // 廠商名稱
        public string INID { get; set; }            // 庫房責任中心
        public string INID_NAME { get; set; }       // 責任中心名稱
        public string APPID { get; set; }           // 申請人ID
        public string APPNM { get; set; }           // 申請人名稱
        public string APPTIME { get; set; }         // 申請時間
        public string EMAIL { get; set; }           // 廠商Email
        public string CREATE_USER { get; set; }     // 建立人員
        public string CREATE_TIME { get; set; }     // 建立時間
        public string CR_STATUS { get; set; }       // 狀態
        public string UPDATE_TIME { get; set; }     // 異動時間
        public string UPDATE_USER { get; set; }     // 異動人員
        public string UPDATE_IP { get; set; }       // 異動IP
        public string WH_NAME { get; set; }         // 庫房名稱
        public string AGEN_NO { get; set; }         //廠商名稱
        public string M_CONTPRICE { get; set; }     //合約單價
        public string UPRICE { get; set; }          //最小單價(計量單位單價)
        public string STATUS { get; set; }          // 狀態（中文）
        public string MmcodeDisplay { get; set; }   // 院內碼
        public string AppQtyDisplay { get; set; }   // 申請數量       
        public string ReqDateDisplay { get; set; }  // 要求到貨日期
        public string PatientNameDisplay { get; set; }  //病人姓名 
        public string ChartNoDisplay { get; set; }      //病人病歷號
        public string DrnameDisplay { get; set; }          // 使用醫師
        public string ORDERTIME { get; set; }       //產生通知單時間
        public string ORDERID { get; set; }         //產生通知人員代碼  
        public string APP_AMOUNT { get; set; }      //申請金額     
        public string USEWHERE { get; set; }        //用途   
        public string USEWHEN { get; set; }         //本次申請量預估使用時間  
        public string TEL { get; set; }             //電話   
        public string UseWhereDisplay { get; set; } //用途   
        public string UseWhenDisplay { get; set; }  //本次申請量預估使用時間  
        public string TelDisplay { get; set; }      //電話  
        public string M_APPLYID { get; set; }       //
        public string M_CONTID { get; set; }        //
        public string ISSMALL { get; set; }         //        
        public string RPTDATE { get; set; }         //報表日期        
        public string EMAILTIME { get; set; }       //寄EMAIL時間
        public string REPLYTIME { get; set; }       //收信確認時間


    }
}