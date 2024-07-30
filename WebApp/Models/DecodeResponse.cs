using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class DecodeResponse : JCLib.Mvc.BaseModel
    {
        public string WmCmpy { get; set; } //公司
        public string WmWhs { get; set; } //倉庫
        public string WmOrg { get; set; } //成本中心
        public string CrVmpy { get; set; } //供應商料號
        public string CrItm { get; set; } //供應商料號
        public string WmRefCode { get; set; } //供應商料號
        public string WmBox { get; set; } //材料盒
        public string WmLoc { get; set; } //儲位
        public string WmSrv { get; set; } //技術碼
        //品項訊息
        public string WmSku { get; set; } //規格碼
        public string WmMid { get; set; } //資材碼
        public string WmMidName { get; set; } //院內俗品
        public string WmMidNameH { get; set; } //院內品名
        public string WmSkuSpec { get; set; } //規格
        public string WmBrand { get; set; } //廠牌
        public string WmMdl { get; set; } //型號
        public string WmMidCtg { get; set; } //類別
        public string WmEffcDate { get; set; } //效期
        public string WmLot { get; set; } //批號
        public string WmSeno { get; set; } //序號
        public string WmPak { set; get; } //包裝名稱
        public string WmQy { set; get; } //包裝量
        public string ThisBarcode { get; set; } //本次讀取的條碼
        public string UdiBarcodes { get; set; } //該單項累計讀取的UDI條碼用<;>做分隔
        public string GtinString { get; set; } //該單項累計讀取的GTIN 用<;>做分隔
        public string NhiBarcode { get; set; } //傳回健保局要的條碼形態GS1用()號
        public string NhiBarcodes { get; set; } //該單項累計健保局要的條碼形態<;>做分隔
        public string BarcodeType { set; get; } //解碼後填入條碼別UDI/AHOP/SKU/BOX/LOC/SRV
        public string GtinInString { set; get; } //解碼取得Gtin In 字串
        // ===================================================
        public string IsChgItm { set; get; }
        public string Result { set; get; }
        public string ErrMsg { set; get; }
    }
}