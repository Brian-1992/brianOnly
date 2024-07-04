using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0007M : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼,
        public string MAT_CLASS { get; set; } //物料分類,
        public string MMNAME_C { get; set; } //中文品名,
        public string MMNAME_E { get; set; } //英文品名,
        public string BASE_UNIT { get; set; } //計量單位,
        public string AVG_PRICEA { get; set; } //期末單價(軍),
        public string AVG_PRICEB { get; set; } //期末單價(民),
        public string INV_QTYA { get; set; } //調整前戰備存量, //調整後戰備存量,
        public string PINV_QTYB { get; set; } //調整前民品存量,
        public string PINVQTY { get; set; } //調整前期末存量,
        public string SUM1A { get; set; } //調整前期末戰備成本,
        public string SUM1B { get; set; } //調整前期末民品成本,
        public string SUM1 { get; set; } //調整前期末成本,
        //public string AVG_PRICEB { get; set; } //期末單價(民),
        public string INVENTORYQTY { get; set; } //民品盤點總量,
        public string SUM2 { get; set; } //盤盈虧金額,
        //public string AVG_PRICEA { get; set; } //期末單價(軍),
        //public string AVG_PRICEB { get; set; } //期末單價(民),
        //public string INV_QTYA { get; set; } //調整後戰備存量,
        public string INV_QTYB { get; set; } //調整後民品存量,
        public string INVQTY { get; set; } //調整後期末存量,
        public string SUM4A { get; set; } //調整後期末戰備成本,
        public string SUM4B { get; set; } //調整後期末民品成本,
        public string SUM4 { get; set; } //調整後期末成本,
        public string RAT { get; set; } //期末比值,
    }
}