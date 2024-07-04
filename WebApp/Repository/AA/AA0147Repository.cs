using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;


namespace WebApp.Repository.AA
{
    public class AA0147Report_MODEL : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string APPID { get; set; }
        public string APPDEPT { get; set; }
        public string MMCODE { get; set; }
        public Int32 APPQTY { get; set; }
        public Int32 APVQTY { get; set; }
        public Int32 EXPT_DISTQTY { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string APPDEPT_N { get; set; }
        public string APPID_N { get; set; }
        public string STORELOC { get; set; }
        public string APPTIME_T { get; set; }
        public string POST_TIME_T { get; set; }
        public string FRWH_N { get; set; }
        public string APPLY_NOTE { get; set; }
        public string FRWHINV_QTY { get; set; }
        public string TOWHINV_QTY { get; set; }
        public string ISARMY_N { get; set; }
        public string MAX_EXP_DATE { get; set; }
        public string SHORT_REASON { get; set; }
        public string CHINNAME { get; set; }
    }
    public class AA0147Repository : JCLib.Mvc.BaseRepository
    {
        public AA0147Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string apptime1, string apptime2, string appdept, string[] str_FLOWID, string[] str_matclass, string strMMCode, string strDocNo, string hasIngqty, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (case when a.mat_class = '01' then (select a.flowId || ' 藥品' || flowName from ME_FLOW where doctype in ('MR', 'MS') and flowId = a.flowId)   
                         else (select DATA_VALUE || ' 衛材' || DATA_DESC from PARAM_D 
                                   where grp_code = 'ME_DOCM' and data_name = 'FLOWID_MR1' and a.flowid = data_value) end) FLOWID_N,  
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N ,
                        TWN_DATE(A.APPTIME) APPTIME_T,
                        (case when a.docno = nvl(srcdocno, docno)then 'N' else 'Y' end) SRCDOCYN ,
                       (case when (select count(*) from ME_DOCD c, MI_MAST d where c.docno = a.docno and c.mmcode = d.mmcode and d.wexp_id = 'Y') > 0 
                                 then 'Y' else 'N' end) as wexp_yn,
                       (select count(*) from ME_DOCD where DOCNO=A.DOCNO and POSTID='A') as POSTID 
                        FROM ME_DOCM A ,MI_MATCLASS B 
                        WHERE A.MAT_CLASS=B.MAT_CLASS 
                        AND A.DOCTYPE IN ('MR','MS','MR5','MR6') 
                        AND A.FRWH in (select wh_no from MI_WHID where wh_userId = :tuser) ";
            p.Add(":tuser", string.Format("{0}", tuser));

            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", apptime1));
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (appdept != "")
            {
                sql += " AND A.TOWH = :p2 ";
                p.Add(":p2", string.Format("{0}", appdept));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_FLOWID.Length > 0)
            {
                sql += @" AND A.FLOWID in :flowids ";
                p.Add(":flowids", str_FLOWID);
            }

            //判斷MAT_CLASS查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_matclass.Length > 0)
            {
                string sql_matclass = "";
                sql += @" AND (";
                foreach (string tmp_matclass in str_matclass)
                {
                    if (string.IsNullOrEmpty(sql_matclass))
                    {
                        if (tmp_matclass.Contains("SUB_"))
                            sql_matclass = @"(select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = '" + tmp_matclass.Replace("SUB_", "") + "') > 0 ";
                        else
                            sql_matclass = @" A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                    else
                    {
                        if (tmp_matclass.Contains("SUB_"))
                            sql_matclass += @" OR (select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = '" + tmp_matclass.Replace("SUB_", "") + "') > 0 ";
                        else
                            sql_matclass += @" OR A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                }
                sql += sql_matclass + ") ";
            }

            if (!String.IsNullOrEmpty(strMMCode))
            {
                sql += " and (select count(*) from ME_DOCD where DOCNO=A.DOCNO and MMCODE=:mmcode)>0";
                p.Add(":mmcode", strMMCode);
            }

            if (!String.IsNullOrEmpty(strDocNo))
            {
                sql += " and a.docno=:docno";
                p.Add(":docno", strDocNo);
            }
            if (hasIngqty == "true")
            {
                sql += " and (select count(*) from ME_DOCD where DOCNO=A.DOCNO and (INV_QTY(A.FRWH,MMCODE)-APVQTY)>=0)>0";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, A.EXPT_DISTQTY,A.APVQTY, TWN_TIME(a.apl_contime) as apl_contime,
                         B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.DISC_UPRICE,
                       (case when (trim(A.POSTID) is null or A.POSTID='A') then INV_QTY(C.FRWH,A.MMCODE) else A.S_INV_QTY end) as S_INV_QTY, 
                       (case when (trim(A.POSTID) is null or A.POSTID='A') then INV_QTY(C.TOWH,A.MMCODE) else A.INV_QTY end) as INV_QTY,
                        C.FLOWID,
                        (case when nvl( a.isTransPr,'N' ) = 'Y' then 'V' else ' ' end) ISTRANSPR, SHORT_REASON , A.CHINNAME, A.CHARTNO,
                       (case when A.POSTID IS NULL then '待核可' 
                        else (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'ME_DOCD' and DATA_NAME = 'POSTID' and DATA_VALUE = A.POSTID) end) as POSTID,
                        (select UNA FROM UR_ID where TUSER=A.DIS_USER) as DIS_USER, TWN_TIME(A.DIS_TIME) as DIS_TIME,
                        (select UNA FROM UR_ID where TUSER=A.APVID) as APVID, TWN_TIME(A.APVTIME) as APVTIME

                        FROM ME_DOCD A
                        INNER JOIN MI_MAST B ON B.MMCODE = A.MMCODE 
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        LEFT OUTER JOIN V_MM_TOTAPL2 E ON E.DATE_YM=SUBSTR(TWN_DATE((case when c.post_time is null then C.APPTIME else C.post_time end)),0,5) and E.MMCODE=A.MMCODE  and E.TOWH = C.TOWH
                        WHERE  1 = 1 and trim(A.POSTID) is not null ";

            if (DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", DOCNO));
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
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int ApplyDN(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD 
                        SET EXPT_DISTQTY = nvl(trim(APVQTY), '0'), 
                            BW_MQTY = 0, 
                            PICK_QTY = nvl(trim(APVQTY), '0'),
                            ACKQTY =  nvl(trim(APVQTY), '0'),
                            DIS_USER = :UPDATE_USER, APVID = :UPDATE_USER, 
                            DIS_TIME = SYSDATE,  APVTIME = SYSDATE, 
                            postid =  '3',
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyDMRMS(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD 
                        SET postid ='4',  UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO and postid = 'C' ";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = 
                        (case   when doctype in ('MR1','MR2') and flowid='2' then '6' 
                                when doctype ='MR' and flowid = '0102' then '0199' 
                                when doctype ='MS' and flowid = '0602' then '0699' end),
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public bool CheckExistsM2M4(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND to_number(substr(FLOWID, length(FLOWID)-1, 2)) not in (2,3,4)";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public string CallProc(string id, string upuser, string upip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: upuser, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: upip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            if (retid == "N")
            {
                retid = errmsg;
            }
            return retid;
        }
        public string GetDoctype(string id)
        {
            string sql = @"SELECT DOCTYPE FROM ME_DOCM where DOCNO = :DOCNO ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction));
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo(string id)
        {
            string sql = @"
                with temp_whkinds as (
                     select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id, b.wh_grade
                       from MI_WHID a, MI_WHMAST b
                      where wh_userid = :TUSER
                        and a.wh_no = b.wh_no
                        and ((b.wh_grade in ('1','2') and b.wh_kind ='0')
                                  or (b.wh_grade ='1' and b.wh_kind ='1'))
                ), temp_mat_class as (
                     select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM 
                       from temp_whkinds a, MI_MATCLASS b
                      where (a.task_id = b.mat_clsid)
                     union
                     select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM  
                       from temp_whkinds a, MI_MATCLASS b
                      where (a.task_id = '2')
                            and b.mat_clsid = '3'
                            )
                select p.* from(
                    select b.flowid as VALUE, '藥品'||b.flowName as text,flowId || ' 藥品' || flowName as COMBITEM,
                                (case when to_number(substr(b.flowid, length(b.flowid)-1,2)) in (2,3,4) then 'Y' else 'N' end) as extra1,
                               (case when to_number(substr(b.flowid, length(b.flowid)-1,2))=11 then 1.1 
                                          when to_number(substr(b.flowid, length(b.flowid)-1,2))=51 then 5.1
                                 else to_number(substr(b.flowid, length(b.flowid)-1,2)) end) orderby
                       from temp_mat_class a, temp_whkinds c, ME_FLOW b
                     where a.value = '01' 
                        and (b.doctype in ('MR') and c.wh_grade = '1') 
                        and b.flowid not in ('0100', '0104')
                    union
                    select DISTINCT DATA_VALUE as VALUE, '衛材'||DATA_DESC as TEXT,DATA_VALUE || ' 衛材' || DATA_DESC as COMBITEM,
                                (case when to_number(substr(b.data_value, length(b.data_value)-1,2)) in (2,3,4) then 'Y' else 'N' end) as extra1,
                                (case when to_number(substr(b.data_value, length(b.data_value)-1,2))=11 then 1.1 
                                          when to_number(substr(b.data_value, length(b.data_value)-1,2))=51 then 5.1
                                  else to_number(substr(b.data_value, length(b.data_value)-1,2)) end) orderby
                        from temp_mat_class a, PARAM_D b
                      where a.value in ('02', '03', '04', '05', '06', '07', '08') 
                          and b.DATA_VALUE not in ('5', '51')
                          and b.GRP_CODE='ME_DOCM' AND b.DATA_NAME='FLOWID_MR1'
                   )p order by substr(COMBITEM,1,2), orderby  ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetApplyKindCombo(string id)
        {
            string sql = @"with temp_whkinds as (
                            select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id
                            from MI_WHID a, MI_WHMAST b
                            where wh_userid = :TUSER
                            and a.wh_no = b.wh_no
                            and ((b.wh_grade in ('1','2') and a.task_id ='1')
                                  or (b.wh_grade ='1' and a.task_id in ('2','3')))
                            ), temp_mat_class as (
                            select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM 
                            from temp_whkinds a, MI_MATCLASS b
                            where (a.task_id = b.mat_clsid)
                            union
                            select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM  
                            from temp_whkinds a, MI_MATCLASS b
                            where (a.task_id = '2')
                            and b.mat_clsid = '3'
                            )
                            select b.flowid as value, '藥品'||b.flowName as text,flowId || ' 藥品' || flowName as COMBITEM
                            from temp_mat_class a, ME_FLOW b
                            where a.value = '01' 
                            and b.doctype in ('MS')
                            union
                            select DISTINCT DATA_VALUE as VALUE, '衛材'||DATA_DESC as TEXT,DATA_VALUE || ' 衛材' || DATA_DESC as COMBITEM 
                            from temp_mat_class a, PARAM_D b
                            where a.value in ('02', '03', '04', '05', '06', '07', '08') 
                            and b.GRP_CODE='ME_DOCM' AND b.DATA_NAME='FLOWID_MR1'
                            ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetAppDeptCombo(string id)
        {
            var p = new DynamicParameters();

            string sql = @"with temp_grade1_kind0 as (
                                  select distinct wh_grade, wh_kind 
                                    from MI_WHID a, MI_WHMAST b
                                   where a.wh_userid = :TUSER
                                and a.wh_no = b.wh_no
                                and b.wh_grade in ('1', '2')
                                and b.wh_kind = '0'
                                ), temp_grade2_kind0 as (
                                select distinct wh_grade, wh_kind  
                                    from MI_WHID a, MI_WHMAST b
                                   where a.wh_userid = :TUSER
                                and a.wh_no = b.wh_no
                                and b.wh_grade = '2'
                                and b.wh_kind = '0'
                                ), temp_grade1_kind1 as (
                                select distinct wh_grade, wh_kind  
                                    from MI_WHID a, MI_WHMAST b
                                   where a.wh_userid = :TUSER
                                and a.wh_no = b.wh_no
                                and b.wh_grade = '1'
                                and b.wh_kind = '1'
                                )
                                select b.wh_no as value, b.wh_name as text, b.wh_no||' '||b.wh_name as combitem 
                                from temp_grade1_kind0 a, MI_WHMAST b
                                where a.wh_kind = b.wh_kind
                                and a.wh_grade < b.wh_grade
                                and nvl(b.cancel_id, 'N') = 'N'
                                and b.wh_grade not in ('5','M','S')
                                union
                                select b.wh_no as value, b.wh_name as text, b.wh_no||' '||b.wh_name as combitem 
                                from temp_grade2_kind0 a, MI_WHMAST b
                                where a.wh_kind = b.wh_kind
                                and a.wh_grade < b.wh_grade
                                and nvl(b.cancel_id, 'N') = 'N'
                                and b.wh_grade not in ('5','M','S')
                                union
                                select b.wh_no as value, b.wh_name as text, b.wh_no||' '||b.wh_name as combitem  
                                from temp_grade1_kind1 a, MI_WHMAST b
                                where a.wh_kind = b.wh_kind
                                and a.wh_grade < b.wh_grade
                                and nvl(b.cancel_id, 'N') = 'N'
                                and b.wh_grade not in ('5','M','S')
                              ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
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
        public IEnumerable<COMBO_MODEL> GetTowhCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM 
                        FROM MI_WHMAST A,UR_ID B
                        WHERE A.INID=B.INID 
                        AND A.WH_KIND = '1' 
                        AND TUSER=:TUSER 
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string id)
        {
            string sql = @"  with temp_whkinds as (
                            select b.wh_no, b.wh_kind, 
                                             nvl((case when b.wh_kind = '0' then '1' else '2' end), '2') as task_id
                            from MI_WHID a, MI_WHMAST b
                            where wh_userid = :TUSER
                                       and a.wh_no = b.wh_no
                                       and ((b.wh_grade in ('1', '2') and b.wh_kind = '0')
                                            or (b.wh_grade = '1' and b.wh_kind = '1')
                                           )
                            )
                            select distinct b.mat_class as value,
                                         '全部'||b.mat_clsname as text,
                                         b.mat_class || ' ' ||  '全部'||b.mat_clsname as COMBITEM 
                                  from temp_whkinds a, MI_MATCLASS b
                                 where (a.task_id = b.mat_clsid)
                                union
                                select 'SUB_' || b.data_value as value, b.data_desc as text,
                                b.data_value || ' ' || b.data_desc as COMBITEM 
                                from temp_whkinds a, PARAM_D b
	                             where b.grp_code ='MI_MAST' 
	                               and b.data_name = 'MAT_CLASS_SUB'
	                               and b.data_value = '1'
	                               and trim(b.data_desc) is not null
                                   and (a.task_id = '1')
                                union
                                select 'SUB_' || b.data_value as value, b.data_desc as text,
                                b.data_value || ' ' || b.data_desc as COMBITEM 
                                from temp_whkinds a, PARAM_D b
	                             where b.grp_code ='MI_MAST' 
	                               and b.data_name = 'MAT_CLASS_SUB'
	                               and b.data_value <> '1'
	                               and trim(b.data_desc) is not null
                                   and (a.task_id = '2')
                            ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                     FROM  MI_MAST a
                                    WHERE  1 = 1
                                      AND  nvl(cancel_id, 'N') = 'N'
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (UPPER(MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        #region 2020-05-06 檢核需對應到院內碼
        public IEnumerable<ME_DOCD> GetAllDocds(string docno)
        {
            string sql = "select * from ME_DOCD where docno = :docno";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno = docno }, DBWork.Transaction);
        }
        public bool CheckExptQtyMmcode(string docno, string mmcode)
        {
            string sql = @"SELECT 1 FROM ME_DOCD A,ME_DOCM C
                            WHERE A.DOCNO=C.DOCNO AND A.DOCNO=:DOCNO and a.mmcode = :mmcode
                            AND EXPT_DISTQTY > INV_QTY( C.FRWH,A.MMCODE ) ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, mmcode = mmcode }, DBWork.Transaction) == null);
        }
        public int ChkNotIsTransPr(string docno)
        {
            string sql = @"SELECT count(*) FROM ME_DOCD WHERE DOCNO=:DOCNO AND nvl(isTransPr,'N') = 'N' ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        #endregion

        #region 配合批號效期先進先出管制, 增加批號效期及儲位出入庫控制
        public int ChkHasExp(string[] docno)
        {
            string sql = @"SELECT count(*) FROM ME_DOCM A, ME_DOCD B, MI_WEXPINV C WHERE A.DOCNO = B.DOCNO
                        and A.FRWH = C.WH_NO and B.MMCODE = C.MMCODE and C.INV_QTY > 0
                        and nvl(B.POSTID,' ')='A'  and A.DOCNO in :DOCNO ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetExpList(string[] DOCNO, string chkCol, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            //1130229 撥發時抓儲位要篩選有庫存量,否則會將無庫存量的儲位檔扣成負數
            var sql = @"select A.DOCNO, B.MMCODE,
                        (select MMNAME_C from MI_MAST where MMCODE = B.MMCODE) as MMNAME_C, B." + chkCol + @" as APVQTY,
                        C.LOT_NO, TWN_DATE(C.EXP_DATE) as EXPDATE, C.INV_QTY, 0 as APPQTY,
                        (select STORE_LOC from MI_WLOCINV where WH_NO = A.FRWH and MMCODE = B.MMCODE and INV_QTY>0 and rownum = 1) as STORE_LOC,
                        (case when C.EXP_DATE - sysdate <= 180 then 'Y' else 'N' end) as UP,
                        A.FRWH, A.TOWH, APPQTY as APVQTY_C
                        from ME_DOCM A, ME_DOCD B, MI_WEXPINV C
                        where A.DOCNO = B.DOCNO and A.FRWH = C.WH_NO and B.MMCODE = C.MMCODE 
                        and C.INV_QTY > 0  and nvl(B.POSTID,' ')='A'   ";

            if (DOCNO.Length > 0)
            {
                sql += " AND A.DOCNO in :p0 ";
                p.Add(":p0", DOCNO);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public bool CheckWlocExists(string wh_no, string mmcode, string store_loc)
        {
            string sql = @"SELECT 1 FROM MI_WLOCINV WHERE WH_NO=:WH_NO AND MMCODE = :MMCODE AND STORE_LOC=:STORE_LOC";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode, STORE_LOC = store_loc }, DBWork.Transaction) == null);
        }

        public bool CheckWexpExists(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string sql = @"SELECT 1 FROM MI_WEXPINV WHERE WH_NO=:WH_NO AND MMCODE = :MMCODE and LOT_NO = :LOT_NO and TWN_DATE(EXP_DATE) = :EXP_DATE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date }, DBWork.Transaction) == null);
        }
        public int InsertWloc(string wh_no, string mmcode, string store_loc, string invqty, string tuser, string userIp)
        {
            string sql = string.Format(@" insert into MI_WLOCINV (WH_NO, MMCODE, STORE_LOC, INV_QTY, CREATE_USER, CREATE_TIME, UPDATE_IP)
                values(:WH_NO, :MMCODE, :STORE_LOC, :INVQTY, :TUSER, sysdate, :USERIP)
                ");
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, STORE_LOC = store_loc, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }
        // 入庫(無store_loc)則增加數量
        public int UpdateWloc(string wh_no, string mmcode, string invqty, string tuser, string userIp)
        {
            string sql = @" update MI_WLOCINV A
                set INV_QTY = INV_QTY + :INVQTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE 
                and STORE_LOC = (select STORE_LOC from MI_WLOCINV where WH_NO = A.WH_NO and MMCODE = A.MMCODE and rownum = 1)
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        // 出庫(有store_loc)則扣除數量
        public int UpdateWloc(string wh_no, string mmcode, string store_loc, string invqty, string procSign, string tuser, string userIp)
        {
            string sql = @" update MI_WLOCINV A
                set INV_QTY = INV_QTY" + procSign + @"  :INVQTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE 
                and STORE_LOC = :STORE_LOC
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, INVQTY = invqty, STORE_LOC = store_loc, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int InsertWexp(string wh_no, string mmcode, string lot_no, string exp_date, string invqty, string tuser, string userIp)
        {
            string sql = string.Format(@" insert into MI_WEXPINV (WH_NO, MMCODE, LOT_NO, EXP_DATE, INV_QTY, CREATE_USER, CREATE_TIME, UPDATE_IP)
                values(:WH_NO, :MMCODE, :LOT_NO, twn_todate(:EXP_DATE), :INVQTY, :TUSER, sysdate, :USERIP)
                ");
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int UpdateWexp(string wh_no, string mmcode, string lot_no, string exp_date, string invqty, string procSign, string tuser, string userIp)
        {
            string sql = @" update MI_WEXPINV
                set INV_QTY = INV_QTY " + procSign + @" :INVQTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE and LOT_NO = :LOT_NO and EXP_DATE = twn_todate(:EXP_DATE)
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }
        public int ChkHasExpDetail(string docno, string seq)
        {
            string sql = @"SELECT count(*) FROM ME_DOCM A, ME_DOCD B, MI_WEXPINV C WHERE A.DOCNO = B.DOCNO
                        and A.FRWH = C.WH_NO and B.MMCODE = C.MMCODE and C.INV_QTY > 0 and A.DOCNO in :DOCNO and B.SEQ in :SEQ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetExpListDetail(string[] DOCNO, string[] SEQ, string chkCol, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.DOCNO, B.MMCODE,
                        (select MMNAME_C from MI_MAST where MMCODE = B.MMCODE) as MMNAME_C, B." + chkCol + @" as APVQTY,
                        C.LOT_NO, TWN_DATE(C.EXP_DATE) as EXPDATE, C.INV_QTY, 0 as APPQTY,
                        (select STORE_LOC from MI_WLOCINV where WH_NO = A.FRWH and MMCODE = B.MMCODE and rownum = 1) as STORE_LOC,
                        (case when C.EXP_DATE - sysdate <= 180 then 'Y' else 'N' end) as UP,
                        A.FRWH, A.TOWH, B." + chkCol + @" as APVQTY_C
                        from ME_DOCM A, ME_DOCD B, MI_WEXPINV C
                        where A.DOCNO = B.DOCNO
                        and A.FRWH = C.WH_NO and B.MMCODE = C.MMCODE and C.INV_QTY > 0 ";

            if (DOCNO.Length > 0)
            {
                sql += " AND A.DOCNO in :p0 ";
                p.Add(":p0", DOCNO);
            }
            if (SEQ.Length > 0)
            {
                sql += " and B.SEQ in :p1";
                p.Add(":p1", SEQ);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        #endregion

        public IEnumerable<AA0147Report_MODEL> GetPrintData(string[] str_DOCNO, string UserName, string hospCode, string order)
        {
            var p = new DynamicParameters();
          
            string sql = @"SELECT A.DOCNO, A.APPID, A.APPDEPT, B.MMCODE, B.APPQTY, B.APVQTY, B.EXPT_DISTQTY,
                         B.APLYITEM_NOTE, C.MMNAME_C, C.MMNAME_E, C.BASE_UNIT
                        ,(SELECT RTrim(WH_NO || ' ' || WH_NAME) FROM MI_WHMAST WHERE WH_NO = A.TOWH) AS APPDEPT_N                        
                        ,(SELECT UNA FROM UR_ID WHERE A.APPID = TUSER) AS APPID_N
                        ,GET_STORELOC(A.FRWH, B.MMCODE)AS STORELOC 
                        ,TWN_DATE(A.APPTIME) APPTIME_T
                        ,(case when trim(A.POST_TIME) is not null then TWN_DATE(A.POST_TIME) else null end) as POST_TIME_T
                        ,(SELECT RTrim(WH_NO || ' ' || WH_NAME) FROM MI_WHMAST WHERE WH_NO = A.FRWH) AS FRWH_N 
                        ,A.APPLY_NOTE
                        ,(case when B.POSTID='D' then NVL(INV_QTY(A.FRWH,B.MMCODE),0)
                                       else NVL(INV_QTY(A.FRWH,B.MMCODE),0)-B.APVQTY end) as FRWHINV_QTY
                        ,NVL(INV_QTY(A.TOWH,B.MMCODE),0)  as TOWHINV_QTY
                        ,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D 
                          WHERE GRP_CODE='ME_DOCM' and DATA_NAME='ISARMY' AND DATA_VALUE = A.ISARMY) as ISARMY_N
                        , (select TWN_DATE(max(EXP_DATE)) from MI_WEXPINV 
                           where WH_NO=A.FRWH and MMCODE=B.MMCODE)  as MAX_EXP_DATE
                        , (case when B.POSTID='D' and B.APVQTY>0 then '已撥發;' 
                                     when B.POSTID='D' and B.APVQTY=0 then '不核撥;'
                                     when B.POSTID='A' then '進貨待撥;' end)||B.SHORT_REASON as SHORT_REASON, B.CHINNAME
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C
                        WHERE A.DOCNO = B.DOCNO AND B.MMCODE = C.MMCODE  and B.APVQTY>0";
            //1130204 北投列印只列出當次欲撥發品項
            if (hospCode == "818")
                sql += " and B.POSTID=(case when (select count(*) from ME_DOCD  where DOCNO=A.DOCNO and POSTID= 'A')>0 then 'A'  else 'D' end)";

            p.Add(":PRINT_USER", UserName);

            //判斷DOCNO查詢條件是否有值
            if (str_DOCNO.Length > 0)
            {
                sql += @" AND A.DOCNO IN :DOCNO ";
                p.Add("DOCNO", str_DOCNO);
            }
            sql += " ORDER BY  A.DOCNO, STORELOC ";

            //先將查詢結果暫存在tmp_AB0118Report_MODEL，接著產生BarCode的資料
            IEnumerable<AA0147Report_MODEL> tmp_AA0147Report_MODEL = DBWork.Connection.Query<AA0147Report_MODEL>(sql, p);
            //return DBWork.Connection.Query<AB0118Report_MODEL>(sql, p);
            return tmp_AA0147Report_MODEL;
        }
        public bool CheckWhidValid(string docno, string userId)
        {
            string sql = @"
                select 1 from ME_DOCM a
                 where a.docno = :docno
                   and exists (select 1 from MI_WHID
                                where wh_userId = :userId
                                  and wh_no = a.frwh)
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno, userId }, DBWork.Transaction) != null;
        }
        public int UpdatePostid(ME_DOCD me_docd)
        {
            string cond_postid = "";
            if (me_docd.POSTID == "3")
            {
                cond_postid = " and NVL(POSTID, ' ')='A' ";
            }
            else if (me_docd.POSTID == "4")
            {
                cond_postid = " and NVL(POSTID, ' ')='C' ";
            }
            var sql = @"UPDATE ME_DOCD SET POSTID = :POSTID, APVTIME=SYSDATE, APVID=:UPDATE_USER, PICK_QTY=APVQTY,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO=:DOCNO AND SEQ=:SEQ " + cond_postid;
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public int defAckqty(string docno)
        {
            var sql = @"UPDATE ME_DOCD SET ACKQTY=APVQTY
                                WHERE DOCNO=:DOCNO ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int defAckqty(string docno, string seq)
        {
            var sql = @"UPDATE ME_DOCD SET ACKQTY=APVQTY
                                WHERE DOCNO=:DOCNO  AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public int UpdateMeDocd(ME_DOCD ME_DOCD)
        {
            var sql = @"update ME_DOCD set 
                                        EXPT_DISTQTY = nvl(trim(:APVQTY), '0'), 
                                        APVQTY = nvl(trim(:APVQTY), '0'), 
                                        S_INV_QTY=INV_QTY((select FRWH from ME_DOCM where DOCNO=:DOCNO),MMCODE),
                                        INV_QTY=INV_QTY((select TOWH from ME_DOCM where DOCNO=:DOCNO),MMCODE),
                                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                    where DOCNO = :DOCNO and SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetDocds(string docno, string seq)
        {
            string sql = "select * from ME_DOCD where docno = :docno and seq = :seq";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno = docno, seq = seq }, DBWork.Transaction);
        }
        public bool ChkPostIdNotA(string docno, string seq)
        {
            string sql = @"select 1 from ME_DOCD 
                                       where DOCNO = :DOCNO and SEQ=:SEQ and (nvl(POSTID,' ') <>'A' ) ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction) == null);
        }
        public int InsertMEDOCD(ME_DOCD ME_DOCD)
        {
            var sql = @"insert into ME_DOCD (
                            docno, seq, mmcode, appqty, apvqty, expt_distqty, apl_contime,  aplyitem_note, gtapl_reson,
                            create_time, create_user, update_time, update_user, update_ip, frwh_d,s_inv_qty, inv_qty, oper_qty, 
                             postid, avg_price,m_agenno,  srcdocno,m_contprice, uprice, disc_cprice, disc_uprice, m_nhikey,
                             isTransPr,CHINNAME, CHARTNO, pr_qty, dis_user, dis_time)
                        select a.docno, (select max(SEQ)+1  from ME_DOCD where DOCNO=:DOCNO), a.mmcode,
                            (a.appqty-a.apvqty) as appqty, (a.appqty-a.apvqty) as apvqty, (a.appqty-a.apvqty) as expt_distqty,
                            a.apl_contime,  a.aplyitem_note, a.gtapl_reson, sysdate as create_time, :UPDATE_USER as create_user, 
                            sysdate as update_time,  :UPDATE_USER as update_user, :UPDATE_IP as update_ip, a.frwh_d,
                             INV_QTY((select FRWH from ME_DOCM where DOCNO=a.DOCNO),a.MMCODE)  as s_inv_qty,
                             INV_QTY((select TOWH from ME_DOCM where DOCNO=a.DOCNO),a.MMCODE)  as inv_qty,
                            a. oper_qty, 'A' as postid,a.avg_price,a.m_agenno,a. srcdocno, a.m_contprice, a.uprice, 
                            a.disc_cprice,  a.disc_uprice, a.m_nhikey,a.isTransPr,a.CHINNAME, a.CHARTNO, a.pr_qty, a.dis_user, a.dis_time
                        from ME_DOCD a
                       where  a.DOCNO = :DOCNO  and a.SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int UpdateAppqty(ME_DOCD me_docd)
        {
            var sql = @"update ME_DOCD set APPQTY=:APPQTY,
                                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                    where DOCNO=:DOCNO AND SEQ=:SEQ ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int ChkApvQtyDetail(string docno, string seq)
        {
            string sql = @"select count(*) from ME_DOCM A, ME_DOCD B
                                        where A.DOCNO = B.DOCNO and A.DOCNO in :DOCNO and B.SEQ in :SEQ and B.APVQTY<=0";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        :UPDATE_IP,
                        (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode') as HOSP_CODE,
                        (case when (select count(*) from UR_UIR where RLNO in ('MAT_14') and TUSER = :TUSER) > 0 then 'Y' else 'N' end) as IS_GRADE1
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }

        public IEnumerable<ME_DOCD> GetDocdsInv(string docno)
        {
            string sql = @"select * from ME_DOCD 
                                       where docno = :docno and POSTID='A'  and APVQTY>0
                                           and INV_QTY(FRWH_D,MMCODE)>0
                                           and INV_QTY(FRWH_D,MMCODE)>=APVQTY";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno = docno }, DBWork.Transaction);
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}
