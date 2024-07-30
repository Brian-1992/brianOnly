using System.Data;
using JCLib.DB;
using Dapper;
using System.Collections.Generic;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0041Repository : JCLib.Mvc.BaseRepository
    {
        public AA0041Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool T1CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM UR_INID WHERE INID=:INID";
            return !(DBWork.Connection.ExecuteScalar(sql, new { INID = id }, DBWork.Transaction) == null);
        }

        public bool T2CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM UR_INID_GRP WHERE GRP_NO=:GRP_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { GRP_NO = id }, DBWork.Transaction) == null);
        }

        public bool T2CheckRelExists(string id)
        {
            string sql = @"SELECT 1 FROM UR_INID_REL WHERE GRP_NO=:GRP_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { GRP_NO = id }, DBWork.Transaction) == null);
        }

        public int T1Create(UR_INID ur_inid)
        {
            var sql = @"INSERT INTO UR_INID (INID, INID_NAME, INID_FLAG, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:INID, :INID_NAME, :INID_FLAG, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ur_inid, DBWork.Transaction);
        }

        public int T2Create(UR_INID_GRP ur_inid_grp)
        {
            var sql = @"INSERT INTO UR_INID_GRP (GRP_NO, GRP_NAME, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:GRP_NO, :GRP_NAME, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ur_inid_grp, DBWork.Transaction);
        }

        public int T1Update(UR_INID ur_inid)
        {
            var sql = @"UPDATE UR_INID SET INID = :INID, INID_NAME = :INID_NAME, INID_OLD = :INID_O, INID_FLAG = :INID_FLAG,
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE INID = :INID_O";
            return DBWork.Connection.Execute(sql, ur_inid, DBWork.Transaction);
        }

        public int T2Update(UR_INID_GRP ur_inid_grp)
        {
            var sql = @"UPDATE UR_INID_GRP SET GRP_NAME = :GRP_NAME,
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE GRP_NO = :GRP_NO";
            return DBWork.Connection.Execute(sql, ur_inid_grp, DBWork.Transaction);
        }

        public int T1Delete(UR_INID ur_inid)
        {
            var sql = @"UPDATE UR_INID SET INID_OLD = 'D',
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE INID = :INID";
            return DBWork.Connection.Execute(sql, ur_inid, DBWork.Transaction);
        }

        public int T2Delete(UR_INID_GRP ur_inid_grp)
        {
            var sql = @"DELETE from UR_INID_GRP
                            WHERE GRP_NO = :GRP_NO ";
            return DBWork.Connection.Execute(sql, ur_inid_grp, DBWork.Transaction);
        }

        public IEnumerable<UR_INID> GetT1All(string inid, string inid_name)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT INID, INID_NAME, INID_FLAG, INID_OLD, 
                (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='UR_INID' 
                 AND DATA_NAME='INID_FLAG' AND DATA_VALUE=A.INID_FLAG) INID_FLAG_NAME, 
                (SELECT B.GRP_NO || ' ' || (SELECT GRP_NAME FROM UR_INID_GRP WHERE GRP_NO = B.GRP_NO) FROM UR_INID_REL B WHERE INID = A.INID) as GRP_NO,
                CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, 
                (SELECT UNA FROM UR_ID WHERE TUSER = A.CREATE_USER) CREATE_USER_NAME,
                (SELECT UNA FROM UR_ID WHERE TUSER = A.UPDATE_USER) UPDATE_USER_NAME
                FROM UR_INID A WHERE 1=1 ";

            if (inid != "")
            {
                sql += " AND INID LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", inid));
            }
            if (inid_name != "")
            {
                sql += " AND INID_NAME LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", inid_name));
            }

            return DBWork.PagingQuery<UR_INID>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<UR_INID_GRP> GetT2All(string grp_no)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT GRP_NO, GRP_NAME, 
                CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP
                FROM UR_INID_GRP WHERE 1=1 ";

            if (grp_no != "")
            {
                sql += " AND GRP_NO LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", grp_no));
            }

            return DBWork.PagingQuery<UR_INID_GRP>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<UR_INID_REL> GetT3All(string grp_no)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.GRP_NO, A.INID, 
                (select INID_NAME from UR_INID where INID=A.INID) as INID_NAME
                FROM UR_INID_REL A WHERE 1=1 ";

            if (grp_no != "")
            {
                sql += " AND A.GRP_NO = :p0 ";
                p.Add(":p0", grp_no);
            }

            return DBWork.PagingQuery<UR_INID_REL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<UR_INID> GetT31All(string grp_no, string inid, string inid_name)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.INID, A.INID_NAME, A.INID_FLAG
                FROM UR_INID A
                WHERE (A.INID_OLD <> 'D' or A.INID_OLD is null)
                and not exists (select * from UR_INID_REL where INID = A.INID) ";

            // p.Add(":p0", grp_no);

            if (inid != "")
            {
                sql += " AND A.INID like :p1 ";
                p.Add(":p1", string.Format("%{0}%", inid));
            }
            if (inid_name != "")
            {
                sql += " AND A.INID_NAME like :p2 ";
                p.Add(":p2", string.Format("%{0}%", inid_name));
            }

            return DBWork.PagingQuery<UR_INID>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<UR_INID> T1Get(string id)
        {
            var sql = @"SELECT INID, INID_NAME, INID_FLAG, INID_OLD, 
                (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='UR_INID' 
                 AND DATA_NAME='INID_FLAG' AND DATA_VALUE=A.INID_FLAG) INID_FLAG_NAME, 
                (SELECT B.GRP_NO || ' ' || (SELECT GRP_NAME FROM UR_INID_GRP WHERE GRP_NO = B.GRP_NO) FROM UR_INID_REL B WHERE INID = A.INID) as GRP_NO,
                CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, 
                (SELECT UNA FROM UR_ID WHERE TUSER = A.CREATE_USER) CREATE_USER_NAME,
                (SELECT UNA FROM UR_ID WHERE TUSER = A.UPDATE_USER) UPDATE_USER_NAME
                FROM UR_INID A WHERE INID = :INID";
            return DBWork.Connection.Query<UR_INID>(sql, new { INID = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_INID_GRP> T2Get(string id)
        {
            var sql = @"SELECT GRP_NO, GRP_NAME, 
                CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER
                FROM UR_INID_GRP WHERE GRP_NO = :GRP_NO";
            return DBWork.Connection.Query<UR_INID_GRP>(sql, new { GRP_NO = id }, DBWork.Transaction);
        }

        public int T3Delete(string p0)
        {
            var sql = "";
            string[] allCBData = p0.ToString().Trim().Split(',');   //把資料用','分開，一筆grid data為一筆
            var p = new DynamicParameters();
            int delCnt = 0;
            for (int i = 0; i < allCBData.Length; i++)
            {
                string[] gridRowData = allCBData[i].Split('^');
                string GRP_NO = gridRowData[0].ToString();
                string INID = gridRowData[1].ToString();

                sql = @" DELETE from UR_INID_REL WHERE GRP_NO = :p0 AND INID = :p1";
                p.Add(":p0", GRP_NO);
                p.Add(":p1", INID);

                var cnt = DBWork.Connection.Execute(sql, p, DBWork.Transaction);
                delCnt++;
            }

            return delCnt;
        }

        public int T31Add(string p0)
        {
            var sql = "";
            string[] allCBData = p0.ToString().Trim().Split(',');   //把資料用','分開，一筆grid data為一筆
            var p = new DynamicParameters();
            int addCnt = 0;
            for (int i = 0; i < allCBData.Length; i++)
            {
                string[] gridRowData = allCBData[i].Split('^');
                string GRP_NO = gridRowData[0].ToString();
                string INID = gridRowData[1].ToString();

                if (chkRel(INID) == 0) // 檢查此INID是否已被其他使用者指定
                {
                    sql = @" insert into UR_INID_REL (GRP_NO, INID) values(:p0, :p1) ";
                    p.Add(":p0", GRP_NO);
                    p.Add(":p1", INID);

                    var cnt = DBWork.Connection.Execute(sql, p, DBWork.Transaction);
                    addCnt++;
                }
                else
                {
                    addCnt = 0;
                    break;
                }
            }

            return addCnt;
        }

        internal DataTable T1GetExcel(string inid, string inid_name)
        {
            var dt = new DataTable();
            var p = new DynamicParameters();
            var sql = @"SELECT A.INID AS 責任中心代碼, A.INID_NAME AS 責任中心名稱,
                A.INID_OLD AS 刪除註記,
                (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='UR_INID' 
                 AND DATA_NAME='INID_FLAG' AND DATA_VALUE=A.INID_FLAG) AS 單位類別註記,
                (SELECT B.GRP_NO || ' ' || (SELECT GRP_NAME FROM UR_INID_GRP WHERE GRP_NO = B.GRP_NO) FROM UR_INID_REL B WHERE INID = A.INID) as 歸戶,
                A.UPDATE_TIME AS 紀錄更新日期時間,
                A.UPDATE_USER AS 紀錄更新人員
                FROM UR_INID A WHERE 1=1 ";

            if (inid != "")
            {
                sql += " AND INID LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", inid));
            }
            if (inid_name != "")
            {
                sql += " AND INID_NAME LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", inid_name));
            }
            sql += " ORDER BY INID ";
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
            {
                dt.Load(rdr);
            }
            return dt;
        }

        //=== COMBO ===

        public IEnumerable<ComboItemModel> GetCombo()
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"SELECT INID AS VALUE, INID||' '||INID_NAME AS TEXT 
                        FROM UR_INID WHERE INID_OLD != 'D' ";

            sql += " ORDER BY INID ";

            return DBWork.Connection.Query<ComboItemModel>(sql, p);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT FROM MI_MAST A WHERE 1=1 ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
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

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetGrpCombo()
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"SELECT GRP_NO AS VALUE, GRP_NO || ' ' || GRP_NAME AS TEXT 
                        FROM UR_INID_GRP ";

            sql += " ORDER BY GRP_NO ";

            return DBWork.Connection.Query<ComboItemModel>(sql, p);
        }

        public int chkRel(string inid)
        {
            string sql = @" select count(*) from UR_INID_REL
                            where INID = :INID ";
            return DBWork.Connection.QueryFirst<int>(sql, new { INID = inid }, DBWork.Transaction);
        }
    }
}