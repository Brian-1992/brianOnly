using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
namespace WebApp.Repository.B
{
    public class BD0006_MODEL : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string ISBACK { get; set; }
        public string ISBACK_DT { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_TEL { get; set; }
        public string PO_STATUS { get; set; }
        public string MAT_CLASS { get; set; }
        public string EMAIL { get; set; }
        public string REPLY_DT { get; set; }
        public string MAT_CLASS_NAME { get; set; }
    }
    public class BD0006Repository : JCLib.Mvc.BaseRepository
    {
        public BD0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<MM_PO_M> GetMasterAll(string StartDate, string EndDate, string mat_class, bool isHospCode0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = string.Empty;
            if (isHospCode0) {
                sql = @"select distinct TWN_DATE(PO_TIME) as PO_TIME_N, MAT_CLASS,  '已回覆' as ISBACK,
                         (select count(*)   FROM MM_PO_M 
                          where  po_no in  (select po_no  from mm_po_m
                              where  TWN_DATE(po_time) = TWN_DATE(a.po_time)             
                                and substr(po_no,1,3) in ('INV','GEN') and mat_class=:p4
                                and isback='Y') ) as CNT,
                         (select count(distinct po_no)  FROM  PH_REPLY r
                          where  po_no in  (select po_no  from mm_po_m
                              where   TWN_DATE(po_time) = TWN_DATE(a.po_time)                
                                and substr(po_no,1,3) in ('INV','GEN') and mat_class=:p4
                                and isback='Y') ) as REPLY_CNT  
                       from mm_po_m a
                       where TWN_DATE(po_time) BETWEEN :StartDate AND :EndDate             
                           and substr(po_no,1,3) in ('INV','GEN') and mat_class=:p4
                          and isback='Y'         
                       union
                       select  distinct TWN_DATE(PO_TIME) as PO_TIME_N, MAT_CLASS,  '未回覆' as ISBACK,
                         (select count(*) FROM MM_PO_M 
                          where   po_no in  (select po_no  from mm_po_m
                              where  TWN_DATE(po_time) = TWN_DATE(a.po_time)              
                                and substr(po_no,1,3) in ('INV','GEN') and mat_class=:p4
                                and isback='N') ) as CNT,
                         (select count(distinct po_no)  FROM  PH_REPLY 
                          where  po_no in  (select po_no  from mm_po_m
                              where   TWN_DATE(po_time) = TWN_DATE(a.po_time)             
                                and substr(po_no,1,3) in ('INV','GEN') and mat_class=:p4
                                and isback='N') ) as REPLY_CNT         
                       from mm_po_m a
                       where TWN_DATE(po_time) BETWEEN :StartDate AND :EndDate             
                         and substr(po_no,1,3) in ('INV','GEN') and mat_class=:p4
                         and isback='N'  
                       order by PO_TIME_N ";
            }
            else {
                sql = @"select distinct TWN_DATE(PO_TIME) as PO_TIME_N, MAT_CLASS, 
                                    (select mat_class||' '||mat_clsname from MI_MATCLASS where mat_class=a.mat_class) mat_class_name,
                                '已回覆' as ISBACK,
                         (select count(*)   FROM MM_PO_M 
                          where  po_no in  (select po_no  from mm_po_m
                              where  TWN_DATE(po_time) = TWN_DATE(a.po_time)             
                               and mat_class=:p4
                                and isback='Y') ) as CNT,
                         (select count(distinct po_no)  FROM  PH_REPLY r
                          where  po_no in  (select po_no  from mm_po_m
                              where   TWN_DATE(po_time) = TWN_DATE(a.po_time)                
                                and mat_class=:p4
                                and isback='Y') ) as REPLY_CNT  
                       from mm_po_m a
                       where TWN_DATE(po_time) BETWEEN :StartDate AND :EndDate             
                           and mat_class=:p4
                          and isback='Y'         
                       union
                       select  distinct TWN_DATE(PO_TIME) as PO_TIME_N, MAT_CLASS, 
                                (select mat_class||' '||mat_clsname from MI_MATCLASS where mat_class=a.mat_class) mat_class_name,
                          '未回覆' as ISBACK,
                         (select count(*) FROM MM_PO_M 
                          where   po_no in  (select po_no  from mm_po_m
                              where  TWN_DATE(po_time) = TWN_DATE(a.po_time)              
                                and mat_class=:p4
                                and isback='N') ) as CNT,
                         (select count(distinct po_no)  FROM  PH_REPLY 
                          where  po_no in  (select po_no  from mm_po_m
                              where   TWN_DATE(po_time) = TWN_DATE(a.po_time)             
                                and mat_class=:p4
                                and isback='N') ) as REPLY_CNT         
                       from mm_po_m a
                       where TWN_DATE(po_time) BETWEEN :StartDate AND :EndDate             
                         and mat_class=:p4
                         and isback='N'  
                       order by PO_TIME_N ";
            }

            p.Add("p4", mat_class);
            p.Add("StartDate", StartDate);
            p.Add("EndDate", EndDate);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PO_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BD0006_MODEL> GetDetailAll(string PO_TIME, string ISBACK, string MAT_CLASS, bool isHospCode0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = string.Empty;
            if (isHospCode0)
            {
                sql = @"select po_no, 
                       (select TWN_TIME_FORMAT(min(back_dt)) from PH_MAILBACK where AGEN_NO=a.AGEN_NO and PO_NO=a.PO_NO) as ISBACK_DT, 
                       (select TWN_TIME_FORMAT(min(create_time)) from PH_REPLY where AGEN_NO=a.AGEN_NO and PO_NO=a.PO_NO) as REPLY_DT,                          
                    a.agen_no, b.agen_namec, b.agen_tel, b.email, 
                    (select data_desc from PARAM_D where  DATA_NAME='PO_STATUS' and DATA_VALUE=a.po_status) as PO_STATUS
                    from MM_PO_M a, PH_VENDER b
                    where substr(a.po_no,1,3) in ('INV','GEN') and a.agen_no=b.agen_no and TWN_DATE(po_time) = :PO_TIME and isback = :ISBACK and mat_class=:MAT_CLASS
                    order by po_no, a.agen_no";
            }
            else {
                sql = @"select po_no, 
                       (select TWN_TIME_FORMAT(min(back_dt)) from PH_MAILBACK where AGEN_NO=a.AGEN_NO and PO_NO=a.PO_NO) as ISBACK_DT, 
                       (select TWN_TIME_FORMAT(min(create_time)) from PH_REPLY where AGEN_NO=a.AGEN_NO and PO_NO=a.PO_NO) as REPLY_DT,                          
                    a.agen_no, b.agen_namec, b.agen_tel, b.email, 
                    (select data_desc from PARAM_D where  DATA_NAME='PO_STATUS' and DATA_VALUE=a.po_status) as PO_STATUS
                    from MM_PO_M a, PH_VENDER b
                    where 1=1 and a.agen_no=b.agen_no and TWN_DATE(po_time) = :PO_TIME and isback = :ISBACK and mat_class=:MAT_CLASS
                    order by po_no, a.agen_no";
            }

            //已回覆=Y  其餘=N
            ISBACK = ISBACK == "已回覆" ? "Y" : "N";

            p.Add(":PO_TIME", PO_TIME);
            p.Add(":ISBACK", ISBACK);
            p.Add(":MAT_CLASS", MAT_CLASS);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0006_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int SendEmail(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = :PO_STATUS, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";
            return DBWork.Connection.Execute(sql, new { PO_NO = mm_po_m.PO_NO, PO_STATUS = mm_po_m.PO_STATUS, UPDATE_USER = mm_po_m.UPDATE_USER, UPDATE_IP = mm_po_m.UPDATE_IP }, DBWork.Transaction);
        }

        public DataTable GetExcel(string PO_TIME, string ISBACK, string MAT_CLASS, bool isHospCode0)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = string.Empty;
            if (isHospCode0)
            {
                sql = @"select po_no 訂單編號, MAT_CLASS 物料分類,
                    (case when isback='Y' then '已回覆' when isback='N' then '未回覆' end) 回覆狀態, 
                    a.agen_no 廠商代碼, b.agen_namec 廠商名稱, b.agen_tel 廠商電話,
                    (select data_desc from PARAM_D where  DATA_NAME='PO_STATUS' and DATA_VALUE=a.po_status) as 狀態,
                    b.EMAIL 
                    from MM_PO_M a, PH_VENDER b
                    where substr(a.po_no,1,3) in ('INV','GEN') and a.agen_no=b.agen_no and TWN_DATE(po_time) = :PO_TIME and isback = :ISBACK
                      and MAT_CLASS=:MAT_CLASS
                    order by po_no, a.agen_no";
            }
            else {
                sql = @"
                select po_no 訂單編號, MAT_CLASS 物料分類,
                    (case when isback='Y' then '已回覆' when isback='N' then '未回覆' end) 回覆狀態, 
                    a.agen_no 廠商代碼, b.agen_namec 廠商名稱, b.agen_tel 廠商電話,
                    (select data_desc from PARAM_D where  DATA_NAME='PO_STATUS' and DATA_VALUE=a.po_status) as 狀態,
                    b.EMAIL 
                    from MM_PO_M a, PH_VENDER b
                    where 1=1 and a.agen_no=b.agen_no and TWN_DATE(po_time) = :PO_TIME and isback = :ISBACK
                      and MAT_CLASS=:MAT_CLASS
                    order by po_no, a.agen_no";
            }

            //已回覆=Y  其餘=N
            ISBACK = ISBACK == "已回覆" ? "Y" : "N";

            p.Add(":PO_TIME", PO_TIME);
            p.Add(":ISBACK", ISBACK);
            p.Add(":MAT_CLASS", MAT_CLASS);

            DataTable dt = new DataTable();

            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}