using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;
using System.Collections.Generic;

namespace WebAppVen.Repository.BH
{
    public class BH0004Repository : JCLib.Mvc.BaseRepository
    {
        public BH0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<WB_AIRHIS> GetAll(string AGEN_NO, string NAMEC, string FBNO, string TXTDAY_B, string TXTDAY_E, string SEQ,string p6, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = "";

            if (p6 == "T1")
            {
                sql = @"SELECT * FROM [dbo].[WB_AIRHIS] Where agen_no=@agen_no ";
            }
            else
            {
                sql = @"SELECT * FROM [dbo].[WB_AIRST] Where agen_no=@agen_no ";
            }


            if (NAMEC != "")
            {
                sql += " AND NAMEC LIKE @p0 ";
                p.Add("@p0", string.Format("%{0}%", NAMEC));
            }
            if (FBNO != "")
            {
                sql += " AND FBNO LIKE @p1 ";
                p.Add("@p1", string.Format("{0}%", FBNO));
            }
            if (TXTDAY_B != "")
            {
                sql += " AND TXTDAY >= @p2 ";
                p.Add("@p2", string.Format("{0}", TXTDAY_B));
            }
            if (TXTDAY_E != "")
            {
                sql += " AND TXTDAY <= @p3 ";
                p.Add("@p3", string.Format("{0}", TXTDAY_E));
            }
            if (SEQ != "")
            {
                sql += " AND SEQ=@p4 ";
                p.Add("@p4", string.Format("{0}", SEQ));
            }

            sql += " order by TXTDAY, NAMEC, FBNO ";

            p.Add("@agen_no", AGEN_NO);


            return DBWork.Connection.Query<WB_AIRHIS>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<WB_AIRHIS> Get(WB_AIRHIS wb_airhis)
        {

            var sql = @"SELECT * FROM WB_AIRHIS WHERE FBNO=@FBNO  AND SEQ=(select ISNULL(max(seq),0) from WB_AIRHIS where txtday = @TXTDAY) AND TXTDAY=@TXTDAY AND AGEN_NO=@AGEN_NO ";
            return DBWork.Connection.Query<WB_AIRHIS>(sql, wb_airhis, DBWork.Transaction);
        }

        public IEnumerable<WB_AIRHIS> Get_U(WB_AIRHIS wb_airhis)
        {
            var sql = @"SELECT * FROM WB_AIRHIS WHERE FBNO=@FBNO  AND SEQ=@SEQ AND TXTDAY=@TXTDAY AND AGEN_NO=@AGEN_NO ";
            return DBWork.Connection.Query<WB_AIRHIS>(sql, wb_airhis, DBWork.Transaction);
        }


        public int Create(WB_AIRHIS wb_airhis)
        {
            string sql = @"INSERT INTO [dbo].[WB_AIRHIS]
                          ([TXTDAY]
                          ,[AGEN_NO]
                          ,[FBNO]
                          ,[SEQ]
                          ,[EXTYPE]
                          ,[XSIZE]
                          ,[NAMEC]
                          ,[STATUS]
                          ,[CREATE_TIME]
                          ,[CREATE_USER]
                          ,[UPDATE_TIME]
                          ,[UPDATE_USER]
                          ,[UPDATE_IP]
                          ,[FLAG])
                     VALUES
                           (@TXTDAY 
                           ,@AGEN_NO
                           ,@FBNO
                           ,(select ISNULL(max(seq),0)+1 from WB_AIRHIS where txtday=@TXTDAY)
                           ,@EXTYPE       
                           ,@XSIZE
                           ,@NAMEC
                           ,'A'
                           ,(select GETDATE())
                           ,@CREATE_USER
                           ,(select GETDATE())
                           ,@UPDATE_USER
                           ,@UPDATE_IP
                           ,'A'
                		   )";

            return DBWork.Connection.Execute(sql, wb_airhis, DBWork.Transaction);
        }

        public int Update(WB_AIRHIS wb_airhis)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE [dbo].[WB_AIRHIS]
                       SET 
                           [NAMEC] = @NAMEC
                          ,[EXTYPE] = @EXTYPE
                          ,[XSIZE] = @XSIZE
                          ,[UPDATE_TIME] = (select GETDATE())
                          ,[UPDATE_USER] = @UPDATE_USER
                          ,[UPDATE_IP] = @UPDATE_IP
                     WHERE FBNO=@FBNO_old AND  SEQ=@SEQ AND TXTDAY=@TXTDAY AND AGEN_NO=@AGEN_NO";


            return DBWork.Connection.Execute(sql, wb_airhis, DBWork.Transaction);
        }

        public int Delete(WB_AIRHIS wb_airhis)
        {
            var sql = @"DELETE WB_AIRHIS 
                                WHERE FBNO=@FBNO  AND SEQ=@SEQ AND TXTDAY=@TXTDAY AND AGEN_NO=@AGEN_NO";
            return DBWork.Connection.Execute(sql, wb_airhis, DBWork.Transaction);
        }

        public int confirmData(WB_AIRHIS wb_airhis)
        {

            var sql = @"update WB_AIRHIS 
                        set status='B', flag='A'  
                     WHERE FBNO=@FBNO AND SEQ=@SEQ AND TXTDAY=@TXTDAY AND AGEN_NO=@AGEN_NO";


            return DBWork.Connection.Execute(sql, wb_airhis, DBWork.Transaction);
        }

        public bool CheckExists(WB_AIRHIS wb_airhis)
        {
            string sql = @"SELECT 1 FROM WB_AIRHIS WHERE FBNO=@FBNO AND SEQ=@SEQ AND TXTDAY=@TXTDAY AND AGEN_NO=@AGEN_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, wb_airhis, DBWork.Transaction) == null);
        }

        public IEnumerable<ComboItemModel> GetDeptCombo()
        {
            string sql = @"select inid+' '+inid_name TEXT ,inid VALUE  from ur_inid";
            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }
        

        public bool CheckAGENExists(string AGEN_NO)
        {
            string sql = @"SELECT 1 FROM PH_VENDER WHERE AGEN_NO=@AGEN_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = AGEN_NO }, DBWork.Transaction) == null);
        }

        public bool CheckMmcodeExists(string MMCODE)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=@MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction) == null);
        }
        public bool CheckDeptExists(string Dept)
        {
            string sql = @"SELECT 1 FROM UR_INID WHERE INID=@INID ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { INID = Dept }, DBWork.Transaction) == null);
        }

        public bool CheckFbnoExists(string FBNO)
        {
            string sql = @"SELECT 1 FROM WB_AIRST WHERE FBNO=@FBNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { FBNO = FBNO }, DBWork.Transaction) == null);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS FROM MI_MAST A WHERE 1=1 ";


            if (p0 != "")
            {
                //sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, @MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, @MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, @MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                sql = string.Format(sql, "(ISNULL(CHARINDEX( @MMCODE_I,A.MMCODE), 1000) + ISNULL(CHARINDEX( @MMNAME_E_I,A.MMNAME_E), 100) * 10 + ISNULL(CHARINDEX(@MMNAME_C_I,A.MMNAME_C), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add("@MMCODE_I", p0);
                p.Add("@MMNAME_E_I", p0);
                p.Add("@MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE @MMCODE ";
                p.Add("@MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE @MMNAME_E ";
                p.Add("@MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE @MMNAME_C) ";
                p.Add("@MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }


        public string GetDataTime()
        {
            var sql = @"select convert(CHAR(20), UPDATE_TIME , 120) as UPDATE_TIME from WB_AIRTIME";

            var str = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction);

            return str == null ? "" : str.ToString();

        }
    }
}