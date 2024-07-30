using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.B
{
    public class BD0011Repository : JCLib.Mvc.BaseRepository
    {
        public BD0011Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢
        public IEnumerable<BD0011> GetAll(string START_DATE, string END_DATE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT PMM.DOCID,
                               PMM.AGEN_NO,
                               PMM.MSG,
                               PMM.OPT,
                               PMM.OPT OPT_TEXT,
                               (CASE
                                    WHEN PMM.OPT = 'A' THEN '藥衛全部廠商'
                                    WHEN PMM.OPT = 'G' THEN '一般物品全部廠商'
                                    WHEN PMM.OPT = 'P' THEN '部分廠商'
                                    WHEN PMM.OPT = 'E' THEN '藥品廠商'
                                    WHEN PMM.OPT = 'R' THEN '衛材廠商'
                                    WHEN PMM.OPT = 'O' THEN '其他廠商'
                                END) OPT_DISPLAY,
                               TWN_DATE (PMM.SEND_DT) SEND_DT,
                               TWN_DATE (PMM.SEND_DT) SEND_DT_DISPLAY,
                               PMM.THEME,
                               PMM.STATUS,
                               (CASE
                                    WHEN PMM.STATUS = '80' THEN '未通知'
                                    WHEN PMM.STATUS = '84' THEN '待傳MAIL'
                                    WHEN PMM.STATUS = '82' THEN '已傳MAIL'
                                END) STATUS_NAME,
                               (SELECT FILENAME
                                FROM PH_MSGMAIL_D PMD
                                WHERE PMM.DOCID = PMD.DOCID
                                AND PMM.THEME = PMD.THEME AND ROWNUM = 1) FILENAME
                        FROM PH_MSGMAIL_M PMM
                        WHERE 1 = 1";

            if (START_DATE != "")
            {
                sql += " AND TWN_DATE (PMM.SEND_DT) >= :START_DATE ";
                p.Add(":START_DATE", string.Format("{0}", START_DATE));
            }
            if (END_DATE != "")
            {
                sql += " AND TWN_DATE (PMM.SEND_DT) <= :END_DATE ";
                p.Add(":END_DATE", string.Format("{0}%", END_DATE));
            }

            sql += " ORDER BY PMM.SEND_DT, PMM.DOCID";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0011>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //新增1
        public int CreateM(BD0011 BD0011)
        {
            var sql = @"INSERT INTO PH_MSGMAIL_M 
                                    (DOCID,
                                     THEME,
                                     SEND_DT,
                                     MSG,
                                     CREATE_TIME,
                                     CREATE_USER,
                                     UPDATE_IP,
                                     STATUS,
                                     OPT,
                                     AGEN_NO)
                              VALUES ( :DOCID,
                                       :THEME,
                                       TO_DATE ( :SEND_DT, 'YYYY/MM/DD'),
                                       :MSG,
                                       SYSDATE,
                                       :CREATE_USER,
                                       :UPDATE_IP,
                                       '80',
                                       :OPT,
                                       :AGEN_NO)";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }

        //新增2
        public int CreateD(BD0011 BD0011)
        {
            var sql = @"INSERT INTO PH_MSGMAIL_D 
                                    (SEQ,
                                     DOCID,
                                     THEME,
                                     FILENAME,
                                     UPDATE_TIME,
                                     STATUS)
                              VALUES ( (SELECT NVL (MAX (SEQ) + 1, 0) SEQ
                                        FROM PH_MSGMAIL_D
                                        WHERE DOCID = :DOCID),
                                        :DOCID,
                                        :THEME,
                                        :FILENAME,
                                        SYSDATE,
                                        'A')";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }

        //修改1
        public int UpdateM(BD0011 BD0011)
        {
            var sql = @"UPDATE PH_MSGMAIL_M
                        SET SEND_DT = TO_DATE ( :SEND_DT, 'YYYY/MM/DD'),
                            MSG = :MSG,
                            CREATE_TIME = SYSDATE,
                            CREATE_USER = :CREATE_USER,
                            UPDATE_IP = :UPDATE_IP,
                            OPT = :OPT,
                            AGEN_NO = :AGEN_NO,
                            THEME = :THEME
                        WHERE DOCID = :DOCID";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }

        //修改2
        public int UpdateD(BD0011 BD0011)
        {
            var sql = @"UPDATE PH_MSGMAIL_D
                        SET THEME = :THEME,
                            FILENAME = :FILENAME,
                            UPDATE_TIME = SYSDATE,
                            STATUS = 'A'
                        WHERE DOCID = :DOCID";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }

        public bool CheckExists(string DOCID, string THEME)
        {
            string sql = @"SELECT 1
                           FROM PH_MSGMAIL_M
                           WHERE DOCID = :DOCID
                           AND THEME = :THEME";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCID = DOCID, THEME = THEME }, DBWork.Transaction) == null);
        }

        //刪除1
        public int DeleteM(string DOCID, string THEME)
        {
            var sql = @"DELETE FROM PH_MSGMAIL_M
                        WHERE DOCID = :DOCID
                        AND THEME = :THEME";
            return DBWork.Connection.Execute(sql, new { DOCID = DOCID, THEME = THEME }, DBWork.Transaction);
        }

        //刪除2
        public int DeleteD(string DOCID, string THEME)
        {
            var sql = @"DELETE FROM PH_MSGMAIL_D
                        WHERE DOCID = :DOCID
                        AND THEME = :THEME";
            return DBWork.Connection.Execute(sql, new { DOCID = DOCID, THEME = THEME }, DBWork.Transaction);
        }

        //取得識別號
        public string GetDocid()
        {
            var sql = @"SELECT TO_CHAR (SYSDATE, 'YYYYMMDDHH24MISS') FROM DUAL";
            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        //取得廠商選擇視窗內容
        public IEnumerable<PH_VENDER> GetAGEN()
        {
            string sql = @"  SELECT a.AGEN_NO, 
                                    a.AGEN_NAMEC,
                                    a.email
                              FROM PH_VENDER a
                             where a.rec_status='A'
                               and a.AGEN_NAMEC<>'無合約'
                             ORDER BY a.AGEN_NO";
            return DBWork.Connection.Query<PH_VENDER>(sql, DBWork.Transaction);
        }

        //寄送MAIL
        public int SendMail(BD0011 BD0011)
        {
            var sql = @"UPDATE PH_MSGMAIL_M
                           SET STATUS = '84',
                               CREATE_TIME = SYSDATE,
                               CREATE_USER = :CREATE_USER,
                               UPDATE_IP = :UPDATE_IP
                         WHERE DOCID = :DOCID
                           AND THEME = :THEME";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }
        //新增 PH_MSGMAIL_AGEN
        public int CreateAGENNO(BD0011 BD0011, string agen_no)
        {
            BD0011.AGEN_NO = agen_no;
            var sql = @"INSERT INTO PH_MSGMAIL_AGEN 
                         (DOCID, AGEN_NO, CREATE_TIME, UPDATE_USER, STATUS)
                        VALUES (:DOCID, :AGEN_NO, sysdate, :CREATE_USER, '84')";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }
        public int CreateAGENNO_ALL(BD0011 BD0011)
        {   // 全部廠商
            var sql = @"INSERT INTO PH_MSGMAIL_AGEN 
                         (DOCID, AGEN_NO, CREATE_TIME, UPDATE_USER, STATUS)
                        select :DOCID, agen_no, sysdate, :CREATE_USER, '84'
                        from PH_VENDER 
                        where rec_status='A'
                          and trim(EMAIL) is not null  --無email，不寄信
                        order by agen_no ";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }
        public int CreateAGENNO_MED(BD0011 BD0011)
        {   // 藥品廠商 抓廠編為3碼，且有藥品者
            var sql = @"INSERT INTO PH_MSGMAIL_AGEN 
                         (DOCID, AGEN_NO, CREATE_TIME, UPDATE_USER, STATUS)
                        select :DOCID, agen_no, sysdate, :CREATE_USER, '84'
                        from PH_VENDER a
                        where a.rec_status='A' and  length(a.AGEN_NO)=3
                          and trim(EMAIL) is not null  --無email，不寄信
                          and exists (select 1 from MI_MAST 
                                       where m_agenno = a.agen_no
                                        and mat_class = '01')
                        order by agen_no ";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }
        public int CreateAGENNO_MAT(BD0011 BD0011)
        {   // 衛材衛廠商 抓廠編為3碼者，且有衛材者
            var sql = @"INSERT INTO PH_MSGMAIL_AGEN 
                         (DOCID, AGEN_NO, CREATE_TIME, UPDATE_USER, STATUS)
                        select :DOCID, agen_no, sysdate, :CREATE_USER, '84'
                        from PH_VENDER a
                        where a.rec_status='A' and  length(a.AGEN_NO)=3
                          and trim(EMAIL) is not null  --無email，不寄信
                          and exists (select 1 from MI_MAST 
                                       where m_agenno = a.agen_no
                                        and mat_class = '02')
                        order by agen_no ";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }
        public int CreateAGENNO_GEN(BD0011 BD0011)
        {   // 一般物品廠商 抓廠編為4碼者
            var sql = @"INSERT INTO PH_MSGMAIL_AGEN 
                         (DOCID, AGEN_NO, CREATE_TIME, UPDATE_USER, STATUS)
                        select :DOCID, agen_no, sysdate, :CREATE_USER, '84'
                        from PH_VENDER 
                        where rec_status='A' and  length(AGEN_NO)=4
                          and MAIN_INID='540000'
                          and trim(EMAIL) is not null
                        order by agen_no ";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }
        public int CreateAGENNO_OTHER(BD0011 BD0011)
        {   // 其他廠商 抓廠編為3碼，無藥品且無衛材的者
            var sql = @"INSERT INTO PH_MSGMAIL_AGEN 
                         (DOCID, AGEN_NO, CREATE_TIME, UPDATE_USER, STATUS)
                        select :DOCID, agen_no, sysdate, :CREATE_USER, '84'
                        from PH_VENDER a
                        where a.rec_status='A' and  length(a.AGEN_NO)=3
                          and trim(EMAIL) is not null  --無email，不寄信
                          and not exists (select 1 from MI_MAST 
                                           where m_agenno = a.agen_no
                                             and mat_class in ('01', '02'))
                        order by agen_no ";
            return DBWork.Connection.Execute(sql, BD0011, DBWork.Transaction);
        }

        public IEnumerable<ERROR_LOG> GetErrorLogs(string po_no) {
            string sql = string.Format(@"
                           select * from ERROR_LOG
                            where pg = 'BDS001'
                              and msg like '%{0}%'"
                            , po_no);
            return DBWork.Connection.Query<ERROR_LOG>(sql, DBWork.Transaction);
        }
    }
}