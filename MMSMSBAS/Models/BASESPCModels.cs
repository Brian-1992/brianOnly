using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    public class BASESPCModels
    {
        public string ESPTYPE { get; set; }             //1.特殊品項類別
        public string SPECIALORDERCODE { get; set; }    //2.特殊品項代碼
        public string INSUFLAG { get; set; }            //3.健保碼否
        public string ESPRECNO { get; set; }            //4.特殊品項資料序號
        public string SYSTEMID { get; set; }            //5.系統代碼
        public string CREATEDATETIME { get; set; }      //6.記錄建立日期/時間
        public string CREATEOPID { get; set; }          //7.記錄建立人員
        public string CANCELFLAG { get; set; }          //8.記錄作廢碼
        public string CANCELOPID { get; set; }          //9.記錄作廢人員
        public string CANCELDATETIME { get; set; }      //10.記錄作廢日期/時間
        public string PROCOPID { get; set; }            //11.記錄處理人員
        public string PROCDATETIME { get; set; }        //12.記錄處理日期/時間

    }
}
