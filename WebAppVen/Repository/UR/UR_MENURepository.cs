using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using System.Collections.Generic;
using System.Data;
using WebAppVen.Models;

namespace WebAppVen.Repository.UR
{
    public class UR_MENURepository : JCLib.Mvc.BaseRepository
    {
        public UR_MENURepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<UR_MENU> GetMenuAll(string parent, string sg, bool expanded = false)
        {
            var sql = @"SELECT FG, PG, SG, FS, FT, F0, FL, F1, FA0, FU, FUP, FG AS id,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            {0} AS expanded
                            FROM UR_MENU
                            WHERE (SG = @SG) {1} 
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, expanded ? "1" : "0", " AND PG IS NULL ");
                return DBWork.Connection.Query<UR_MENU>(sql, new { SG = sg });
            }
            else
            {
                sql = string.Format(sql, expanded ? "1" : "0", " AND PG = @PG ");
                return DBWork.Connection.Query<UR_MENU>(sql, new { PG = parent, SG = sg });
            }
        }

        public IEnumerable<MenuView> GetMenuIndex(string parent, string tuser, string sg, bool expanded = true)
        {
            var url_action = "Index";
            if (sg == "MOBILE") url_action = "Mobile";
            var sql = @"SELECT FG, PG, FS, FG AS id,
                            --CASE WHEN (FT = 'F') THEN '' ELSE FA0 END AS url,
                            CASE WHEN (FT = 'F') THEN '' ELSE '/Form/{0}/'+FG+iif(FUP is NULL,'','?'+FUP) END AS url,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 + '(' + F0 + ')' END AS text,
                            --CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            {1} AS expanded
                            FROM UR_MENU
                            WHERE (SG = @SG) {2} AND (FL = 1) AND (FG IN
                                (SELECT  A.FG
                                 FROM    UR_TACL A INNER JOIN
                                         UR_UIR B ON A.RLNO = B.RLNO
                                 WHERE  (A.V = 1) AND (B.TUSER = @TUSER)))
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, url_action, expanded ? "1" : "0", " AND PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { SG = sg, TUSER = tuser });
            }
            else
            {
                sql = string.Format(sql, url_action, expanded ? "1" : "0", " AND PG = @PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { PG = parent, SG = sg, TUSER = tuser });
            }
        }

        public IEnumerable<MenuView> GetMenuByUser(string parent, string tuser)
        {
            var sql = @"SELECT FG, PG, FS, FG AS id,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 + '(' + F0 + ')' END AS text,
                            --CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            --CASE WHEN (PG IS NULL) THEN 1 ELSE 0 END AS expanded
                            0 AS expanded
                            FROM UR_MENU
                            WHERE (SG = 'YS') {0} AND (FL = 1) AND (FG IN
                                (SELECT  A.FG
                                 FROM    UR_TACL A INNER JOIN
                                         UR_UIR B ON A.RLNO = B.RLNO
                                 WHERE  (A.V = 1) AND (B.TUSER = @TUSER)))
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, " AND PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser });
            }
            else
            {
                sql = string.Format(sql, " AND PG = @PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { PG = parent, TUSER = tuser });
            }
        }

        public IEnumerable<MenuView> GetMenuByRole(string parent, string rlno, string sg)
        {
            var sql = @"SELECT A.FG, A.PG, A.FS, A.FG AS id,B.V,B.R,B.U,B.P,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 + '(' + F0 + ')' END AS text,
                            --CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            0 AS expanded
                            FROM UR_MENU A LEFT OUTER JOIN UR_TACL B ON A.FG=B.FG AND B.RLNO=@RLNO
                            WHERE A.SG=@SG {0}
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, " AND A.PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { RLNO = rlno, SG = sg });
            }
            else
            {
                sql = string.Format(sql, " AND A.PG = @PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { RLNO = rlno, SG = sg, PG = parent });
            }
        }

        public IEnumerable<MenuView> GetMenuByQuery(string menuName, string tuser)
        {
            var sql = @"SELECT FG as id,F1 + '(' + F0 + ')' AS text,
                        CASE WHEN (FT = 'F') THEN '' ELSE '/Form/Index/'+FG+iif(FUP is NULL,'','?'+FUP) END AS url,
                        FS FROM UR_MENU
                        WHERE FL = 1 AND FT = 'L' AND(UPPER(F0) LIKE UPPER('%' + @MENUNAME + '%') OR UPPER(F1) LIKE UPPER('%' + @MENUNAME + '%'))
                        AND FG IN(SELECT A.FG FROM UR_TACL A JOIN UR_UIR B ON A.RLNO = B.RLNO WHERE A.V = 1 AND B.TUSER =@TUSER)
                        ORDER BY FG";
            var a = sql;
            return DBWork.Connection.Query<MenuView>(sql, new { MENUNAME = menuName, TUSER = tuser });
        }
        public bool CheckExists(string fg)
        {
            string sql = @"SELECT 1 FROM UR_MENU WHERE FG=@FG";
            return !(DBWork.Connection.ExecuteScalar(sql, new { FG = fg }, DBWork.Transaction) == null);
        }

        public IEnumerable<UR_MENU> Get(string fg)
        {
            var sql = @"SELECT FG, PG, SG, FS, FT, F0, F1, FA0, FU, FUP, FL, FG AS id,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            0 AS expanded 
                            from UR_MENU where FG=@FG";
            return DBWork.Connection.Query<UR_MENU>(sql, new { FG = fg }, DBWork.Transaction);
        }

        public string GetUrl(string fg)
        {
            var sql = @"SELECT FU + '.js' + iif(FUP is NULL,'','?'+FUP) FROM UR_MENU where FG=@FG";
            return DBWork.Connection.ExecuteScalar(sql, new { FG = fg }, DBWork.Transaction)?.ToString()??"";
        }

        public decimal GetNextFS(string pg)
        {
            var sql = @"SELECT ISNULL((MAX(FS) + 1), 0) FS FROM UR_MENU WHERE PG=@PG";

            return decimal.Parse(DBWork.Connection.ExecuteScalar(sql, new { PG = pg }, DBWork.Transaction).ToString());
        }

        public int Create(UR_MENU ur_menu)
        {
            var _afrs = 0;
            var sql = @"INSERT INTO UR_MENU (FG, PG, SG, FS, FT, F0, F1, FA0, FU, FUP, FL)
                        VALUES (@FG, @PG, @SG, @FS, @FT, @F0, @F1, @FA0, @FU, @FUP, @FL)";

            _afrs = DBWork.Connection.Execute(sql, ur_menu, DBWork.Transaction);

            return _afrs;
        }

        public int Update(UR_MENU ur_menu)
        {
            var _afrs = 0;
            var sql = @"UPDATE UR_MENU 
                            SET FT = @FT,
                            F0 = @F0,
                            F1 = @F1,
                            FU = @FU,
                            FUP = @FUP,
                            FL = @FL 
                            WHERE FG=@FG";

            _afrs = DBWork.Connection.Execute(sql, ur_menu, DBWork.Transaction);

            return _afrs;
        }

        public int UpdateFS(UR_MENU ur_menu)
        {
            var _afrs = 0;
            var sql = @"UPDATE UR_MENU SET FS = @FS WHERE FG=@FG";

            _afrs = DBWork.Connection.Execute(sql, ur_menu, DBWork.Transaction);

            return _afrs;
        }

        public int Delete(string fg)
        {
            var _afrs = 0;
            var sql = @"DELETE FROM UR_MENU WHERE FG=@FG";

            _afrs = DBWork.Connection.Execute(sql, new { FG = fg }, DBWork.Transaction);

            return _afrs;
        }

        /*
        public ApiResponse GetMenuView(string tuser)
        {
            ar.etts = GetChildNode(null, tuser);
            return ar;
        }

        public DataTable GetMenuByUser(string tuser)
        {
            DataTable dt = new DataTable();
            string sql = @"SELECT A.FG,PG,SG,FT,FS,F0,F1,FA0 FROM UR_MENU A INNER JOIN UR_TACL B ON A.FG=B.FG AND B.V=1
                            AND B.RLNO = (SELECT RLNO FROM UR_UIR WHERE TUSER = :TUSER {0}) ORDER BY FS";
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { TUSER = tuser }, DBWork.Transaction))
            {
                dt.Load(rdr);
            }
            return dt;
        }

        public IEnumerable<MenuView> GetChildNode(string parent, string tuser, bool rec = true)
        {
            IEnumerable<MenuView> result;
            using (var conn = db.GetConnection())
            {
                string sql = @"SELECT FG, PG, FS, FG AS id,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 + '(' + F0 + ')' END AS text,
                            CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            1 AS expanded
                            FROM UR_MENU
                            WHERE (SG = 'YS') {0} AND (FL = 1) AND (FG IN
                                (SELECT  A.FG
                                 FROM    UR_TACL AS A INNER JOIN
                                         UR_UIR AS B ON A.RLNO = B.RLNO
                                 WHERE  (A.V = 1) AND (B.TUSER = :TUSER)))
                            ORDER BY FS";
                if (parent == null || parent == "")
                {
                    sql = string.Format(sql, " AND PG IS NULL ");
                    result = conn.Query<MenuView>(sql, new { TUSER = tuser });
                }
                else
                {
                    sql = string.Format(sql, " AND PG = :PG ");
                    result = conn.Query<MenuView>(sql, new { PG = parent, TUSER = tuser });
                }
                if (rec)
                    foreach (MenuView mv in result)
                    {
                        if(!mv.leaf)
                            mv.children = GetChildNode(mv.FG, tuser);
                    }
            }
            return result;
        }
         * */
    }
}