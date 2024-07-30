using System;
using System.Data;
using JCLib.DB;
using Dapper;
using System.Collections.Generic;
using TSGH.Models;
using WebApp.Models;


namespace WebApp.Repository.AB
{
    public class AB0065_MODEL : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string DOCNO { get; set; }
        public string DOCTYPE { get; set; }

        public string FLOWID { get; set; }
        public string POST_TIME { get; set; }
        public string MEDNO { get; set; }
        public string HOSP { get; set; }
        public string UPDATE_USER { get; set; }
        public string IO { get; set; }
        public string APVQTY { get; set; }
        public string AMOUNT { get; set; }
        public string FRWH { get; set; }
        public string TOWH { get; set; }
        public string PAT { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string DOSE { get; set; }
        public string PATH { get; set; }
        public string FREQ { get; set; }
    }

    public class AB0065Repository : JCLib.Mvc.BaseRepository
    {
        public AB0065Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MMCODE, 
                    MMNAME_E,
                    MMNAME_C
                    FROM MI_MAST A
                    WHERE MAT_CLASS = '01' ";

            if (mmcode != "")
            {
                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

            }
            else
            {
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //病患清單


        public IEnumerable<TSGH.Models.PARAM_D> TRCODEGet()
        {
            string sql = @"select DATA_VALUE, DATA_DESC from PARAM_D where GRP_CODE='MI_WHMAST' and DATA_NAME='WH_GRADE0065' order by DATA_SEQ";
            return DBWork.Connection.Query<TSGH.Models.PARAM_D>(sql, DBWork.Transaction);
        }

        public IEnumerable<AB0065_MODEL> GetPrintData(string TRCODE1, string D0, string D1, string P0, string P1, string P2, string P3, string P4)
        {

            var p = new DynamicParameters();

            var sql = @"select a.DOCNO, a.DOCTYPE, a.FLOWID, SUBSTR(TWN_DATE(a.POST_TIME),1,7)|| '' || TO_CHAR(a.POST_TIME,'HH24MISS') as POST_TIME, 
b.MEDNO, 0 as HOSP, a.UPDATE_USER, '-' as IO, 
b.APVQTY, 
round((select M_CONTPRICE from MI_MAST where MMCODE=b.MMCODE)*b.APVQTY,1) as AMOUNT, 
a.FRWH, a.TOWH, '' as PAT, b.MMCODE, 
c.MMNAME_E, 
0 as DOSE, '' as PATH, '' as FREQ 
from ME_DOCM a, ME_DOCD b, MI_MAST c where 1=1 ";

            //日期範圍　傳進來格式　1080802
            sql += "AND to_char(SUBSTR(TWN_DATE(a.POST_TIME),1,7)) >= to_char(:date1) and to_char(SUBSTR(TWN_DATE(a.POST_TIME),1,7)) <= to_char(:date2) ";
            p.Add(":date1", D0);
            p.Add(":date2", D1);



            //交易碼
            if (TRCODE1 == "4")
            {
                sql += "AND DOCTYPE='MS' and FLOWID='0699' ";
            }else if(TRCODE1 == "5")
            {
                sql += "AND (DOCTYPE='MS' or DOCTYPE='MR') and (FLOWID='0699' or FLOWID='0199') ";
            }
            else if (TRCODE1 == "6")
            {
                sql += "AND DOCTYPE='IN' and FLOWID='0799' ";
            }
            else if (TRCODE1 == "7")
            {
                sql += "AND DOCTYPE='TR' and FLOWID='0299'' ";
            }

            //病例號
            if (P0 != "" && P0 != null)
            {
                sql += "AND b.MEDNO= Trim(:medno) ";
                p.Add(":medno", P0);
            }

            //藥品代碼
            if ((P1 != "" && P1 != null) && P2 == "")
            {
                sql += "AND b.MMCODE >= Trim(:mmcode1) ";
                p.Add(":mmcode1", P1);
            }
            else if ((P1 != "" && P1 != null) && (P2 != "" && P2 != null))
            {
                sql += "AND b.MMCODE >= Trim(:mmcode1) AND b.MMCODE <= Trim(:mmcode2) ";
                p.Add(":mmcode1", P1);
                p.Add(":mmcode2", P2);
            }
            else if (P1 == "" && (P2 != "" && P2 != null))
            {
                sql += "AND b.MMCODE <= Trim(:mmcode2) ";
                p.Add(":mmcode2", P2);
            }

            //藥品名稱
            if ((P3 != "" && P3 != null) && P4 == "")
            {
                sql += "AND c.MMNAME_E >= Trim(:mmname_e1) ";
                p.Add(":mmname_e1", P3);
            }
            else if ((P3 != "" && P3 != null) && (P4 != "" && P4 != null))
            {
                sql += "AND c.MMNAME_E >= Trim(:mmname_e1) AND c.MMNAME_E <= Trim(:mmname_e2) ";
                p.Add(":mmname_e1", P3);
                p.Add(":mmname_e2", P4);
            }
            else if (P3 == "" && (P4 != "" && P4 != null))
            {
                sql += "AND c.MMNAME_E <= Trim(:mmname_e2) ";
                p.Add(":mmname_e2", P4);
            }

            sql += "and a.docno = b.docno and b.MMCODE = c.MMCODE order by b.MMCODE, a.FRWH ";


            return DBWork.Connection.Query<AB0065_MODEL>(sql, p, DBWork.Transaction);
        }

    }

}