using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// ME_AB0075C--TPN藥局工作量
    /// </summary>
    public class ME_AB0075CModles
    {
        public string VISITDATE { get; set; } //1.日期(YYYMMDD)
        public string CNTVISITSEQ { get; set; } //2.人數
        public string CNTORDERNO { get; set; } //3.藥品筆數
    }
}
