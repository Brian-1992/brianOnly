using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// 介接HISBACK所設立Table_1080704
    /// </summary>
    public class HISBACKModles
    {
        public string MEDNO { get; set; } //1.病歷號碼
        public string CHINNAME { get; set; } //2.姓名
        public string NRCODE { get; set; } //3.病房
        public string BEDNO { get; set; } //4.病床
        public string ORDERCODE { get; set; } //5.院內碼
        public string ORDERENGNAME { get; set; } //6.藥品名稱
        public string BEGINDATETIME { get; set; } //7.開始日期
        public string ENDDATETIME { get; set; } //8.結束日期
        public string DOSE { get; set; } //9.劑量
        public string FREQNO { get; set; } //10.頻率
        public string NEEDBACKQTY { get; set; } //11.建議退量
        public string BACKQTY { get; set; } //12.實際退量
        public string DIFF { get; set; } //13.差異量
        public string PHRBACKREASON { get; set; } //14.
        public string CREATEDATETIME { get; set; } //15.退藥日期
        public string CREATEOPID { get; set; } //16.退藥人ID
        public string CHARTNO { get; set; } //17.病歷號
        public string STOCKUNIT { get; set; } //18.扣庫單位
        public string ORDERNO { get; set; } //19.醫令序號
        public string INSUAMOUNT1 { get; set; } //20.健保價 (2020.8.7移除)
        public string PAYAMOUNT1 { get; set; } //21.自費價 (2020.8.7移除)
        public string BACKNAME { get; set; } //22.退藥人
        public string ORDERUNIT { get; set; } //23.醫囑單位
        public string PROCDATETIME { get; set; } //24.BACK處理時間
        public string BACKKIND { get; set; } //25.退藥類別
        public string ORDERTYPE { get; set; } //26.醫令種類
        public string RETURNSTOCKCODE { get; set; } //27.退庫別
        public string USEDATETIME { get; set; } //28.預計執行時間
        //public string PHRBACKREASON_NAME { get; set; } //29.退藥原因_中文(code_src好像找不到)

        public string SCDT { get; set; } // CREATEDATETIME 開始時間
        public string ECDT { get; set; } // CREATEDATETIME 結束時間
        public string PAYFLAG { get; set; } // 自費否Y/N
        public string BUYFLAG { get; set; } // 自費購買
        public string ORDERSORT { get; set; } // 醫令種類
    }
}
