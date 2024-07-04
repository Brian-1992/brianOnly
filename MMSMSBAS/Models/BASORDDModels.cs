using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    /// <summary>
    /// 介接HIS資料庫 BASORDD資料表(診療收費項目基本表明細)
    /// </summary>
    public class BASORDDModels
    {
        public string ORDERCODE { get; set; } //1.
        public string BEGINDATE { get; set; } //2.
        public string ENDDATE { get; set; } //3.
        public string STOCKTRANSQTYO { get; set; } //4.
        public string STOCKTRANSQTYI { get; set; } //5.
        public string ATTACHTRANSQTYO { get; set; } //6.
        public string ATTACHTRANSQTYI { get; set; } //7.
        public string INSUAMOUNT1 { get; set; } //8.
        public string INSUAMOUNT2 { get; set; } //9.
        public string PAYAMOUNT1 { get; set; } //10.
        public string PAYAMOUNT2 { get; set; } //11.
        public string COSTAMOUNT { get; set; } //12.
        public string MAMAGEFLAG { get; set; } //13.
        public string MAMAGERATE { get; set; } //14.
        public string INSUORDERCODE { get; set; } //15.
        public string INSUSIGNI { get; set; } //16.
        public string INSUSIGNO { get; set; } //17.
        public string INSUEMGFLAG { get; set; } //18.
        public string HOSPEMGFLAG { get; set; } //19.
        public string DENTALREFFLAG { get; set; } //20.
        public string PPFTYPE { get; set; } //21.
        public string PPFPERCENTAGE { get; set; } //22.
        public string INSUKIDFLAG { get; set; } //23.
        public string HOSPKIDFLAG { get; set; } //24.
        public string CONTRACTPRICE { get; set; } //25.
        public string CONTRACNO { get; set; } //26.
        public string SUPPLYNO { get; set; } //27.
        public string CASEFROM { get; set; } //28.
        public string EXAMINEDISCFLAG { get; set; } //29.
        public string CREATEDATETIME { get; set; } //30.
        public string CREATEOPID { get; set; } //31.
        public string PROCDATETIME { get; set; } //32.
        public string PROCOPID { get; set; } //33.
        public string ORIGINALPRODUCER { get; set; } //34.
        public string AGENTNAME { get; set; } //35.
        public string ENDDATETIME { get; set; } //36.
        public string EXECFLAG { get; set; } //37.
        public string DRUGCASEFROM { get; set; } //38.
        public string ARMYINSUORDERCODE { get; set; } //39.
        public string ARMYINSUAMOUNT { get; set; } //40.
        public string PTRESOLUTIONCLASS { get; set; } //41.
        public string FDALICENSENO { get; set; } //
        public string PRICE_T99 { get; set; }  //
    }
}
