using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// 介接AB0071所設立Table_1080704
    /// </summary>
    public class ME_AB0071Modles
    {
        public string CHARTNO { get; set; } //1.病歷號
        public string MEDNO { get; set; } //2.病歷號碼
        public string VISITSEQ { get; set; } //3.門/住診療序號
        public string ORDERNO { get; set; } //4.醫令序號
        public string DETAILNO { get; set; } //5.明細序號
        public string ORDERCODE { get; set; } //6.院內碼
        public string USEQTY { get; set; } //7.數量
        public string CHINNAME { get; set; } //8.開立醫師
        public string SIGNOPID { get; set; } //9.給藥人員
        public string CREATEDATETIME { get; set; } //10.日期時間
        public string STOCKCODE { get; set; } //11.庫別代碼
        public string INOUTFLAG { get; set; } //12.A:買 D:退
        public string ORDERENGNAME { get; set; } //13.英文名稱
        public string RESTRICTCODE { get; set; } //14.管制用藥
        public string HIGHPRICEFLAG { get; set; } //15.高價用藥(Y/N)
        public string NRCODE { get; set; } //16.病房代碼
        public string BEDNO { get; set; } //17.床位號
        public string NRCODENAME { get; set; } //18.病房-床位
        public string DOSE { get; set; } //19.劑量
        public string ORDERUNIT { get; set; } //20.劑量單位
        public string PATHNO { get; set; } //21.途徑
        public string FREQNO { get; set; } //22.頻率
        public string PAYFLAG { get; set; } //23.自費
        public string BUYFLAG { get; set; } //24.自備
        public string BAGSEQNO { get; set; } //25.藥袋號
        public string RXNO { get; set; } //26.處方箋
    }
}
