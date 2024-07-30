using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;
using WebApp.Models.F;

namespace WebApp.Repository.F
{
    public class FA0029_MODEL : JCLib.Mvc.BaseModel
    {
        public string APPDEPT { get; set; }
        public string APPDEPT_NAME { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string MMCODE { get; set; }
        public string SUM_APV_VOLUME { get; set; }
      

    }
    public class FA0029Repository : JCLib.Mvc.BaseRepository
    {
        public FA0029Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0029_MODEL> GetAll(string ym, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select appdept,(select inid_name from UR_INID where inid=c.appdept) as appdept_name,mmcode,(select mmname_c from MI_MAST where mmcode=c.mmcode) as mmname_c,(select mmname_e from MI_MAST where mmcode=c.mmcode) as mmname_e,sum(apv_volume) as sum_apv_volume
                       from (select a.appdept,b.mmcode,
                                    round((select m_voll*m_volw*m_volh*m_volc/m_swap*
                                                  (select apl_inqty from MI_WINVMON where data_ym=:YM and wh_no=a.appdept and mmcode=b.mmcode) 
                                                     from MI_MAST where mmcode=b.mmcode and m_swap is not null and m_swap<>0),4 ) as apv_volume
                       from ME_DOCM a,ME_DOCD b where a.docno=b.docno and substr(TWN_DATE(b.apvtime),1,5)=:YM
                       and (select mat_class from MI_MAST where mmcode=b.mmcode) in ('02','03','04','05','06') and (select m_storeid from MI_MAST where mmcode=b.mmcode)='1' )c
                       group by appdept,mmcode order by appdept,mmcode ";


            p.Add("YM", ym);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0029_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string YM)
        {
            var p = new DynamicParameters();

            var sql = @"select appdept as 成本碼,(select inid_name from UR_INID where inid=c.appdept) as 單位名稱,mmcode as 院內碼,(select mmname_c from MI_MAST where mmcode=c.mmcode) as 中文品名,(select mmname_e from MI_MAST where mmcode=c.mmcode) as 英文品名,sum(apv_volume) as 申領品項總材積
                       from (select a.appdept,b.mmcode,
                                    round((select m_voll*m_volw*m_volh*m_volc/m_swap*
                                                  (select apl_inqty from MI_WINVMON where data_ym=:YM and wh_no=a.appdept and mmcode=b.mmcode) 
                                                     from MI_MAST where mmcode=b.mmcode and m_swap is not null and m_swap<>0),4 ) as apv_volume
                       from ME_DOCM a,ME_DOCD b where a.docno=b.docno and substr(TWN_DATE(b.apvtime),1,5)=:YM
                       and (select mat_class from MI_MAST where mmcode=b.mmcode) in ('02','03','04','05','06') and (select m_storeid from MI_MAST where mmcode=b.mmcode)='1' )c
                       group by appdept,mmcode order by appdept,mmcode ";


            p.Add("YM", YM);
           

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<FA0029_MODEL> GetReport(string YM)
        {
            var p = new DynamicParameters();
            var sql = @"select appdept,(select inid_name from UR_INID where inid=c.appdept) as appdept_name,mmcode,(select mmname_c from MI_MAST where mmcode=c.mmcode) as mmname_c,(select mmname_e from MI_MAST where mmcode=c.mmcode) as mmname_e,sum(apv_volume) as sum_apv_volume
                       from (select a.appdept,b.mmcode,
                                    round((select m_voll*m_volw*m_volh*m_volc/m_swap*
                                                  (select apl_inqty from MI_WINVMON where data_ym=:YM and wh_no=a.appdept and mmcode=b.mmcode) 
                                                     from MI_MAST where mmcode=b.mmcode and m_swap is not null and m_swap<>0),4 ) as apv_volume
                       from ME_DOCM a,ME_DOCD b where a.docno=b.docno and substr(TWN_DATE(b.apvtime),1,5)=:YM
                       and (select mat_class from MI_MAST where mmcode=b.mmcode) in ('02','03','04','05','06') and (select m_storeid from MI_MAST where mmcode=b.mmcode)='1' )c
                       group by appdept,mmcode order by appdept,mmcode ";

            p.Add("YM", YM);

            return DBWork.Connection.Query<FA0029_MODEL>(sql, p, DBWork.Transaction);
        }
    }
}
