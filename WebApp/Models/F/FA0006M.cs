using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0006M : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼,
        public string MAT_CLASS { get; set; } //物料分類,
        public string MMNAME_C { get; set; } //中文品名,
        public string MMNAME_E { get; set; } //英文品名,
        public string BASE_UNIT { get; set; } //計量單位,
        public string PMN_AVGPRICEA { get; set; } //上期結存單價(軍),
        public string PMN_AVGPRICEB { get; set; } //上期結存單價(民),
        public string PINV_QTYA { get; set; } //期初戰備存量,
        public string PINV_QTYB { get; set; } //期初民品存量,
        public string PINVQTY { get; set; } //期初合計存量,
        public string SUM1A { get; set; } //期初戰備成本,
        public string SUM1B { get; set; } //期初民品成本,
        public string SUM1 { get; set; } //期初成本,
        public string IN_PRICE { get; set; } //進貨單價,
        public string IN_QTYA { get; set; } //戰備進貨量,
        public string IN_QTYB { get; set; } //民品進貨量,
        public string INQTY { get; set; } //合計進貨量,
        public string SUM2A { get; set; } //戰備進貨成本,
        public string SUM2B { get; set; } //民品進貨成本,
        public string SUM2 { get; set; } //進貨成本,
        public string AVG_PRICEA { get; set; } //消耗單價(軍), //期末單價(軍),
        public string AVG_PRICEB { get; set; } //消耗單價(民), //期末單價(民),
        public string OUT_QTYA { get; set; } //戰備消耗量,
        public string OUT_QTYB { get; set; } //民品消耗量,
        public string OUTQTY { get; set; } //合計消耗量,
        public string SUM3A { get; set; } //戰備消耗成本,
        public string SUM3B { get; set; } //民品消耗成本,
        public string SUM3 { get; set; } //消耗成本,
        public string INV_QTYA { get; set; } //期末戰備存量,
        public string INV_QTYB { get; set; } //期末民品存量,
        public string INVQTY { get; set; } //期末合計存量,
        public string SUM4A { get; set; } //期末戰備成本,
        public string SUM4B { get; set; } //期末民品成本,
        public string SUM4 { get; set; } //期末成本,

    }
}