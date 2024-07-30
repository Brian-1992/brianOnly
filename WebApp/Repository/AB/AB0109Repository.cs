using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApp.Models.AB;

namespace WebApp.Repository.AB
{
    public class AB0109Repository : JCLib.Mvc.BaseRepository
    {
        public AB0109Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region GetComboBox
        public IEnumerable<AB0109> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} MMCODE,MMNAME_C,MMNAME_E,BASE_UNIT,          
                                  (case when (M_APPLYID='E' and M_CONTID='3') then UPRICE
                                     else (case when (M_CONTPRICE is null or M_CONTPRICE=0) then UPRICE
                                             else M_CONTPRICE
                                           end)
                                   end) as CR_UPRICE,
                                  M_PAYKIND,M_AGENNO as AGEN_NO,
                                  (select AGEN_NO || ' ' || AGEN_NAMEC 
                                     from PH_VENDER
                                    where AGEN_NO=a.M_AGENNO
                                  ) as AGEN_NAME,
                                 (select EMAIL from PH_VENDER where AGEN_NO = a.M_AGENNO) as EMAIL,
                                 (case when WEXP_ID = 'Y' then 'Y'
                                       when WEXP_ID = 'y' then 'Y'
                                   else 'N'
                                  end) as WEXP_ID,
                                  M_CONTPRICE, UPRICE, M_APPLYID, M_CONTID
                             from MI_MAST a
                            where MAT_CLASS='02'      --衛材
                              and ( (M_APPLYID<>'E')  --申請申購識別碼
                                    or (M_APPLYID='E' and M_CONTID='3') )  --鎖E但是如果是3零購,就可以申請
                              and M_STOREID<>'1'      --庫備識別碼
                              and NVL(CANCEL_ID,'N')='N'      --是否作廢 
                              and (M_APPLYID<>'P')  --申請申購識別碼鎖P
                           ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
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
            return DBWork.Connection.Query<AB0109>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTOWHcombo(string id)
        {
            string sql = @"select A.WH_NO VALUE, WH_NO||' '||WH_NAME TEXT,
                                  A.WH_NO||' '||A.WH_NAME COMBITEM
                             from MI_WHMAST A
                            where WH_KIND='1' and wh_grade > '1'
                              and exists(select 'X' 
                                           from UR_ID B 
                                          where (A.SUPPLY_INID=B.INID OR A.INID=B.INID) and TUSER=:TUSER)
                              and not exists(select 'X' 
                                               from MI_WHID B
                                              where TASK_ID in ('2','3') and WH_USERID=:TUSER)
                              and A.cancel_id = 'N'
                            union all 
                           select A.WH_NO, A.WH_NO||' '||WH_NAME, A.WH_NO||' '||A.WH_NAME COMBITEM
                             from MI_WHMAST A,MI_WHID B
                            where A.WH_NO=B.WH_NO and TASK_ID in ('2','3') and WH_USERID=:TUSER
                              and A.cancel_id='N'
                              and A.wh_grade>'1'
                             order by 1
                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetStatusCombo()
        {
            string sql = @"select DATA_VALUE as VALUE,DATA_DESC  as TEXT,
                                  DATA_VALUE||' '||DATA_DESC  as COMBITEM
                             from PARAM_D
                            where GRP_CODE='CR_DOC' and DATA_NAME='CR_STATUS'
                            order by DATA_VALUE
                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        #endregion

        //查詢
        #region 查詢
        public IEnumerable<AB0109> GetAll(string mmcode, string towh, string status, string id,int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            p.Add(":TUSER", string.Format("{0}", id));

            var sql = @"select a.CRDOCNO, MMCODE, MMCODE as MmcodeDisplay,MMNAME_C, MMNAME_E,                 
                               APPQTY, APPQTY as AppQtyDisplay, BASE_UNIT, TOWH, TOWH||' '||WH_NAME as WH_NAME,                               
                               TWN_DATE(REQDATE) as REQDATE,TWN_DATE(REQDATE) as ReqDateDisplay, DRNAME, DRNAME as DrnameDisplay,
                               PATIENTNAME, PATIENTNAME as PatientNameDisplay, 
                               CHARTNO, CHARTNO as ChartNoDisplay, CR_UPRICE,
                               (select DATA_VALUE||' '||DATA_DESC 
                                  from PARAM_D 
                                 where GRP_CODE='MI_MAST' and DATA_NAME='M_PAYKIND'
                                   and DATA_VALUE=a.M_PAYKIND) as M_PAYKIND,
                               (select AGEN_NO||' '||AGEN_NAMEC 
                                  from PH_VENDER 
                                 where AGEN_NO=a.AGEN_NO) as AGEN_NAME,
                               (select INID||' '||INID_NAME from UR_INID where INID=a.INID) as INID,
                               USER_NAME(APPID) as APPNM,
                               twn_time(APPTIME) as APPTIME, EMAIL, USER_NAME(CREATE_USER) as CREATE_USER , 
                               twn_time(CREATE_TIME) as CREATE_TIME,
                               (select DATA_DESC 
                                  from PARAM_D 
                                 where GRP_CODE='CR_DOC'
                                   and DATA_NAME='CR_STATUS' and DATA_VALUE=a.CR_STATUS) as STATUS,CR_STATUS,
                              b.USEWHERE,b.USEWHEN,b.TEL,b.USEWHERE as UseWhereDisplay,b.USEWHEN as UseWhenDisplay,b.TEL as TelDisplay,
                              (select M_APPLYID from MI_MAST where MMCODE=a.MMCODE) as M_APPLYID,
                              (select M_CONTID from MI_MAST where MMCODE=a.MMCODE) as M_CONTID
                         from CR_DOC a
                         left join CR_DOC_SMALL b
                           on a.CRDOCNO=b.CRDOCNO
                        where a.TOWH in (select A.WH_NO 
                                           from MI_WHMAST A
                                          where WH_KIND='1' and wh_grade > '1'
                                            and exists(select 'X' 
                                                         from UR_ID B 
                                                        where (A.SUPPLY_INID=B.INID OR A.INID=B.INID) and TUSER=:TUSER)
                                            and not exists(select 'X' 
                                                             from MI_WHID B
                                                            where TASK_ID in ('2','3') and WH_USERID=:TUSER)
                                            and A.cancel_id = 'N'
                                          union all 
                                         select A.WH_NO
                                           from MI_WHMAST A,MI_WHID B
                                          where A.WH_NO=B.WH_NO and TASK_ID in ('2','3') and WH_USERID=:TUSER
                                            and A.cancel_id='N'
                                            and A.wh_grade>'1')
                       ";

            if (string.IsNullOrEmpty(mmcode) == false)
            {
                sql += " and a.MMCODE=:p0 ";
                p.Add(":p0", string.Format("{0}", mmcode));
            }
            if (string.IsNullOrEmpty(towh) == false)
            {
                sql += " and a.TOWH=:p1 ";
                p.Add(":p1", string.Format("{0}", towh));
            }
            if (string.IsNullOrEmpty(status) == false)
            {
                sql += " and a.CR_STATUS=:p2 ";
                p.Add(":p2", string.Format("{0}", status));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0109>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        #endregion

        public int Create(AB0109 def)
        {
            var sql = @"insert into CR_DOC(CRDOCNO, MMCODE, APPQTY, BASE_UNIT, TOWH,
                                         REQDATE, DRNAME, PATIENTNAME, CHARTNO, CR_UPRICE,
                                         M_PAYKIND, AGEN_NO, INID, EMAIL, M_CONTPRICE,
                                         UPRICE, CR_STATUS, CREATE_TIME, CREATE_USER, UPDATE_IP,
                                         WH_NAME,MMNAME_C,MMNAME_E,ISSMALL)
                      values (:CRDOCNO, :MMCODE, :APPQTY, :BASE_UNIT, :TOWH,
                             TO_DATE(:REQDATE,'yyyy/mm/dd'), :DRNAME, :PATIENTNAME, :CHARTNO, :CR_UPRICE,
                             :M_PAYKIND, :AGEN_NO, (select INID from MI_WHMAST where WH_NO=:TOWH), :EMAIL, :M_CONTPRICE,
                             :UPRICE, 'A', sysdate, :CREATE_USER, :UPDATE_IP,
                             (select WH_NAME from MI_WHMAST where WH_NO=:TOWH),:MMNAME_C,:MMNAME_E,:ISSMALL)                             
                      ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }

        public int Update(AB0109 def)
        {
            string sql = @"update CR_DOC
                              set APPQTY=:APPQTY,
                                  TOWH=:TOWH,
                                  REQDATE=TO_DATE(:REQDATE,'yyyy/mm/dd'),
                                  DRNAME=:DRNAME,
                                  PATIENTNAME=:PATIENTNAME,
                                  CHARTNO=:CHARTNO,
                                  UPDATE_TIME=sysdate, 
                                  UPDATE_USER=:UPDATE_USER,
                                  UPDATE_IP=:UPDATE_IP
                            where CRDOCNO=:CRDOCNO
                              and CR_STATUS<>'C'
                           ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }
        public int Delete(AB0109 def)
        {
            string sql = @"update CR_DOC
                              set CR_STATUS='C',
                                  UPDATE_TIME=sysdate, 
                                  UPDATE_USER=:UPDATE_USER,
                                  UPDATE_IP=:UPDATE_IP
                            where CRDOCNO=:CRDOCNO
                              and CR_STATUS='A'
                           ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }
        public int Apply(AB0109 def)
        {
            string sql = @"update CR_DOC
                              set CR_STATUS='B',
                                  ACKMMCODE=:MMCODE,
                                  APPTIME =sysdate, 
                                  APPID=:UPDATE_USER,
                                  UPDATE_TIME=sysdate, 
                                  UPDATE_USER=:UPDATE_USER,
                                  UPDATE_IP=:UPDATE_IP
                            where CRDOCNO=:CRDOCNO
                              and CR_STATUS='A'
                           ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }
        public int Reject(AB0109 def)
        {
            string sql = @"update CR_DOC
                              set CR_STATUS='D',
                                  UPDATE_TIME=sysdate, 
                                  UPDATE_USER=:UPDATE_USER,
                                  UPDATE_IP=:UPDATE_IP
                            where CRDOCNO=:CRDOCNO
                              and CR_STATUS='B'
                           ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }

        public bool ChkExists_WHMM01(string mmcode)
        {
            string sql = @"select 1 
                             from MI_WHMM  
                            where MMCODE=:mmcode                             
                          ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { mmcode = mmcode }, DBWork.Transaction) != null; 
        }
        public bool ChkExists_WHMM02(string mmcode,string towh)
        {
            string sql = @"select 1 
                             from MI_WHMM  
                            where MMCODE=:mmcode and WH_NO=:wh_no                             
                          ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { mmcode = mmcode, wh_no=towh }, DBWork.Transaction) != null; 
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                               (select INV_QTY from MI_WHINV where MMCODE = A.MMCODE and WH_NO ='PH1S' and ROWNUM=1) as INV_QTY 
                          from MI_MAST A 
                         where MAT_CLASS='02'      --衛材
                           and ( (M_APPLYID<>'E')  --申請申購識別碼
                               or (M_APPLYID='E' and M_CONTID='3') )  --鎖E但是如果是3零購,就可以申請
                           and M_STOREID<>'1'      --庫備識別碼
                           and CANCEL_ID<>'Y'      --是否作廢
                       ";
            if (query.MMCODE != "")
            {
                sql += " and A.MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " and A.MMNAME_C like :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " and A.MMNAME_E like UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int InsCR_DOC_SMALL(AB0109 def)
        {
            var sql = @"insert into CR_DOC_SMALL(CRDOCNO, USEWHERE, USEWHEN, TEL)
                        values(:CRDOCNO, :USEWHERE, :USEWHEN, :TEL)
                       ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }
        public int UpCR_DOC_SMALL(AB0109 def)
        {
            var sql = @"update CR_DOC_SMALL a
                           set USEWHERE=:USEWHERE,
                               USEWHEN=:USEWHEN,
                               TEL=:TEL
                         where CRDOCNO=:CRDOCNO
                           and exists(select 1 from CR_DOC where CRDOCNO=a.CRDOCNO and CR_STATUS<>'C')
                       ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }

        public int DelCR_DOC_SMALL(AB0109 def)
        {
            var sql = @"delete CR_DOC_SMALL a
                         where CRDOCNO=:CRDOCNO
                           and exists(select 1 from CR_DOC where CRDOCNO=a.CRDOCNO and CR_STATUS='A')
                       ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }

        public int RejCR_DOC_SMALL(AB0109 def)
        {
            var sql = @"delete CR_DOC_SMALL a
                         where CRDOCNO=:CRDOCNO
                           and exists(select 1 from CR_DOC where CRDOCNO=a.CRDOCNO and CR_STATUS='B')
                       ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }

        public string GetCrdocno()
        {
            var p = new DynamicParameters();
            var sql = @" 
                         select 'EMG'||lpad(CRDOC_SEQ.nextval,7,0) as crdocno from dual
                        ";
            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);

        }
        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        public bool CheckMmcodeValid(string mmcode) {
            string sql = @"
                select MMCODE,MMNAME_C,MMNAME_E,BASE_UNIT,          
                       (case when (M_APPLYID='E' and M_CONTID='3') then UPRICE
                          else (case when (M_CONTPRICE is null or M_CONTPRICE=0) then UPRICE
                                  else M_CONTPRICE
                                end)
                        end) as CR_UPRICE,
                       M_PAYKIND,M_AGENNO as AGEN_NO,
                       (select AGEN_NO || ' ' || AGEN_NAMEC 
                          from PH_VENDER
                         where AGEN_NO=a.M_AGENNO
                       ) as AGEN_NAME,
                      (select EMAIL from PH_VENDER where AGEN_NO = a.M_AGENNO) as EMAIL,
                      (case when WEXP_ID = 'Y' then 'Y'
                            when WEXP_ID = 'y' then 'Y'
                        else 'N'
                       end) as WEXP_ID,
                       M_CONTPRICE, UPRICE, M_APPLYID, M_CONTID
                  from MI_MAST a
                 where MAT_CLASS='02'      --衛材
                   and ( (M_APPLYID<>'E')  --申請申購識別碼
                         or (M_APPLYID='E' and M_CONTID='3') )  --鎖E但是如果是3零購,就可以申請
                   and M_STOREID<>'1'      --庫備識別碼
                   and NVL(CANCEL_ID,'N')='N'      --是否作廢 
                   and (M_APPLYID<>'P')  --申請申購識別碼鎖P
                   and a.mmcode = :mmcode
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { mmcode }, DBWork.Transaction) != null;
        }
    }
}