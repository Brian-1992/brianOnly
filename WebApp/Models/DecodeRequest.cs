using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class DecodeRequest : JCLib.Mvc.BaseModel
    {
        public string WmCmpy { get; set; } //公司
        public string WmOrg { get; set; } //成本中心
        public string WmWhs { get; set; } //倉庫
        public string WmSku { get; set; } //規格碼
        public string WmEffcDate { get; set; } //效期
        public string WmLot { get; set; } //批號
        public string WmSeno { get; set; } //序號
        public string WmPak { get; set; } //包裝
        public string WmQy { get; set; } //數量
        public string WmLoc { get; set; } //儲位
        public string WmBox { get; set; } //盒號
        public string WmSrv { get; set; } //醫療服務項目
        public string ThisBarcode { get; set; } //本次讀取的條碼
        public string UdiBarcodes { get; set; } //該單項累計讀取的UDI條碼用<;>做分隔
        public string NhiBarcodes { get; set; } //該單項累計讀取的轉換後條碼用<;>做分隔
        public string GtinString { get; set; } //該單項累計讀取的GTIN字串用<;>做分隔
    }
}