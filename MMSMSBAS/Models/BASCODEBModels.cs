using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    /// <summary>
    /// 介接HIS資料庫 BASFREQ資料表(給藥頻率代碼表)
    /// </summary>
    public class BASCODEBModels
    {
        public string FREQNO { get; set; } //1.院內頻率
        public string FREQNAME { get; set; } //2.名稱
        public string FREQEASYNAME { get; set; } //3.頻率英文名稱
        public string INSUFREQNO { get; set; } //4.名稱(For 外用)
        public string FREQTIMES { get; set; } //5.健保給藥頻率代碼
        public string DAYSDIVIDE { get; set; } //6.次數(for 24 hours)
        public string FREQSORT { get; set; } //7.天數分割
        public string STATFLAG { get; set; } //8.排列順序
        public string PRNFLAG { get; set; } //9.是否為STAT
        public string CREATEDATETIME { get; set; } //10.是否為PRN
        public string CREATEOPID { get; set; } //11.記錄建立日期/時間
        public string PROCDATETIME { get; set; } //12.記錄建立人員
        public string PROCOPID { get; set; } //13.記錄處理日期/時間
        public string FREQENGNAME { get; set; } //14.記錄處理人員
    }
}
