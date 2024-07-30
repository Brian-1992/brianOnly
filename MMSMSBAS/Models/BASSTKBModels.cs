using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    /// <summary>
    /// 介接HIS STKSUPP(供應商資料表)資料表所有欄位
    /// </summary>
    public class BASSTKBModels
    {
        public string SUPPLYNO { get; set; } //1.廠商代碼(供應商代碼)
        public string SUPPLYEASYNAME { get; set; } //2.供應商簡稱
        public string SUPPLYCHINNAME { get; set; } //3.供應商中文名稱
        public string CONSIGNMENTCODE { get; set; } //4.寄售代碼
        public string CONTACTTELNO { get; set; } //5.連絡電話
        public string FAXNO { get; set; } //6.傳真號碼
        public string EMAIL { get; set; } //7.E-MAIL
        public string ADDRESS { get; set; } //8.通訊地址
        public string CONTACTMAN { get; set; } //9.連絡人
        public string BANKACCOUNT { get; set; } //10.農銀帳號
        public string CREATEDATETIME { get; set; } //11.記錄建立日期/時間
        public string CREATEOPID { get; set; } //12.記錄建立人員
        public string PROCDATETIME { get; set; } //13.記錄處理日期/時間
        public string PROCOPID { get; set; } //14.記錄處理人員
        public string ISLOCALBANK { get; set; } //15.
        public string BANKNAME { get; set; } //16.
        public string BANKCODE { get; set; } //17.
        public string BANKSUBCODE { get; set; } //18.
        
    }
}
