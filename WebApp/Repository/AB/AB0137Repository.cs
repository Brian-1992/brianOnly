using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Types;
using System.Linq;
using WebApp.Models.AB;


namespace WebApp.Repository.AB
{
    public class AB0137Repository : JCLib.Mvc.BaseRepository
    {
        public AB0137Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0137> GetAll(string USEDATE_B, string USEDATE_E, string STOCKCODE, string MMCODE, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"select USEDATE,STOCKCODE,MMCODE,USEQTY, BASE_UNIT,
                           (select MMNAME_E from mi_mast where mmcode=a.mmcode) as MMNAME_E,
                           (select MMNAME_C from mi_mast where mmcode=a.mmcode) as MMNAME_C,
                           TWN_TIME(CREATE_TIME) as CREATE_TIME
                    from HIS14_TADDET2 a
                    where 1=1
                    ";


            if (!string.IsNullOrWhiteSpace(USEDATE_B))
            {
                sql += " AND USEDATE >= :USEDATE_B ";
                DateTime tempDate = DateTime.Parse(USEDATE_B);
                string date = (tempDate.Year - 1911).ToString() + tempDate.Month.ToString().PadLeft(2, '0') + tempDate.Day.ToString().PadLeft(2, '0');
                p.Add(":USEDATE_B", string.Format("{0}", date));
            }
            if (!string.IsNullOrWhiteSpace(USEDATE_E))
            {
                sql += "AND USEDATE <= :USEDATE_E ";
                DateTime tempDate = DateTime.Parse(USEDATE_E);
                string date = (tempDate.Year - 1911).ToString() + tempDate.Month.ToString().PadLeft(2, '0') + tempDate.Day.ToString().PadLeft(2, '0');
                p.Add(":USEDATE_E", string.Format("{0}", date));
            }

            if (!string.IsNullOrWhiteSpace(STOCKCODE))
            {
                sql += " AND STOCKCODE =:STOCKCODE ";
                p.Add(":STOCKCODE", string.Format("{0}", STOCKCODE));
            }
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " AND MMCODE =:MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0137>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

                                 
        public int Insert(AB0137 data)
        {
            string sql = @"Insert into HIS14_TADDET2
                          (ID, USEDATE, DET_STKKIND, DET_KIND, STOCKCODE, MMCODE,
                           USEQTY, BASE_UNIT, STOCKFLAG,
                           CREATE_TIME, CREATE_USER, UPDATE_IP)
                        Values
                          (HIS14TADDET2_SEQ.nextval, twn_date(sysdate),'1','1',:STOCKCODE,:MMCODE,
                           :USEQTY,(select base_unit from mi_mast where mmcode=:MMCODE),'Y',
                           Sysdate,:CREATE_USER,:UPDATE_IP)
            ";
            return DBWork.Connection.Execute(sql, data, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWh_noCombo()
        {
            var p = new DynamicParameters();


            string sql = @"select wh_no || ' ' || wh_name TEXT ,wh_no VALUE from MI_WHMAST where wh_kind='1' and wh_grade='1' ";

            sql += "order by VALUE ";

            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<AB0137> GetMmcodeData(List<AB0137> AB0137List)
        {
            string sql = @" SELECT mmcode,MMNAME_E,MMNAME_C,MAT_CLASS FROM MI_MAST 
                            WHERE MMCODE in :MMCODE ";

            List<string> mmcodeList = new List<string>();

            foreach (var data in AB0137List)
            {
                mmcodeList.Add(data.MMCODE);
            }

            return DBWork.Connection.Query<AB0137>(sql, new { MMCODE = mmcodeList }, DBWork.Transaction);
        }

        public bool CheckExistsMMCODE(string mmcode)
        {
            string sql = @" SELECT 1 FROM MI_MAST 
                          WHERE MMCODE = :MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }


        public bool CheckFlagMMCODE(string mmcode)
        {
            string sql = @" SELECT nvl(cancel_id,'N') FROM MI_MAST 
                          WHERE MMCODE = :MMCODE ";
            return DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction).ToString() == "N";
        }


        public int GetUnitRate(string mmcode)
        {
            string sql = @"
                select unitRate from MI_MAST where mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, new { mmcode }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                        FROM MI_MAST A 
                        WHERE 1=1 
                          and a.mat_class = '01'
                         ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

    }
}