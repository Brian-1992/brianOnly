using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// 介接AB0079S(每月藥品(S)醫令統計)所設立Table_1080705
    /// </summary>
    public class ME_AB0079SModles
    {
        public string ORDERCODE { get; set; } //1.院內碼
        public string ORDERENGNAME { get; set; } //2.英文名稱
        public string CREATEYM { get; set; } //3.查詢月份
        public string SUMQTY { get; set; } //4. 8.醫囑(住)消耗量
        public string SUMAMOUNT { get; set; } //5.  .醫囑(住)消耗金額
        public string OPDQTY { get; set; } //6. 10.醫囑(門)消耗量
        public string OPDAMOUNT { get; set; } //7.  11.醫囑(門)消耗金額
        public string DSM { get; set; } //8.    D=醫生,S=藥品,M=科室，已經指定好了
    }
}
