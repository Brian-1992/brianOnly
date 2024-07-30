using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// ME_AB0075B--PCA每月工作量及項目統計
    /// </summary>
    public class ME_AB0075BModles
    {
        public string SDATE { get; set; } //1.日期(YYYMMDD)
        public string CNT { get; set; } //2.人數
        public string ORDERCODE { get; set; } //3.PCA_類別
        public string SUMQTY { get; set; } //4.PCA_類別筆數
    }
}
