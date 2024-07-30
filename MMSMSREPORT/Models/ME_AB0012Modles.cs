using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.Models
{
    /// <summary>
    /// 介接公藥(管制藥)所設立Table_1080618
    /// </summary>
    public class ME_AB0012Modles
    {
        public string ORDERNO { get; set; } //1.醫令序號
        public string DETAILNO { get; set; } //2.明細序號
        public string NRCODE { get; set; } //3.病房代碼
        public string BEDNO { get; set; } //4.床位號
        public string MEDNO { get; set; } //5.病歷號碼
        public string CHARTNO { get; set; } //6.病歷號
        public string VISITSEQ { get; set; } //7.門/住診療序號
        public string ORDERCODE { get; set; } //8.院內代碼
        public string DOSE { get; set; } //9.劑量
        public string ORDERDR { get; set; } //10.開單醫師
        public string USEDATETIME { get; set; } //11.預計執行時間
        public string CREATEDATETIME { get; set; } //12.記錄建立日期/時間
        public string SIGNOPID { get; set; } //13.SIGN IN護理人員
        public string USEQTY { get; set; } //14.總量(扣庫)
        public string RESTQTY { get; set; } //15.殘餘量
        public string PROVEDR { get; set; } //16.殘餘量認證醫師
        public string PROVEID2 { get; set; } //17.殘餘量認證人員
        public string MEMO { get; set; } //18.備註 MMDD-HHMI(SIGN IN)/MMDD-HHMI(CREATE DATE)
        public string CHINNAME { get; set; } //19.病歷姓名
        public string ORDERENGNAME { get; set; } //20.藥品英文名
        public string SPECNUNIT { get; set; } //21.規格量及單位
        public string ORDERUNIT { get; set; } //22.醫囑單位
        public string STOCKUNIT { get; set; } //23.扣庫單位
        public string FLOORQTY { get; set; } //24.最低庫存量
        public string PROVEID1 { get; set; } //25.給藥認證人員
        public string CARRYKINDI { get; set; } //26.住消耗歸整
        /*
         *1.以UD結轉歸整(所有口服錠劑、膠囊或可保留24小時以上的針劑), EX:2/116:00~2/415:59
         *2.以天歸整(只能保留24小時的針劑等) ,2/116:00~2/215:59,最後一筆EXTEND調整
         *3.以次歸整(開瓶即消耗), 
         *4.後歸整(水藥等特殊POS的藥品), 前台EXTEND不處理,申報醫令合併作業執行歸整
         *5.不歸整(不需進位，例如買斷藥), 前台EXTEND不處理,申報醫令合併作業進到小數點一位  
         */
        public string STOCKTRANSQTYI { get; set; } //住院扣庫轉換量
        public string STOCKCODE { get; set; } //庫別代碼
        public string STARTDATATIME { get; set; } //申請日期時間
    }
}
