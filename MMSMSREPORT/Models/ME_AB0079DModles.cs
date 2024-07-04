using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary> 
    /// 介接AB0079D(每月醫師(D)醫令統計)所設立Table_1080704
    /// </summary>
    public class ME_AB0079DModles
    {
        public string ORDERCODE { get; set; } //1.院內碼
        public string ORDERENGNAME { get; set; } //2.英文名稱
        public string CREATEYM { get; set; } //3.查詢月份
        public string ORDERDR { get; set; } //4.醫師代碼
        public string CHINNAME { get; set; } //5.醫師姓名
        public string SECTIONNO { get; set; } //6.科室
        public string SECTIONNAME { get; set; } //7.科室名
        public string SUMQTY { get; set; } //8.醫囑(住)消耗量
        public string SUMAMOUNT { get; set; } //9.醫囑(住)消耗金額
        public string OPDQTY { get; set; } //10.醫囑(門)消耗量
        public string OPDAMOUNT { get; set; } //11.醫囑(門)消耗金額
        public string DSM { get; set; } //12. D=醫生,S=藥品,M=科室，已經指定好了
    }
}
