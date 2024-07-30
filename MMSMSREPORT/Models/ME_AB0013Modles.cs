using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// 介接藥品補發所設立Table_1080717
    /// 例如：藥局收到病房提出需補發藥品，需確認是不是真的有這筆醫令，所需要確認的清單
    /// </summary>
    public class ME_AB0013Modles
    {
        public string MEDNO { get; set; } //1.病歷號碼
        public string CHARTNO { get; set; } //2.病歷號
        public string CHINNAME { get; set; } //3.病患姓名
        public string VISITSEQ { get; set; } //4.住院號
        public string NRCODE { get; set; } //5.病房號(護理站)
        public string BEDNO { get; set; } //6.病床號
        public string CREATEDATETIME { get; set; } //7.追藥時間
    }
}
