using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// ME_AB0075D--內湖門急汀洲住藥局工作日
    /// </summary>
    public class ME_AB0075DModles
    {
        /// <summary>
        /// 
        /// </summary>
        public string WORKDATE { get; set; } //1.工作日(YYYMMDD)
        public string CNTORDERNO { get; set; } //2.醫令筆數
        public string CNTRXNO { get; set; } //3.檢驗筆數(檢驗檢查申請單號)
    }
}
