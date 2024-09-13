using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Utilities.models
{
    public class logModel
    {
        public string ID { get; set; }
        public string ADCNO { get; set; }
        /// <summary>
        /// 呼叫時間
        /// </summary>
        public DateTime REQUESTDATETIME { get; set; }
        /// <summary>
        /// 呼叫的作業
        /// </summary>
        public string FUNCTIONNAME { get; set; }
        /// <summary>
        /// 傳入的參數
        /// </summary>
        public string REQUESTDATA { get; set; }
        /// <summary>
        /// 回應時間
        /// </summary>
        public DateTime RESPONSEDATETIME { get; set; }
        /// <summary>
        /// 回應狀態
        /// </summary>
        public int RESPONSESTATUS { get; set; }
        /// <summary>
        /// 回應內容
        /// </summary>
        public string RESPONSECONTENT { get; set; }
    }
}

         
