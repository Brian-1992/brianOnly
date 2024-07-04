using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0157Repository : JCLib.Mvc.BaseRepository
    {
        public AA0157Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string[] str_matclass, string apptime1, string apptime2, string applykind, string[] str_flowid, string towh, string applydate, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.*,
                (
                    select DATA_DESC from PARAM_D 
                    where grp_code = 'ME_DOCM' and data_name = 'FLOWID_MR1' 
                    and a.flowid = data_value
                ) as FLOWID_N,
                (
                    select MAT_CLSNAME from MI_MATCLASS 
                    where MAT_CLASS=a.MAT_CLASS
                ) as MAT_CLASS_N,
                (select WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=a.FRWH) as FRWH_N,
                (select WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=a.TOWH) as TOWH_N,
                (
                    select DATA_DESC from PARAM_D 
                    where grp_code = 'ME_DOCM' and data_name = 'APPLY_KIND' 
                    and a.APPLY_KIND = data_value
                ) as APPLY_KIND_N, 
                (select UNA from UR_ID where TUSER=a.APPID) as APP_NAME,
                TWN_DATE(a.APPTIME) as APPTIME_T,
                (select EXT from UR_ID where TUSER=a.APPID) as EXT,
                (case when a.docno = nvl(srcdocno, docno)then 'N' else 'Y' end) as SRCDOCYN,
                (select wh_grade from MI_WHMAST where wh_no = (select frwh from ME_DOCM where docno = a.docno)) as WH_GRADE,
                (select distinct RDOCNO from ME_DOCD where DOCNO=a.DOCNO and rownum=1) as RDOCNO
                from ME_DOCM a
                where a.doctype in ('MR3','MR4') and a.APPLY_KIND in ('1','2')
                and a.frwh in (select wh_no from MI_WHID where wh_userId = :TUSER)
                ";

            p.Add(":TUSER", tuser);

            if (str_matclass.Length > 0)
            {
                sql += " and a.MAT_CLASS in :p0 ";
                p.Add(":p0", str_matclass);
            }
            if (apptime1 != "")
            {
                sql += " and TWN_DATE(a.APPTIME) >= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime1));
            }
            if (apptime2 != "")
            {
                sql += " and TWN_DATE(a.APPTIME) <= :p2 ";
                p.Add(":p2", string.Format("{0}", apptime2));
            }
            if (applykind != "")
            {
                sql += " and a.APPLY_KIND = :p3 ";
                p.Add(":p3", string.Format("{0}", applykind));
            }
            if (str_flowid.Length > 0)
            {
                sql += " and a.FLOWID in :p4 ";
                p.Add(":p4", str_flowid);
            }
            if (towh != "")
            {
                sql += " and a.TOWH = :p5 ";
                p.Add(":p5", string.Format("{0}", towh));
            }
            if (applydate != "")
            {
                sql += @" and ( a.MAT_CLASS='02' or (
                            exists (select 1 from MM_WHAPLDT where TWN_DATE(APPLY_DATE)=:p6 and WH_NO=a.TOWH) ) )
                         ";
                p.Add(":p6", string.Format("{0}", applydate));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            //1120524 囤儲量CAN_DIST_QTY剔除舊資料需1120501後才計算
            var sql = @"select DOCNO, SEQ, MMCODE, APPQTY, APLYITEM_NOTE, GTAPL_RESON, STAT, EXPT_DISTQTY, APVQTY, ACKQTY, 
                APL_CONTIME, BW_MQTY, BW_SQTY, APVTIME, ONWAY_QTY, MMNAME_C, MMNAME_E, BASE_UNIT, M_CONTPRICE, DISC_UPRICE, 
                AVG_PRICE, INV_QTY, AVG_APLYQTY, TOT_APVQTY, TOT_BWQTY, TOT_DISTUN, FLOWID, SAFE_QTY, HIGH_QTY, A_INV_QTY, DIS_TIME_T, 
                (case when (case when B_INV_QTY>CAN_DIST_QTY then CAN_DIST_QTY else B_INV_QTY end)<0 then 0 else (case when B_INV_QTY>CAN_DIST_QTY then CAN_DIST_QTY else B_INV_QTY end) end) as B_INV_QTY, 
                GTAPL_RESON_N, ISTRANSPR, CAN_DIST_QTY, SHORT_REASON
                from (
                    SELECT A.DOCNO, A.SEQ, A.MMCODE, A.APPQTY, A.APLYITEM_NOTE, A.GTAPL_RESON, A.STAT, A.EXPT_DISTQTY, A.APVQTY, A.ACKQTY, TWN_TIME(a.apl_contime) as APL_CONTIME, A.BW_MQTY, A.BW_SQTY, A.APVTIME, A.ONWAY_QTY, B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, B.M_CONTPRICE, B.DISC_UPRICE, (SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1) as AVG_PRICE, nvl((SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ),0) as INV_QTY, ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH) as AVG_APLYQTY, (NVL(E.TOT_APVQTY,0) + (case when c.flowid in ('2') then NVL(A.APPQTY,0) else 0 end)) as TOT_APVQTY, nvl((SELECT SUM(TOT_BWQTY) FROM V_MM_TOTAPL WHERE DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and MMCODE=A.MMCODE),0) as TOT_BWQTY, (SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO  IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1') AND ROWNUM=1) as TOT_DISTUN, C.FLOWID, ( SELECT SAFE_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) as SAFE_QTY, NVL(( SELECT HIGH_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO = C.TOWH AND ROWNUM=1 ),0) as HIGH_QTY, ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=WHNO_MM5 ) as A_INV_QTY, TWN_DATE(A.DIS_TIME) as DIS_TIME_T, (CASE WHEN NVL(( SELECT HIGH_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO = C.TOWH AND ROWNUM=1 ),0) > ( NVL(E.TOT_APVQTY,0) + NVL(A.APPQTY,0) ) 
                        THEN CEIL(A.APPQTY/( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO  IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1') AND ROWNUM=1)) * ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO  IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1') AND ROWNUM=1)
                        ELSE CEIL((NVL(( SELECT HIGH_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO = C.TOWH AND ROWNUM=1 ),0) - NVL(E.TOT_APVQTY,0))/ ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO  IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1') AND ROWNUM=1) ) *( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO  IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1') AND ROWNUM=1)
                        END)B_INV_QTY, (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='GTAPL_REASON' AND DATA_VALUE=A.GTAPL_RESON) as GTAPL_RESON_N, (case when nvl(a.isTransPr,'N')='Y' then 'V' else ' ' end) as ISTRANSPR,
                        (nvl((SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ),0)-nvl( (select sum(DIST_QTY) from BC_CS_DIST_LOG where DIST_STATUS<>'T' and MMCODE=A.MMCODE and TWN_DATE(DIST_TIME)>'1120501'),0)) as CAN_DIST_QTY, A.SHORT_REASON
                    FROM ME_DOCD A
                    INNER JOIN MI_MAST B ON B.MMCODE = A.MMCODE 
                    INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                    LEFT OUTER JOIN V_MM_TOTAPL2 E ON E.DATE_YM=SUBSTR(TWN_DATE((case when c.post_time is null then C.APPTIME else C.post_time end)),0,5) and E.MMCODE=A.MMCODE  and E.TOWH = C.TOWH
                    WHERE 1=1
                ";

            if (DOCNO != "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", DOCNO));
            }
            else
            {
                sql += " AND 1=2 ";
            }
            if (MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", MMCODE));
            }

            sql += ")";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string tuser)
        {
            var sql = @"with temp_whkinds as (
                        select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id
                        from MI_WHID a, MI_WHMAST b
                        where wh_userid = :TUSER
                        and a.wh_no = b.wh_no
                        and b.wh_grade = '1'
                        and a.task_id in ('2','3')
                    )
                    select distinct b.mat_class as VALUE, b.mat_clsname as TEXT, b.mat_class || ' ' ||  b.mat_clsname as COMBITEM
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.task_id = b.mat_clsid)
                    union
                    select distinct b.mat_class as VALUE,
                    b.mat_clsname as TEXT,
                    b.mat_class || ' ' ||  b.mat_clsname as COMBITEM  
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.task_id = '2')
                    and b.mat_clsid = '3'
                    ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetApplyKindCombo()
        {
            var sql = @" select distinct DATA_VALUE as VALUE, DATA_DESC as TEXT,
                    DATA_VALUE || ' ' ||  DATA_DESC as COMBITEM  
                    from PARAM_D
                    where GRP_CODE='ME_DOCM' and DATA_NAME='APPLY_KIND'
                    and DATA_VALUE in ('1', '2')
                    order by DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo(string tuser)
        {
            var sql = @" with temp_whkinds as (
                    select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id
                    from MI_WHID a, MI_WHMAST b
                    where wh_userid = :TUSER
                    and a.wh_no = b.wh_no
                    and  (b.wh_grade ='1' and a.task_id in ('2','3'))
                ), 
                temp_mat_class as (
                    select distinct b.mat_class as VALUE,b.mat_clsname as TEXT,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM 
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.task_id = b.mat_clsid)
                    union
                    select distinct b.mat_class as VALUE,b.mat_clsname as TEXT,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM  
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.task_id = '2')
                    and b.mat_clsid = '3'
                )
                select DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT,
                DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                from temp_mat_class a, PARAM_D b
                where a.VALUE in ('02', '03', '04', '05', '06', '07', '08') 
                and b.GRP_CODE='ME_DOCM' and b.DATA_NAME='FLOWID_MR1'
                order by VALUE
                 ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTowhCombo(string tuser)
        {
            var sql = @" with temp_grade_kind as (
                        select distinct wh_grade, wh_kind  
                            from MI_WHID a, MI_WHMAST b
                        where a.wh_userid =:TUSER
                        and a.wh_no = b.wh_no
                        and b.wh_grade = '1'
                        and b.wh_kind = '1'
                    )
                    select b.wh_no as VALUE, b.wh_name as TEXT, b.wh_no||' '||b.wh_name as COMBITEM  
                    from temp_grade_kind a, MI_WHMAST b
                    where a.wh_kind = b.wh_kind
                    and a.wh_grade < b.wh_grade
                    and nvl(b.cancel_id, 'N') = 'N'
                    and b.wh_grade not in ('5','M','S')
                    order by VALUE
                 ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A, ME_DOCD B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":DOCNO", p1);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string ChkFlowid(string id)
        {
            string sql = @"SELECT substr(FLOWID, -1) FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
        }

        public bool ChkSrcdoc(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO and DOCNO <> SRCDOCNO and SRCDOCNO is not null";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool ChkApptime(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM A WHERE DOCNO=:DOCNO
                and (
                    (A.doctype = 'MR3' and trunc(A.apptime) >= to_date(to_char(sysdate,'yyyymm')||(select DATA_VALUE from PARAM_D where GRP_CODE='MM_PR_M' and DATA_NAME='0308_BDAY'),'yyyymmdd')  
                    and trunc(A.apptime) <= to_date(to_char(sysdate,'yyyymm')||(select DATA_VALUE from PARAM_D where GRP_CODE='MM_PR_M' and DATA_NAME='0308_EDAY'),'yyyymmdd')
                    and trunc(A.sendapvtime) >= to_date(to_char(sysdate,'yyyymm')||(select DATA_VALUE from PARAM_D where GRP_CODE='MM_PR_M' and DATA_NAME='0308_BDAY'),'yyyymmdd') 
                    and trunc(A.sendapvtime) <= to_date(to_char(sysdate,'yyyymm')||(select DATA_VALUE from PARAM_D where GRP_CODE='MM_PR_M' and DATA_NAME='0308_EDAY'),'yyyymmdd'))
                    or
                    (A.doctype = 'MR4' and trunc(A.apptime) >= to_date(to_char(sysdate,'yyyymm')||(select DATA_VALUE from PARAM_D where GRP_CODE='MM_PR_M' and DATA_NAME='02_BDAY'),'yyyymmdd')  
                    and trunc(A.apptime) <= to_date(to_char(sysdate,'yyyymm')||(select DATA_VALUE from PARAM_D where GRP_CODE='MM_PR_M' and DATA_NAME='02_EDAY'),'yyyymmdd')
                    and trunc(A.sendapvtime) >= to_date(to_char(sysdate,'yyyymm')||(select DATA_VALUE from PARAM_D where GRP_CODE='MM_PR_M' and DATA_NAME='02_BDAY'),'yyyymmdd') 
                    and trunc(A.sendapvtime) <= to_date(to_char(sysdate,'yyyymm')||(select DATA_VALUE from PARAM_D where GRP_CODE='MM_PR_M' and DATA_NAME='02_EDAY'),'yyyymmdd'))
                ) ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool ChkIsTransPr(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO and nvl(isTransPr,'N') = 'Y' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public string ChkAppqty(string id)
        {
            string sql = @"select MMCODE from ME_DOCD where docno = :DOCNO and nvl(isTransPr,'N') = 'N' and (EXPT_DISTQTY+BW_MQTY)<APPQTY ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction));
        }

        public bool ChkAppqty2(string id)
        {
            string sql = @"select 1 from ME_DOCD where docno = :DOCNO and (EXPT_DISTQTY+BW_MQTY)>APPQTY ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool ChkDocd(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool ChkBwMqty(string id)
        {
            string sql = @"select 1 from ME_DOCD A where A.docno = :DOCNO and A.BW_MQTY > 0 and A.BW_MQTY>(SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=WHNO_MM5 ) ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        //public bool ChkExptDistqty(string id)
        //{
        //    string sql = @"select 1 from ME_DOCD where docno = :DOCNO and EXPT_DISTQTY>B_INV_QTY ";
        //    return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        //}

        public string CheckExptBwQtyMmcode(string docno)
        {
            string sql = @"SELECT listagg(A.MMCODE,',')within group (order by A.MMCODE) as MMCODE FROM ME_DOCD A,ME_DOCM C
                            WHERE A.DOCNO=C.DOCNO AND A.DOCNO=:DOCNO
                            AND ( EXPT_DISTQTY + BW_MQTY ) > 
                                ( (nvl(( select INV_QTY from MI_WHINV where MMCODE = A.MMCODE AND WH_NO=C.FRWH ),0)-
                                   nvl(( select sum(DIST_QTY) from BC_CS_DIST_LOG where DIST_STATUS<>'T' and MMCODE=A.MMCODE and TWN_DATE(DIST_TIME)>'1120501'),0))+ 
                                  ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=WHNO_MM5 ) )
                            AND nvl(A.ISTRANSPR,'N') <> 'Y' ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
        }

        public string CheckExptQtyMmcode(string docno)
        {
            string sql = @"select listagg(A.MMCODE,',')within group (order by A.MMCODE) as MMCODE from ME_DOCD A, ME_DOCM C
                            where A.DOCNO=C.DOCNO AND A.DOCNO=:DOCNO
                            and EXPT_DISTQTY>0
                            and EXPT_DISTQTY>(nvl((select INV_QTY from MI_WHINV where MMCODE=A.MMCODE and WH_NO=C.FRWH),0)-
                                              nvl((select sum(DIST_QTY) from BC_CS_DIST_LOG where DIST_STATUS<>'T' and MMCODE=A.MMCODE and TWN_DATE(DIST_TIME)>'1120501'),0))        
                            AND nvl(A.ISTRANSPR,'N') <> 'Y' 
                            ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
        }

        public int Apply_UpdateMeDocd1(ME_DOCM me_docm)
        {
            var sql = @"update ME_DOCD 
                set pick_qty = nvl(trim(EXPT_DISTQTY), '0')+ nvl(trim(BW_MQTY), '0'), 
                ackqty = nvl(trim(EXPT_DISTQTY), '0')+ nvl(trim(BW_MQTY), '0'), 
                apvqty=nvl(trim(EXPT_DISTQTY), '0')+ nvl(trim(BW_MQTY), '0'), 
                dis_user = :UPDATE_USER, dis_time = sysdate, apl_contime = sysdate, 
                update_user = :UPDATE_USER, update_time = sysdate, update_ip = :UPDATE_IP, apvtime = sysdate, apvid = :UPDATE_USER
                where docno = :DOCNO
                and nvl(isTransPr,'N') = 'N'
                ";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int Apply_UpdateMeDocd2(ME_DOCM me_docm)
        {
            var sql = @"update ME_DOCD 
                set pick_qty = 0, ackqty = 0, apvqty = 0, EXPT_DISTQTY=0, dis_user = :UPDATE_USER, 
                dis_time = sysdate, apl_contime = sysdate, apvid = :UPDATE_USER, apvtime = sysdate, 
                update_time = sysdate, update_user = :UPDATE_USER, update_ip = :UPDATE_IP
                where docno = :DOCNO
                and nvl(isTransPr,'N') = 'Y'
                ";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public SP_MODEL Apply_PostDoc(string docno, string updusr, string updip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: docno, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: updusr, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };
            return sp;
        }

        public string GetVDocno(string id)
        {
            string sql = @"select towh||twn_systime||mat_class from ME_DOCM where docno = :DOCNO";
            return DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
        }

        public int Apply_InsertMeDocm(ME_DOCM me_docm)
        {
            var sql = @"insert into ME_DOCM (docno, doctype, flowid, appid, appdept, apptime, useId, useDept, 
                frwh, towh, apply_kind, apply_note, mat_class, 
                create_time, create_user, update_time, update_user, update_ip, isContid3, srcdocno, 
                sendapvid, sendapvtime, sendapvdept)
                select :ITEM_STRING as docno, a.doctype as doctype, '2', a.appid, a.appdept, a.apptime,a.useId, a.useDept, 
                a.frwh, a.towh, a.apply_kind, ('轉申購產生新單據 '||a.apply_note) as apply_note, a.mat_class, 
                sysdate as create_time, :UPDATE_USER as create_user, sysdate as update_time, :UPDATE_USER as update_user, :UPDATE_IP as update_ip, a.isContid3 as isContid3, a.docno as srcdocno,
                :UPDATE_USER as sendapvid, a.sendapvtime, (select INID FROM UR_ID where TUSER=:UPDATE_USER) as sendapvdept
                from ME_DOCM a where a.docno = :DOCNO
                ";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int Apply_InsertMeDocd(ME_DOCM me_docm)
        {
            var sql = @"insert into ME_DOCD(docno, seq, mmcode, appqty, avg_price, aplyitem_note, gtapl_reson, 
                m_contprice, uprice, disc_cprice, disc_uprice, m_nhikey, 
                create_time, create_user, update_time, update_user, update_ip, 
                isTransPr)
                select :ITEM_STRING as docno, a.seq, a.mmcode, a.appqty, a.avg_price, a.aplyitem_note, a.gtapl_reson, 
                a.m_contprice, a.uprice, a.disc_cprice, a.disc_uprice, a.m_nhikey, 
                sysdate as create_time, :UPDATE_USER as create_user, sysdate as update_time, :UPDATE_USER as update_user, :UPDATE_IP as update_ip,
                'N' as isTransPr
                from ME_DOCD a 
                where a.docno = :DOCNO and nvl(a.isTransPr,'N') = 'Y'
                ";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int Apply_UpdateMeDocdReason(ME_DOCM me_docm)
        {
            var sql = @"update ME_DOCD
                set short_reason = '進貨待撥，新單號'|| :ITEM_STRING
                where docno = :DOCNO and nvl(isTransPr,'N') = 'Y'
                ";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<string> GetMatClassList(string[] str_DOCNO)
        {
            string sql = @"SELECT DISTINCT MAT_CLASS FROM ME_DOCM A WHERE DOCNO in :DOCNO order by MAT_CLASS ";

            return DBWork.Connection.Query<string>(sql, new { DOCNO = str_DOCNO }, DBWork.Transaction);
        }

        public string GetVPrno(string id)
        {
            string sql = @"select whno_mm1||twn_systime||:MATCLASS from dual";
            return DBWork.Connection.ExecuteScalar(sql, new { MATCLASS = id }, DBWork.Transaction).ToString();
        }

        public bool ChkPrNo(string id)
        {
            string sql = @"select 1 from MM_PR_M where pr_no = :PR_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = id }, DBWork.Transaction) == null);
        }

        public int Apply_InsertPrM(string pr_no, string matclass, string username, string userip)
        {
            var sql = @"insert into MM_PR_M(pr_no, pr_dept, pr_time, pr_user, mat_class, m_storeid, pr_status, 
                create_time, create_user, update_ip, xaction)
                values (:PR_NO, user_inid(:CREATE_USER), sysdate, :CREATE_USER, :MAT_CLASS, '0', '35',
                sysdate, :CREATE_USER, :UPDATE_IP, '0' )
                ";
            return DBWork.Connection.Execute(sql, new { PR_NO = pr_no , MAT_CLASS = matclass , CREATE_USER = username , UPDATE_IP = userip }, DBWork.Transaction);
        }

        public int Apply_InsertPrD(string[] docno, string pr_no, string matclass, string username, string userip)
        {
            var sql = @"insert into MM_PR_D(pr_no, mmcode, m_contid, 
                m_contprice, m_purun, pr_price, 
                pr_qty, req_qty_t, unit_swap, 
                agen_fax, agen_name, agen_no, 
                disc, is_email, rec_status, m_nhikey,
                create_time, create_user, update_time, update_user, update_ip, src_pr_qty)
                select :PR_NO as pr_no, a.mmcode as mmcode, b.m_contid as m_contid,
                b.m_contprice as m_contprice, b.m_purun as m_purun, b.uprice as pr_price, 
                sum(a.appqty) as pr_qty, 
                (case when ceil((sum(a.appqty)/nvl(c.exch_ratio,1)))=0 then 1 else ceil((sum(a.appqty)/nvl(c.exch_ratio,1))) end) as req_qty_t, 
                nvl(c.exch_ratio,1) as unit_swap, 
                d.agen_fax as agen_fax, d.agen_namec as agen_name, b.m_agenno as agen_no, 
                b.m_discperc as disc, 'N' as isEmail, '35' as rec_status, b.m_nhikey as m_nhikey,
                sysdate as create_time, :CREATE_USER as create_user, sysdate as update_time, :CREATE_USER as update_user, :UPDATE_IP as update_ip,
                sum(a.appqty) as src_pr_qty
                from ME_DOCM e, ME_DOCD a, MI_MAST b, MI_UNITEXCH c, PH_VENDER d
                where e.docno in :DOCNO
                and e.mat_class = :MAT_CLASS
                and a.docno = e.docno
                and b.mmcode = a.mmcode
                and c.mmcode = a.mmcode
                and c.agen_no = b.m_agenno
                and c.unit_code = b.m_purun
                and d.agen_no = b.m_agenno
               group by a.mmcode, b.m_contid, b.m_contprice, b.m_purun, b.uprice, c.exch_ratio, d.agen_fax, 
                d.agen_namec, b.m_agenno, b.m_discperc, b.m_nhikey
                ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, PR_NO = pr_no, MAT_CLASS = matclass, CREATE_USER = username, UPDATE_IP = userip }, DBWork.Transaction);
        }

        public int ApplyX(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = '1',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);

        }

        public int UpdateMeDocd(ME_DOCD me_docd)
        {
            var sql = @"update ME_DOCD set BW_MQTY = nvl(trim(:BW_MQTY), '0'), EXPT_DISTQTY = nvl(trim(:EXPT_DISTQTY), '0'), 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public string CheckOrderDcFlag(ME_DOCD docno)
        {
            string sql = @"select (case when nvl(e_orderdcflag,'N')='Y' then 'Y' else 'N' end)
                from MI_MAST 
                where mmcode = (select mmcode from ME_DOCD where docno = :DOCNO and seq = :SEQ)
                ";
            return DBWork.Connection.ExecuteScalar(sql, docno, DBWork.Transaction).ToString();
        }

        public string CheckParcode(ME_DOCD docno)
        {
            string sql = @"select (case when nvl(e_parcode,'0')='2' then 'Y' else 'N' end) 
                from MI_MAST 
                where mmcode = (select mmcode from ME_DOCD where docno = :DOCNO and seq = :SEQ)
                ";
            return DBWork.Connection.ExecuteScalar(sql, docno, DBWork.Transaction).ToString();
        }

        public int TransConfirmMeDocd(ME_DOCD me_docd)
        {
            var sql = @"update ME_DOCD set isTransPr='Y' where docno = :DOCNO and seq = :SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int TransCancelMeDocd(ME_DOCD me_docd)
        {
            var sql = @"update ME_DOCD set isTransPr='N' where docno = :DOCNO and seq = :SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
    }
}