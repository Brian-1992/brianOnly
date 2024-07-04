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
    public class AB0111Repository : JCLib.Mvc.BaseRepository
    {
        public AB0111Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public string GetSql(string crdocno, string mmcode, string agen_no,
                                          string start_date, string end_date, string status,
                                          string userId, bool isAA, bool isExcel)
        {
            string sql = string.Format(@"
                select crdocno, mmcode, mmname_c, mmname_e, appqty,
                       base_unit, towh, (towh||' '||wh_name) as wh_name, twn_date(reqdate) as reqdate, drname,
                       patientname, chartno, cr_uprice,  agen_no,
                       (select data_value||' '|| data_desc from PARAM_D 
                         where data_value = a.m_paykind
                           and grp_code = 'MI_MAST' 
                           and data_name = 'M_PAYKIND') as m_paykind, 
                       inid, 
                       appid,  user_name(appid) as appid_name , twn_time(apptime) as apptime, email, wexp_id,
                       m_contprice, uprice, 
                       (select data_value||' '|| data_desc from PARAM_D 
                         where data_value = a.cr_status
                           and grp_code = 'CR_DOC' 
                           and data_name = 'CR_STATUS') as cr_status, 
                       twn_time(ordertime) as ordertime, orderid, user_name(orderid) as orderid_name,
                       twn_time(emailtime) as emailtime, twn_time(replytime) as replytime,
                       inqty, lot_no, twn_date(exp_date) as exp_date,
                       ackmmcode, ackqty, twn_time(acktime) as acktime, ackid, user_name(ackid) as ackid_name,
                       backqty, twn_time(backtime) as backtime, backid, user_name(backid) as backid_name, --iscfm,
                       cfmqty, twn_time(cfmtime) as cfmtime, cfmid, user_name(cfmid) as cfmid_name,
                       twn_time(purapptime) as purapptime, 
                       twn_time(puracctime) as puracctime, 
                       create_time, create_user, update_time, update_user, update_ip,
                       (select (agen_no||' '||agen_namec) from PH_VENDER
                         where agen_no=a.agen_no) as agen_combine,
                       (select agen_namec from PH_VENDER
                         where agen_no=a.agen_no) as agen_namec,
                       (select agen_tel from PH_VENDER
                         where agen_no=a.agen_no) as agen_tel,
                       (select agen_boss from PH_VENDER
                         where agen_no=a.agen_no) as agen_boss,
                       (select listagg('批號-' || lot_no || ' 效期-' || twn_date(exp_date) || ' 數量-' || inqty, '{0}')
                                within group (order by cr_d_seq) 
                          from CR_DOC_D
                         where crdocno = a.crdocno
                         group by crdocno
                        ) as detail,
                       rdocno, small_dn, pr_no, po_no
                  from CR_DOC a
                 where 1=1
            ", isExcel ? "," : "<br>");

            if (string.IsNullOrEmpty(crdocno) == false)
            {
                sql += "  and a.crdocno = :crdocno";
            }
            if (string.IsNullOrEmpty(mmcode) == false)
            {
                sql += "  and a.mmcode = :mmcode";
            }
            if (string.IsNullOrEmpty(agen_no) == false)
            {
                sql += "  and a.agen_no = :agen_no";
            }
            if (string.IsNullOrEmpty(start_date) == false)
            {
                sql += "  and twn_date(a.reqdate) >= :start_date";
            }
            if (string.IsNullOrEmpty(end_date) == false)
            {
                sql += "  and twn_date(a.reqdate) <= :end_date";
            }
            if (string.IsNullOrEmpty(status) == false)
            {
                sql += string.Format("  and a.cr_status in ( {0} )" , status);
            }
            if (isAA == false)
            {
                sql += @"
                   and towh in
                        (select a.wh_no
                           from MI_WHMAST a
                          where wh_kind='1' and wh_grade > '1'
                            and exists
                                 (select 'X' from UR_ID b 
                                   where (a.supply_inid=b.inid or a.inid=b.inid)
                                     and tuser=:userId)
                            and not exists
                                 (select 'X' from MI_WHID b
                                   where task_id in ('2','3')
                                     and wh_userid=:userId)
                            and nvl(cancel_id,'N')='N'
                         union all 
                         select a.wh_no
                           from MI_WHMAST a,MI_WHID b
                          where a.wh_no=b.wh_no and task_id in ('2','3')
                            and wh_userid=:userid
                            and nvl(cancel_id,'N')='N'
                            and a.wh_grade>'1'
                        )
                ";
            }
            return sql;
        }

        public IEnumerable<AB0111> GetAll(string crdocno, string mmcode, string agen_no, 
                                          string start_date, string end_date, string status, 
                                          string userId, bool isAA) {
            string sql = GetSql(crdocno, mmcode, agen_no,
                                start_date, end_date, status,
                                userId, isAA, false);

            return DBWork.PagingQuery<AB0111>(sql, new
            {
                crdocno,
                mmcode,
                agen_no,
                start_date,
                end_date,
                status,
                userId
            }, DBWork.Transaction);
        }

        public IEnumerable<AB0111> GetDetails(string crdocno) {
            string sql = @"
                select lot_no, twn_date(exp_date) as exp_date, inqty, isudi
                  from CR_DOC_D
                 where crdocno = :crdocno
            ";
            return DBWork.PagingQuery<AB0111>(sql, new { crdocno }, DBWork.Transaction);
        }

        public int UpdatePtName(string crdocno, string drName, string ptName, string chartNo, string userId, string updateIp) {
            string sql = @"
                update CR_DOC
                   set drName = :drName,
                       patientName = :ptName,
                       chartNo = :chartNo,
                       update_time = sysdate,
                       update_user = :userId,
                       update_ip = :updateIp
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, drName, ptName, chartNo, userId, updateIp }, DBWork.Transaction);
        }

        #region combo
        public IEnumerable<COMBO_MODEL> GetAgens() {
            string sql = @"
                select (agen_no || ' ' || agen_namec) as text,  --下拉選單顯示
                       agen_no as value  --where使用
                  from PH_VENDER
                 where rec_status='A'  --修改狀態碼:A啟用,X刪除 
                 order by agen_no
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetStatus()
        {
            string sql = @"
                select data_value || ' ' || data_desc as text,
                       data_value as value
                  from PARAM_D
                 where grp_code = 'CR_DOC' 
                   and data_name = 'CR_STATUS'
                 order by data_value
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        #endregion

        #region print
        public IEnumerable<AB0111> GetPrint(string crdocno) {
            string sql = @"
                 select crdocno, mmcode, mmname_c, mmname_e, appqty,
                       base_unit, towh, (towh||' '||wh_name) as wh_name, twn_date(reqdate) as reqdate, drname,
                       (case when length(patientName) = 2 
                           then substr(patientName, 1,1)||'○'
                           else substr(patientName, 1,1)|| replace(LPAD(' ', length(patientName)-1,'○'),' ','')||substr(patientName, length(patientName),1)
                       end) as patientName,
                       chartno, cr_uprice,  agen_no,
                       (select data_value||' '|| data_desc from PARAM_D 
                         where data_value = a.m_paykind
                           and grp_code = 'MI_MAST' 
                           and data_name = 'M_PAYKIND') as m_paykind, 
                       inid, 
                       appid,  user_name(appid) as appid_name , twn_time(apptime) as apptime, email, wexp_id,
                       m_contprice, uprice, 
                       (select data_value||' '|| data_desc from PARAM_D 
                         where data_value = a.cr_status
                           and grp_code = 'CR_DOC' 
                           and data_name = 'CR_STATUS') as cr_status, 
                       twn_time(ordertime) as ordertime, orderid, user_name(orderid) as orderid_name,
                       twn_time(emailtime) as emailtime, twn_time(replytime) as replytime,
                       inqty, lot_no, exp_date,
                       ackmmcode, ackqty, twn_time(acktime) as acktime, ackid, user_name(ackid) as ackid_name,
                       backqty, twn_time(backtime) as backtime, backid, user_name(backid) as backid_name, --iscfm,
                       cfmqty, twn_time(cfmtime) as cfmtime, cfmid, user_name(cfmid) as cfmid_name,
                       twn_time(purapptime) as purapptime, 
                       twn_time(puracctime) as puracctime, 
                       create_time, create_user, update_time, update_user, update_ip,
                       (select (agen_no||' '||agen_namec) from PH_VENDER
                         where agen_no=a.agen_no) as agen_combine,
                       (select agen_namec from PH_VENDER
                         where agen_no=a.agen_no) as agen_namec,
                       (select agen_tel from PH_VENDER
                         where agen_no=a.agen_no) as agen_tel,
                       (select agen_boss from PH_VENDER
                         where agen_no=a.agen_no) as agen_boss
                  from CR_DOC a
                 where 1=1
                   and crdocno = :crdocno
            ";
            return DBWork.Connection.Query<AB0111>(sql, new { crdocno }, DBWork.Transaction);
        }
        #endregion

        #region excel

        public DataTable GetExcel(string crdocno, string mmcode, string agen_no,
                                         string start_date, string end_date, string status,
                                         string userId, bool isAA)
        {
            string sql = GetSql(crdocno, mmcode, agen_no,
                                start_date, end_date, status,
                                userId, isAA, true);

            sql = string.Format(@"
                    select crdocno as 緊急醫療出貨單編號, mmcode as 申請院內碼, mmname_c as 中文品名,
                           mmname_e as 英文品名, appqty as 申請數量,
                           base_unit as 計量單位, --towh,
                           wh_name as 入庫庫房名稱, reqdate as 要求到貨日, drname as 使用醫師,
                           patientname as 病人姓名, chartno as 病人病歷號, cr_uprice as 單價,  agen_no as 廠商代碼,
                           m_paykind as 收費屬性, 
                           inid as 庫房責任中心, 
                           --appid,
                           appid_name as 申請人, apptime as 申請時間, email as 廠商EMAIL, wexp_id as 批號效期註記,
                           m_contprice as 合約單價, uprice as 最小單價計量單位單價, 
                           cr_status as 狀態,
                           ordertime as 產生通知單時間, --orderid,
                           orderid_name as 產生通知單人員,
                           emailtime as 寄EMAIL時間, replytime as 收信確認時間,
                           inqty as 進貨量, lot_no as 廠商回覆批號, exp_date as 廠商回覆效期,
                           ackmmcode as 點收院內碼, ackqty as 點收量, acktime as 點收時間, --ackid,
                           ackid_name as 點收人,
                           backqty as 退回量, backtime as 退回時間, --backid,
                           backid_name as 退回人,
                           cfmqty as 結驗量, cfmtime as 結驗時間, --cfmid,
                           cfmid_name as 結驗人,
                           purapptime as 產生採購申請單時間,
                           puracctime as 採購進貨接收時間,
                           --create_time, create_user, update_time, update_user, update_ip,
                           --agen_combine,
                           agen_namec as 廠商名稱,
                           agen_tel as 廠商電話,
                           agen_boss as 廠商負責人,
                           detail as 結驗批號效期
                      from ( {0} )
                     order by crdocno
                  ", sql);

            var p = new DynamicParameters();
            p.Add(":crdocno", crdocno);
            p.Add(":mmcode", mmcode);
            p.Add(":agen_no", agen_no);
            p.Add(":start_date", start_date);
            p.Add(":end_date", end_date);
            p.Add(":status", status);
            p.Add(":userId", userId);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        #endregion
    }
}