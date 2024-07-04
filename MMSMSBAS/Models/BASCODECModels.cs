using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    /// <summary>
    /// 介接HIS資料庫 BASPATH資料表(藥品給藥途徑部位表)
    /// </summary>
    public class BASCODECModels
    {
        public string PATHNO { get; set; } //1.院內給藥途徑(部位)代碼
        public string PATHNAME { get; set; } //2.途徑(部位)名稱
        public string INSUPATHNO { get; set; } //3.途徑英文名稱
        public string CREATEDATETIME { get; set; } //4.健保給藥途徑(部位)代碼
        public string CREATEOPID { get; set; } //5.記錄建立日期/時間
        public string DISPLAYSORT { get; set; } //6.記錄建立人員
        public string PROCDATETIME { get; set; } //7.記錄處理日期/時間
        public string PROCOPID { get; set; } //8.記錄處理人員
        public string PATHENGNAME { get; set; } //9.顯示排列順序
    }
}
