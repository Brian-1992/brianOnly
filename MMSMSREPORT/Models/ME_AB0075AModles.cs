using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// ME_AB0075A--化療工作量依醫師確認日期
    /// </summary>
    public class ME_AB0075AModles
    {
        public string PORC_DATE { get; set; } //1.日期(YYYMMDD)
        public string PAT_CNT { get; set; } //2.病人數
        public string TOT_CNT { get; set; } //3.藥品筆數
    }
}
