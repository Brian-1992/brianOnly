using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer14
{
    public class MI_WHCOST
    {
        public string DATA_YM { get; set; }            //	資料年月			
        public string MMCODE { get; set; }         //	院內碼			
        public string SET_YM { get; set; }         //	月結年月			
        public string PMN_INVQTY { get; set; }            //	上月結存總量			
        public string PMN_AVGPRICE { get; set; }          //	上月結存單價(庫存單價)			
        public string MN_INQTY { get; set; }          //	本月進貨總量			
        public string DISC_UPRICE { get; set; }           //	優惠計量單價			
        public string AVG_PRICE { get; set; }         //	庫存平均單價			
        public string CONT_PRICE { get; set; }            //	合約單價			
        public string MIL_PRICE { get; set; }         //	戰備單價			
        public string UPRICE { get; set; }            //	最小單價(計量單位單價)			
        public string DISC_CPRICE { get; set; }           //	優惠合約單價			
        public string M_CONT_PRICE { get; set; }          //	戰備合約單價			
        public string M_DISC_CPRICE { get; set; }         //	戰備優惠合約單價			
        public string M_UPRICE { get; set; }          //	戰備最小單價			
        public string M_DISC_UPRICE { get; set; }         //	戰備優惠最小單價			
        public string M_STOREID { get; set; }          //	庫備識別碼(0非庫備,1庫備)			
        public string M_CONTID { get; set; }           //	合約識別碼(0合約品項,2非合約,3零購)			
        public string SOURCECODE { get; set; }         //	來源代碼P.買斷,C.寄售,R.核醫,N.其它			
        public string CANCEL_ID { get; set; }         //	是否作廢			
        public string NHI_PRICE { get; set; }         //	健保給付價			
        public string HOSP_PRICE { get; set; }            //	自費價			
        public string UNITRATE { get; set; }          //	出貨單位(每一包裝=多少單位量，預設1) 			
        public string WAR_QTY { get; set; }           //	戰備存量(預設0)			
        public string BASE_UNIT { get; set; }          //	計量單位 			
        public string M_PURUN { get; set; }			//	申購計量單位(包裝單位)

    }
}
