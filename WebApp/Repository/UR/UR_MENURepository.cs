using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Collections.Generic;
using System.Data;
using WebApp.Models;

namespace WebApp.Repository.UR
{
    public class UR_MENURepository : JCLib.Mvc.BaseRepository
    {
        public UR_MENURepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<UR_MENU> GetMenuAll(string parent, bool expanded = false)
        {
            var sql = @"SELECT FG, PG, SG, FS, FT, F0, FL, F1, FA0, FU, FUP, FD, FG AS id, ATTACH_URL, 
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            {0} AS expanded
                            FROM UR_MENU
                            WHERE {1} 
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, expanded ? "1" : "0", " PG IS NULL ");
                return DBWork.Connection.Query<UR_MENU>(sql);
            }
            else
            {
                sql = string.Format(sql, expanded ? "1" : "0", " PG = :PG ");
                return DBWork.Connection.Query<UR_MENU>(sql, new { PG = parent });
            }
        }

        public IEnumerable<MenuView> GetMenuIndex(string parent, string tuser, bool expanded = true)
        {
            var sql = @"SELECT FG, PG, FS, FG AS id,
                            CASE WHEN (FT = 'F') THEN '' ELSE FA0||'/'||FG||DECODE(FUP,NULL,'','?'||FUP) END AS url,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 || '(' || F0 || ')' END AS text,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            {0} AS expanded
                            FROM UR_MENU
                            WHERE {1} AND (FL = 1) AND (FG IN
                                (SELECT  A.FG
                                 FROM    UR_TACL A INNER JOIN
                                         UR_UIR B ON A.RLNO = B.RLNO
                                 WHERE  (A.V = 1) AND (B.TUSER = :TUSER)))
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, expanded ? "1" : "0", " PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser });
            }
            else
            {
                sql = string.Format(sql, expanded ? "1" : "0", " PG = :PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { PG = parent, TUSER = tuser });
            }
        }

        public IEnumerable<MenuView> GetMenuLogin(string parent, string tuser, bool expanded = true)
        {
            var sql = @"SELECT FG, PG, FS, FG AS id,
                            CASE WHEN (FT = 'F') THEN '' ELSE FA0||'/'||FG||DECODE(FUP,NULL,'','?'||FUP) END AS url,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 || '(' || F0 || ')' END AS text,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            {0} AS expanded
                            FROM UR_MENU
                            WHERE {1} AND (FL = 1) AND (FG IN
                                (SELECT FG FROM (
                                (SELECT  A.FG
                                 FROM    UR_TACL A INNER JOIN
                                         UR_UIR B ON A.RLNO = B.RLNO
                                 WHERE  (A.V = 1) AND (B.TUSER = :TUSER))
                                UNION
                                (SELECT C.FG
                                 FROM   UR_TACL2 C
                                 WHERE  C.TUSER = :TUSER)
                                 )))
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, expanded ? "1" : "0", " PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser });
            }
            else
            {
                sql = string.Format(sql, expanded ? "1" : "0", " PG = :PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { PG = parent, TUSER = tuser });
            }
        }

        public IEnumerable<NestMenuView> GetMenuIndex2(NestMenuView parentItem, string parent, string tuser)
        {
            List<NestMenuView> result = new List<NestMenuView>();
            var menuViews = GetMenuLogin(parent, tuser);
            foreach(var menuView in menuViews)
            {
                NestMenuView nestMenuView = new NestMenuView();
                nestMenuView.item = menuView;
                if(!menuView.leaf)
                    nestMenuView.child = GetMenuIndex2(nestMenuView, menuView.FG, tuser);
                result.Add(nestMenuView);
            }
            return result;
        }

        public IEnumerable<MenuView> GetMenuByUser(string parent, string tuser)
        {
            var sql = @"SELECT FG, PG, FS, FG AS id,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 || '(' || F0 || ')' END AS text,
                            --CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            --CASE WHEN (PG IS NULL) THEN 1 ELSE 0 END AS expanded
                            0 AS expanded
                            FROM UR_MENU
                            WHERE {0} AND (FL = 1) AND (FG IN
                                (SELECT  A.FG
                                 FROM    UR_TACL A INNER JOIN
                                         UR_UIR B ON A.RLNO = B.RLNO
                                 WHERE  (A.V = 1) AND (B.TUSER = :TUSER)))
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, " PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser });
            }
            else
            {
                sql = string.Format(sql, " PG = :PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { PG = parent, TUSER = tuser });
            }
        }

        public IEnumerable<MenuView> GetMenuByUser2(string parent, string tuser, string manager)
        {
            var sql = @"SELECT A.FG, A.PG, A.FS, A.FD, A.FG AS id,B.V,B.R,B.U,B.P,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 || '(' || F0 || ')' END AS text,
                            --CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            0 AS expanded
                            FROM UR_MENU A LEFT OUTER JOIN UR_TACL2 B ON A.FG=B.FG AND B.TUSER=:TUSER AND B.TACL_CREATE_BY=:MGR 
                            WHERE {0}
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, " PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser, MGR = manager });
            }
            else
            {
                sql = string.Format(sql, " PG = :PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { PG = parent, TUSER = tuser, MGR = manager });
            }
        }

        public IEnumerable<MenuView> GetMenuByRole(string parent, string rlno)
        {
            var sql = @"SELECT A.FG, A.PG, A.FS, A.FD, A.FG AS id,B.V,B.R,B.U,B.P,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 || '(' || F0 || ')' END AS text,
                            --CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            0 AS expanded
                            FROM UR_MENU A LEFT OUTER JOIN UR_TACL B ON A.FG=B.FG AND B.RLNO=:RLNO
                            WHERE {0}
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, " A.PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { RLNO = rlno });
            }
            else
            {
                sql = string.Format(sql, " A.PG = :PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { RLNO = rlno, PG = parent });
            }
        }

        public IEnumerable<MenuView> GetMenuByRoleAndUser(string parent, string rlno, string tuser)
        {
            var sql = @"SELECT A.FG, A.PG, A.FS, A.FD, A.FG AS id,B.V,B.R,B.U,B.P,C.V HV,C.R HR,C.U HU,C.P HP,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 || '(' || F0 || ')' END AS text,
                            --CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            0 AS expanded
                            FROM 
                            (SELECT * FROM UR_MENU WHERE FG IN 
                                (SELECT FG FROM UR_TACL WHERE RLNO IN 
                                    (SELECT RLNO FROM UR_UIR WHERE TUSER=:TUSER) AND (R=1 OR U=1 OR P=1))) A 
                            LEFT OUTER JOIN UR_TACL B ON A.FG=B.FG AND B.RLNO=:RLNO
                            LEFT JOIN UR_TACL C ON A.FG=C.FG AND C.RLNO IN 
                                (SELECT RLNO FROM UR_UIR WHERE TUSER=:TUSER) AND (C.R=1 OR C.U=1 OR C.P=1)
                            WHERE {0}
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, " A.PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser, RLNO = rlno });
            }
            else
            {
                sql = string.Format(sql, " A.PG = :PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser, RLNO = rlno, PG = parent });
            }
        }

        public IEnumerable<MenuView> GetMenuByRoleAndUser2(string parent, string tuser, string manager)
        {
            var sql = @"SELECT A.FG, A.PG, A.FS, A.FD, A.FG AS id, 
                            NVL(B.V, '0') V, NVL(B.R, '0') R, NVL(B.U, '0') U, NVL(B.P, '0') P,
                            C.V HV,C.R HR,C.U HU,C.P HP,
                            CASE WHEN (FT = 'F') THEN F1 ELSE F1 || '(' || F0 || ')' END AS text,
                            --CASE WHEN (FT = 'F') THEN 'task-folder' ELSE 'task' END AS iconCls,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            0 AS expanded
                            FROM 
                            (SELECT * FROM UR_MENU WHERE FG IN 
                                (SELECT FG FROM 
                                    (SELECT FG FROM UR_TACL WHERE RLNO IN 
                                        (SELECT RLNO FROM UR_UIR WHERE TUSER=:MGR) AND (R=1 OR U=1 OR P=1)
                                    )
                                    UNION
                                    (SELECT FG FROM UR_TACL2 WHERE TUSER=:MGR AND (R=1 OR U=1 OR P=1)
                                    )
                                )
                            ) A 
                            LEFT OUTER JOIN UR_TACL2 B ON A.FG=B.FG AND B.TUSER=:TUSER AND B.TACL_CREATE_BY=:MGR 
                            LEFT JOIN (SELECT * FROM (
                                (SELECT FG,V,R,U,P FROM UR_TACL C1 WHERE C1.RLNO IN 
                                    (SELECT RLNO FROM UR_UIR WHERE TUSER=:MGR) AND (C1.R=1 OR C1.U=1 OR C1.P=1)
                                ) UNION
                                (SELECT FG,V,R,U,P FROM UR_TACL2 C2 WHERE C2.TUSER=:MGR AND (C2.R=1 OR C2.U=1 OR C2.P=1)
                                ))) C ON C.FG = A.FG AND (C.R=1 OR C.U=1 OR C.P=1) 
                            WHERE {0}
                            ORDER BY FS";
            if (parent == null || parent == "")
            {
                sql = string.Format(sql, " A.PG IS NULL ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser, MGR = manager });
            }
            else
            {
                sql = string.Format(sql, " A.PG = :PG ");
                return DBWork.Connection.Query<MenuView>(sql, new { TUSER = tuser, MGR = manager, PG = parent });
            }
        }

        public IEnumerable<MenuView> GetMenuByQuery(string menuName, string tuser)
        {
            var sql = @"SELECT FG as id,F1 || '(' || F0 || ')' AS text,
                        CASE WHEN (FT = 'F') THEN '' ELSE FA0||'/'||FG||DECODE(FUP,NULL,'','?'||FUP) END AS url,
                        FS FROM UR_MENU
                        WHERE FL = 1 AND FT = 'L' AND(UPPER(F0) LIKE UPPER('%' || :MENUNAME || '%') OR UPPER(F1) LIKE UPPER('%' || :MENUNAME || '%'))
                        AND FG IN (SELECT A.FG FROM UR_TACL A JOIN UR_UIR B ON A.RLNO = B.RLNO WHERE A.V = 1 AND B.TUSER =:TUSER
                        union SELECT FG FROM UR_TACL2 WHERE V = 1 AND TUSER =:TUSER)
                        AND PG IN (SELECT A.FG FROM UR_TACL A JOIN UR_UIR B ON A.RLNO = B.RLNO WHERE A.V = 1 AND B.TUSER =:TUSER
                        union SELECT FG FROM UR_TACL2 WHERE V = 1 AND TUSER =:TUSER)
                        AND PG <> 'UT0000'
                        ORDER BY FG ";
            var a = sql;
            return DBWork.Connection.Query<MenuView>(sql, new { MENUNAME = menuName, TUSER = tuser });
        }
        public bool CheckExists(string fg)
        {
            string sql = @"SELECT 1 FROM UR_MENU WHERE FG=:FG";
            return !(DBWork.Connection.ExecuteScalar(sql, new { FG = fg }, DBWork.Transaction) == null);
        }

        public IEnumerable<UR_MENU> Get(string fg)
        {
            var sql = @"SELECT FG, PG, SG, FS, FT, F0, F1, FA0, FU, FUP, FD, FL, FG AS id, ATTACH_URL,
                            CASE WHEN (FT = 'F') THEN 0 ELSE 1 END AS leaf,
                            0 AS expanded 
                            from UR_MENU where FG=:FG";
            return DBWork.Connection.Query<UR_MENU>(sql, new { FG = fg }, DBWork.Transaction);
        }

        public string GetUrl(string fg)
        {
            var sql = @"SELECT FU||'.js'||DECODE(FUP,NULL,'','?'||FUP) FROM UR_MENU where FG=:FG";
            return DBWork.Connection.ExecuteScalar(sql, new { FG = fg }, DBWork.Transaction)?.ToString()??"";
        }

        public decimal GetNextFS(string pg)
        {
            var sql = @"SELECT NVL((MAX(FS) + 1), 0) FS FROM UR_MENU WHERE PG=:PG";

            return decimal.Parse(DBWork.Connection.ExecuteScalar(sql, new { PG = pg }, DBWork.Transaction).ToString());
        }

        public int Create(UR_MENU ur_menu)
        {
            var _afrs = 0;
            var sql = @"INSERT INTO UR_MENU (FG, PG, SG, FS, FT, F0, F1, FA0, FU, FUP, FD, FL, ATTACH_URL)
                        VALUES (:FG, :PG, :SG, :FS, :FT, :F0, :F1, :FA0, :FU, :FUP, :FD, :FL, :ATTACH_URL)";

            _afrs = DBWork.Connection.Execute(sql, ur_menu, DBWork.Transaction);

            return _afrs;
        }

        public int Update(UR_MENU ur_menu)
        {
            var _afrs = 0;
            var sql = @"UPDATE UR_MENU 
                            SET FT = :FT,
                            F0 = :F0,
                            F1 = :F1,
                            FA0 = :FA0,
                            FU = :FU,
                            FUP = :FUP,
                            FD = :FD,
                            FL = :FL,
                            ATTACH_URL = :ATTACH_URL 
                            WHERE FG=:FG";

            _afrs = DBWork.Connection.Execute(sql, ur_menu, DBWork.Transaction);

            return _afrs;
        }

        public int UpdateFS(UR_MENU ur_menu)
        {
            var _afrs = 0;
            var sql = @"UPDATE UR_MENU SET FS = :FS WHERE FG=:FG";

            _afrs = DBWork.Connection.Execute(sql, ur_menu, DBWork.Transaction);

            return _afrs;
        }

        public int Delete(string fg)
        {
            var _afrs = 0;
            var sql = @"DELETE FROM UR_MENU WHERE FG=:FG";

            _afrs = DBWork.Connection.Execute(sql, new { FG = fg }, DBWork.Transaction);

            return _afrs;
        }
    }
}