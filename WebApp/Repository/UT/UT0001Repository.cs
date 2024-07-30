using JCLib.DB;
using System.Collections.Generic;
using Dapper;
using System.Text;
using WebApp.Models.UT;
using WebApp.Models;
using System;

namespace WebApp.Repository.UT
{
    public class UT0001Repository : JCLib.Mvc.BaseRepository
    {
        public UT0001Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<UT_DOCM> GetAllM(string DOCNO,string status,string doctype,string flowid,string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            
            StringBuilder sql = new StringBuilder();
            //sql.Append("SELECT B.SEQ,B.MMCODE,MMNAME_C,MMNAME_E,FLOWID,DOCTYPE,EXP_STATUS");
            //sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE C.BASE_UNIT=D.UNIT_CODE)BASE_UNIT");
            //sql.Append(",A.FRWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N");
            //sql.Append(",(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N");
            //sql.Append(",(SELECT COUNT(1) FROM ME_DOCEXP D WHERE D.DOCNO=B.DOCNO AND B.MMCODE=D.MMCODE) EXP_CNT");
            //switch (doctype) {
            //    case "MR":
            //        if (flowid == "0102")
            //            sql.Append(",APVQTY QTY");
            //        else
            //            sql.Append(",ACKQTY QTY");
            //        break;
            //    case "MR1":
            //    case "MR2":
            //    case "MR3":
            //    case "MR4":
            //        if (flowid == "2")
            //            sql.Append(",EXPT_DISTQTY QTY");
            //        else if (flowid == "5")
            //            sql.Append(",APVQTY QTY");
            //        else
            //            sql.Append(",ACKQTY QTY");
            //        break;
            //    case "MS":
            //        sql.Append(",ACKQTY QTY");
            //        break;
            //    case "TR":
            //    case "TR1":
            //        if (flowid=="0202")
            //            sql.Append(",APVQTY QTY");
            //        else
            //            sql.Append(",ACKQTY QTY");
            //        break;
            //    case "XR":
            //        sql.Append(",APVQTY QTY");
            //        break;
            //    default:
            //        sql.Append(",APPQTY QTY"); //調撥 TR,TR1
            //        break;
            //}
            //sql.Append(",(A.DOCNO,A.DOCTYPE,A.FLOWID) ISEDIT");
            //sql.Append(" FROM ME_DOCM A,ME_DOCD B,MI_MAST C");
            //sql.Append(" WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE AND WEXP_ID='Y' AND A.DOCNO = :DOCNO");
            //sql.Append(" AND DOCTYPE IN ('TR','TR1') AND APVQTY>0");
            sql.Append("SELECT SEQ,MMCODE,MMNAME_C,MMNAME_E,FLOWID,DOCTYPE,EXP_STATUS,BASE_UNIT,FRWH,FRWH_N,TOWH_N,QTY,EDIT_TYPE,IS_CLOSE,IS_ADD");
            sql.Append(" FROM V_ME_DOCD_E A");
            sql.Append(" WHERE A.DOCNO = :DOCNO");
            sql.Append(" AND (EXISTS(SELECT 'X' FROM MI_WHID D WHERE D.WH_NO IN (A.TOWH,A.FRWH) AND WH_USERID=:WH_USERID)");
            sql.Append("  OR (EXISTS(SELECT 'X' FROM UR_ID D,MI_WHMAST E WHERE D.INID=E.INID AND D.TUSER=:WH_USERID)))");
            if ((doctype == "MR3" | doctype == "MR4") & (flowid == "4" | flowid == "5"))
            {//分批交貨點收中
                sql.Append(" AND QTY>0");
            }
            p.Add(":DOCNO", DOCNO);
            p.Add(":WH_USERID", tuser);
            if (status != "") { 
                sql.Append(" AND EXP_STATUS=:EXP_STATUS");
                p.Add(":EXP_STATUS", status);
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UT_DOCM>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UT_DOCD> GetAllD1(string DOCTYPE, string DOCNO,string WH_NO, string SEQ, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            //暫時
            StringBuilder sql = new StringBuilder();
            //sql.Append("SELECT WH_NO,MMCODE,EXP_DATE,LOT_NO,INV_QTY ,NULL APVQTY");
            //sql.Append(" FROM MI_WEXPINV WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE AND INV_QTY>0");

            //sql.Append("SELECT A.WH_NO,A.MMCODE,A.EXP_DATE,A.LOT_NO,INV_QTY , B.APVQTY");
            //sql.Append(" FROM MI_WEXPINV A");
            //sql.Append(" FULL JOIN (SELECT MMCODE,EXP_DATE,LOT_NO,APVQTY FROM ME_DOCEXP B WHERE DOCNO=:DOCNO AND SEQ=:SEQ)B");
            //sql.Append(" ON A.MMCODE=B.MMCODE AND A.EXP_DATE=B.EXP_DATE AND A.LOT_NO=B.LOT_NO");
            //sql.Append(" WHERE A.WH_NO=:WH_NO AND A.MMCODE=:MMCODE "); //AND A.INV_QTY > 0
            sql.Append("SELECT CASE WHEN A.MMCODE IS NOT NULL THEN 'Y' ELSE 'N' END ISFOUND, A.WH_NO,NVL(A.MMCODE,B.MMCODE)MMCODE");
            sql.Append(",NVL(A.EXP_DATE,B.EXP_DATE)EXP_DATE,NVL(A.LOT_NO,B.LOT_NO)LOT_NO,NVL(INV_QTY,0)INV_QTY , B.APVQTY");
            sql.Append(" FROM (SELECT WH_NO,MMCODE,EXP_DATE,LOT_NO,INV_QTY");
            sql.Append("   FROM MI_WEXPINV WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE AND INV_QTY>0");
            if (DOCTYPE == "SP")//注意:逾期不出來 ,報廢要出來
                sql.Append(" AND EXP_DATE<TRUNC(SYSDATE)");
            else
            {
                if (DOCTYPE != "SP1" && DOCTYPE != "RN")
                    sql.Append(" AND EXP_DATE>TRUNC(SYSDATE)");
            }
            sql.Append(" )A FULL JOIN (");
            if (DOCTYPE == "MR3" | DOCTYPE == "MR4") //分批交貨
                sql.Append(" SELECT MMCODE,EXP_DATE,LOT_NO,APVQTY FROM ME_DOCEXP B WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE AND C_STATUS='N'");
            else sql.Append("SELECT MMCODE,EXP_DATE,LOT_NO,APVQTY FROM ME_DOCEXP B WHERE DOCNO=:DOCNO AND SEQ=:SEQ");
            sql.Append(")B");
            sql.Append(" ON A.MMCODE=B.MMCODE AND A.EXP_DATE=B.EXP_DATE AND A.LOT_NO=B.LOT_NO");
            p.Add(":WH_NO", WH_NO);
            p.Add(":MMCODE", MMCODE);
            p.Add(":DOCNO", DOCNO);
            p.Add(":SEQ", SEQ);


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UT_DOCD>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public string GetDoctype(string docno)//UT_DOCM
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"SELECT DOCTYPE||';'||FLOWID FROM ME_DOCM WHERE DOCNO=:DOCNO";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<UT_DOCD> GetAllD2(string FLOWID,string DOCTYPE, string DOCNO, string SEQ, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = "SELECT MMCODE,EXP_DATE,LOT_NO,APVQTY FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            if ((DOCTYPE == "MR3" | DOCTYPE == "MR4") & FLOWID != "6") //分批交貨
                sql +=" AND C_STATUS='N'";
            p.Add(":DOCNO", DOCNO);
            p.Add(":SEQ", SEQ);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UT_DOCD>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UT_DOCD> GetAllD3(string DOCTYPE,string WH_NO, string MMCODE,string QTY, int page_index, int page_size, string sorters)
        {
            StringBuilder sql = new StringBuilder();
            var p = new DynamicParameters();
            sql.Append("select wh_no,MMCODE,exp_date,lot_no,inv_qty");
            sql.Append(" ,CASE WHEN QTY < 0 THEN INV_QTY WHEN QTY <= INV_QTY THEN INV_QTY-QTY END APVQTY");
            sql.Append(" from( SELECT WH_NO,MMCODE,EXP_DATE,LOT_NO,INV_QTY ,sum(INV_QTY) over (order by EXP_DATE,LOT_NO)-:QTY qty");
            sql.Append(" FROM MI_WEXPINV");
            sql.Append(" WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE ");
            if (DOCTYPE == "SP")
                sql.Append(" AND EXP_DATE<TRUNC(SYSDATE)");
            else {
                if (DOCTYPE != "SP1")
                    sql.Append(" AND EXP_DATE>TRUNC(SYSDATE)");
            }
            sql.Append(")");
            p.Add(":WH_NO", WH_NO);
            p.Add(":MMCODE", MMCODE);
            p.Add(":QTY", QTY);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UT_DOCD>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }
        public int CreateD(UT_DOCD UT_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCEXP(DOCNO,SEQ,EXP_DATE,LOT_NO,MMCODE,APVQTY,UPDATE_TIME,UPDATE_USER,UPDATE_IP)   
                                   VALUES(:DOCNO,:SEQ,:EXP_DATE,:LOT_NO,:MMCODE,:APVQTY,SYSDATE,:UPDATE_USER,:UPDATE_IP)";
            return DBWork.Connection.Execute(sql, UT_DOCD, DBWork.Transaction);
        }
        public int CreateD2(UT_DOCD UT_DOCD)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO ME_DOCEXP(DOCNO,SEQ,EXP_DATE,LOT_NO,MMCODE,APVQTY,UPDATE_TIME,UPDATE_USER,UPDATE_IP,C_STATUS,BATCHNO)");
            sql.Append("SELECT :DOCNO,:SEQ,:EXP_DATE,:LOT_NO,:MMCODE,:APVQTY,SYSDATE,:UPDATE_USER,:UPDATE_IP,'N'");
            sql.Append(",(SELECT NVL(MAX(BATCHNO),0)+1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND C_STATUS='Y')");
            sql.Append(" FROM DUAL");
            return DBWork.Connection.Execute(sql.ToString(), UT_DOCD, DBWork.Transaction);
        }
        public int DeleteD(UT_DOCD UT_DOCD)
        {
            string sql = "DELETE ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return DBWork.Connection.Execute(sql, UT_DOCD, DBWork.Transaction);
        }
        public bool CheckExistsDKey(UT_DOCD d)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ AND EXP_DATE=:EXP_DATE AND LOT_NO=:LOT_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, d, DBWork.Transaction) == null);
        }
        public bool CheckExistsD2Key(UT_DOCD d)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ AND EXP_DATE=:EXP_DATE AND LOT_NO=:LOT_NO AND C_STATUS='N'";
            return !(DBWork.Connection.ExecuteScalar(sql, d, DBWork.Transaction) == null);
        }
        public int DeleteD2(UT_DOCD d) {
            string sql = "DELETE ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE AND C_STATUS='N'";
            return DBWork.Connection.Execute(sql, d, DBWork.Transaction);
        }
        //public Int16 GetD_STATUS(string docno,string seq)
        //{
        //    StringBuilder sql = new StringBuilder();
        //    sql.Append("SELECT COUNT(1)");
        //    sql.Append(" FROM ME_DOCD");
        //    sql.Append(" WHERE DOCNO = :DOCNO AND SEQ = :SEQ AND EXP_STATUS='R'");
        //    return Convert.ToInt16(DBWork.Connection.ExecuteScalar(sql.ToString(), new { DOCNO = docno, SEQ = seq }, DBWork.Transaction));
        //}
    }
}