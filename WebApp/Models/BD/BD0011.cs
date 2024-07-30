using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BD0011 : JCLib.Mvc.BaseModel
    {
        public string DOCID { get; set; } //識別號(畫面)
        public string AGEN_NO { get; set; } //廠商代碼(畫面)
        public string MSG { get; set; } //訊息內容(畫面)
        public string OPT { get; set; } //訊息發送註記; A-全部,P-部份
        public string OPT_TEXT { get; set; } //訊息發送註記; A-全部,P-部份
        public string OPT_DISPLAY { get; set; } //訊息發送註記; A-全部,P-部份(畫面)
        public string SEND_DT { get; set; } //通知日期; YYYYMMDD(畫面)
        public string SEND_DT_DISPLAY { get; set; } //通知日期; YYYYMMDD
        public string THEME { get; set; } //訊息主旨(畫面)
        public string STATUS { get; set; } //轉檔識別; 80-未通知,84-待傳MAIL, 82-已傳MAIL
        public string STATUS_NAME { get; set; } //轉檔識別; 80-未通知,84-待傳MAIL, 82-已傳MAIL(畫面)
        public string FILENAME { get; set; } //附件
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}