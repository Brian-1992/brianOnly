using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;

namespace WebApp.Repository.C
{
    public class CB0006Repository : JCLib.Mvc.BaseRepository
    {
        public CB0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_ITMANAGER> GetAll(string wh_no, string mmcode, string mmname_c, string mmname_e, string status, string mat_class, string managerid,  int page_index, int page_size, string sorters, string wh_userId)
        {
            var p = new DynamicParameters();

            //var sql = @"SELECT (select mat_class || ' ' || mat_clsname from mi_matclass where mat_class=A1.mat_class) mat_clsname,
            //                   A1.*,
            //                   NVL(d.una, '--') user_name,
            //                   NVL('[' || A1.MANAGERID || ']' || A1.MANAGERNM || ' => ' || NVL(d.una, '') || '', '') as OPT 
            //             from (SELECT A2.*, c.MANAGERNM, c.USERID  
            //                     FROM (SELECT a.wh_no, 
            //                                  a.mmcode, 
            //                                  (select mat_class from MI_MAST where mmcode=a.mmcode) as mat_class,
            //                                  (select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c,
            //                                  (select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e, 
            //                                  b.MANAGERID,
            //                                  b.MANAGERID AS OLD_MANAGERID
            //                             FROM MI_WHINV a 
            //                             LEFT OUTER JOIN BC_ITMANAGER b ON a.mmcode = b.mmcode and a.wh_no=b.wh_no
            //                            WHERE a.WH_NO = :p0)   A2 
            //             LEFT OUTER JOIN BC_MANAGER c ON A2.MANAGERID = c.MANAGERID and A2.WH_NO = c.WH_NO) A1 
            //             LEFT OUTER JOIN UR_ID d ON A1.USERID = d.TUSER
            //             WHERE 1=1 ";

            var sql = @"SELECT (select mat_class || ' ' || mat_clsname from mi_matclass where mat_class=A1.mat_class) mat_clsname,
                               A1.*,
                               (select UNA from UR_ID where A1.MANAGERID=TUSER) as managernm 
                         from (select a.wh_no, 
                                      a.mmcode, 
                                      (select mat_class from MI_MAST where mmcode=a.mmcode) as mat_class,
                                      (select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c,
                                      (select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e, 
                                      b.MANAGERID,
                                      b.MANAGERID AS OLD_MANAGERID,
                                      (select listagg(store_loc,';')
                                              within group (order by store_loc)
                                         from MI_WLOCINV
                                        where WH_NO=a.WH_NO and MMCODE=a.mmcode) as store_loc
                                 from MI_WHINV a 
                                 left outer join BC_ITMANAGER b 
                                   on a.mmcode = b.mmcode and a.wh_no=b.wh_no
                                where a.WH_NO = :p0
                              ) A1
                         WHERE 1=1 ";


            p.Add(":p0", string.Format("{0}", wh_no));

            if (mmcode != "")
            {
                sql += " and A1.MMCODE like :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (mmname_c != "")
            {
                sql += " and A1.mmname_c  like :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmname_c));
            }
            if (mmname_e != "")
            {
                sql += " and A1.mmname_e  like :p3 ";
                p.Add(":p3", string.Format("%{0}%", mmname_e));
            }
            if (status == "Y") {
                sql += " and MANAGERID is not null ";
            }
            if (status == "N")
            {
                sql += " and MANAGERID is null ";
            }
            if (managerid != "") {
                sql += " and A1.managerid  = :p4 ";
                p.Add(":p4", string.Format("{0}", managerid));
            }
            if (mat_class != "")
            {
                sql += " and A1.mat_class = :p5 ";
                p.Add(":p5", string.Format("{0}", mat_class));
            }

            sql += "order by A1.wh_no,mat_clsname,A1.mmcode";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_ITMANAGER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string ts)
        {
            var ss = ts.Split(',');
            var wh_no = ss[0];
            var mmcode = ss[1];
            var mmname_c = ss[2];
            var mmname_e = ss[3];
            var status = ss[4];
            var mat_class = ss[5];
            var managerid = ss[6];

            var p = new DynamicParameters();
            // DataTable dt = new DataTable();
            var sql = @"select AA.庫房別,AA.物料分類代碼,AA.物料分類,AA.院內碼,AA.管理員姓名, AA.中文品名,AA.英文品名, AA.儲位
                          From (SELECT (select mat_clsname from mi_matclass where mat_class=A1.物料分類代碼) as 物料分類, 
                                       A1.*
                                  from (select a.wh_no as 庫房別, 
                                               a.mmcode as 院內碼, 
                                               (select mat_class from MI_MAST where mmcode=a.mmcode) as 物料分類代碼,
                                               (select mmname_c from MI_MAST where mmcode=a.mmcode)  as 中文品名,
                                               (select mmname_e from MI_MAST where mmcode=a.mmcode)  as 英文品名, 
                                               b.MANAGERID as 管理員代碼,
                                                (select una from UR_ID where tuser = b.managerid) as 管理員姓名,
                                               (select listagg(store_loc,',')
                                                       within group (order by store_loc)
                                                  from MI_WLOCINV
                                                 where WH_NO=a.WH_NO and MMCODE=a.mmcode) as 儲位
                                          from MI_WHINV a 
                                          left outer join BC_ITMANAGER b 
                                            on a.mmcode = b.mmcode and a.wh_no=b.wh_no
                                         where a.WH_NO = :p0
                                       ) A1 
                        where 1=1 ";
            //var sql= @"SELECT rownum as 項次,(select mat_clsname from mi_matclass where mat_class=A1.mat_class)as 物料分類,A1.* 
            //             from (SELECT A2.*, c.MANAGERNM, c.USERID  FROM(SELECT a.wh_no as 庫房別, a.mmcode as 院內碼, (select mat_class from MI_MAST where mmcode = a.mmcode) as mat_class,
            //            (select mmname_c from MI_MAST where mmcode = a.mmcode) as 中文品名,(select mmname_e from MI_MAST where mmcode = a.mmcode)  as 英文品名, b.MANAGERID as 品項管理員
            //                 FROM MI_WHINV a LEFT OUTER JOIN BC_ITMANAGER b ON a.mmcode = b.mmcode and a.wh_no = b.wh_no )   A2
            //                 LEFT OUTER JOIN BC_MANAGER c  ON 品項管理員 = c.MANAGERID) A1 LEFT OUTER JOIN UR_ID d ON A1.USERID = d.TUSER
            //            Where 1 = 1";

            p.Add(":p0", string.Format("{0}", wh_no));

            if (mmcode != "" && mmcode != "null")
            {
                sql += " and A1.院內碼 like :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (mmname_c != "" && mmname_c != "null")
            {
                sql += " and A1.中文品名  like :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmname_c));
            }
            if (mmname_e != "" && mmname_e != "null")
            {
                sql += " and A1.英文品名  like :p3 ";
                p.Add(":p3", string.Format("%{0}%", mmname_e));
            }
            if (status == "Y")
            {
                sql += " and 管理員代碼 is not null ";
            }
            if (status == "N")
            {
                sql += " and 管理員代碼 is null ";
            }
            if (managerid != "" && managerid != "null")
            {
                sql += " and A1.管理員代碼  = :p4 ";
                p.Add(":p4", string.Format("{0}", managerid));
            }
            if (mat_class != "" && mat_class != "null")
            {
                sql += " and A1.物料分類代碼 = :p5 ";
                p.Add(":p5", string.Format("{0}", mat_class));
            }

            sql += " ) AA  order by 庫房別,物料分類代碼,院內碼";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int Create(BC_ITMANAGER bc_itmanager)
        {


            var sql = @"INSERT INTO BC_ITMANAGER (WH_NO, MMCODE, MANAGERID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        VALUES (:WH_NO, :MMCODE, :MANAGERID, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, bc_itmanager, DBWork.Transaction);
        }

        public int Update(BC_ITMANAGER bc_itmanager)
        {
            var sql = @"UPDATE BC_ITMANAGER
                           SET MANAGERID = :MANAGERID, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE WH_NO = :wh_no AND MMCODE = :mmcode";
            return DBWork.Connection.Execute(sql, bc_itmanager, DBWork.Transaction);
        }

        public int Delete(BC_ITMANAGER bc_itmanager)
        {
            var sql = @"DELETE FROM BC_ITMANAGER
                         WHERE WH_NO = :wh_no AND MMCODE = :mmcode AND MANAGERID = :old_managerid";
            //var req = new { WH_NO = bc_itmanager.WH_NO, MMCODE = bc_itmanager.MMCODE, MANAGERID = bc_itmanager.OLD_MANAGERID };
            //return DBWork.Connection.Execute(sql, req, DBWork.Transaction);
            return DBWork.Connection.Execute(sql, bc_itmanager, DBWork.Transaction);
        }

        public bool CheckExists(string wh_no, string mmcode, string managerid)
        {
            string sql = @"SELECT 1 FROM BC_ITMANAGER 
                            WHERE WH_NO = :wh_no 
                              AND MMCODE = :mmcode 
                              AND MANAGERID = :managerid ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode, MANAGERID = managerid}, DBWork.Transaction) == null);
        }

        public IEnumerable<ComboItemModel> GetManageridCombo(string wh_no) {
            //string sql = @"SELECT a.MANAGERID as VALUE, 
            //                     '[' || a.MANAGERID || '] ' || a.MANAGERNM || ' =>' || NVL(c.una, '--') || '' as TEXT   
            //                 FROM BC_MANAGER a, MI_WHID b 
            //                 LEFT OUTER JOIN UR_ID  c on b.WH_USERID = c.tuser
            //                 WHERE a.MANAGERID = b.WH_USERID 
            //                   AND a.WH_NO = :WH_NO 
            //                   AND b.WH_NO = a.WH_NO 
            //                ORDER BY a.MANAGERID";
            string sql = @"SELECT tuser as VALUE,
                                  tuser || ' ' || una as TEXT
                             FROM UR_ID
                            WHERE inid = :wh_no
                            ORDER BY tuser";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_NO = wh_no}, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLASS || ' ' || MAT_CLSNAME AS TEXT
                             FROM MI_MATCLASS
                            ORDER BY MAT_CLASS";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }
        public IEnumerable<string> GetUserInid(string tuser) {
            string sql = @"SELECT INID
                             FROM UR_ID 
                            WHERE TUSER = :tuser";
            return DBWork.Connection.Query<string>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWhnoCombo(string tuser) {
            string sql = @"SELECT a.INID AS VALUE, a.INID ||' '|| b.WH_NAME AS TEXT
                             FROM UR_ID a, MI_WHMAST b
                            WHERE a.TUSER = :tuser
                              AND b.WH_NO = a.INID";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }
    }
}