using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    public class PHRDCMGModels
    {
        public string ORDERCODE { get; set; } //1.院內代碼
        public string CHANGETYPE { get; set; } //2.藥物異動類別
        public string CREATEDATETIME { get; set; } //3.記錄建立日期/時間
        public string CHANGEDATE { get; set; } //4.異動日期
        public string DRUGCHANGEMEMO1 { get; set; } //5.備註1
        public string DRUGCHANGEMEMO2 { get; set; } //6.備註2
        public string INSUSIGNI { get; set; } //7.健保負擔碼(住院)
        public string INSUSIGNO { get; set; } //8.健保負擔碼(門診)
        public string CREATEOPID { get; set; } //9.記錄建立人員
        public string PROCDATETIME { get; set; } //10.記錄處理日期/時間
        public string PROCOPID { get; set; } //11.記錄處理人員
    }
}
