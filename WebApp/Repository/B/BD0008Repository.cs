using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;

namespace WebApp.Repository.B
{
    public class BD0008Repository : JCLib.Mvc.BaseRepository
    {
        public BD0008Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<PH_EQPD> GetAll(string recym, string wh_no, string contracno, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();
            #region 舊的
            //string sql = @"select :RECYM AS RecYM, a.wh_no, a.mmcode, a.m_purun, a.mmname_e, a.m_contprice, a.inv_qty  as INV_QTY, nvl(c.min_ordqty,1) UNIT_SWAP, a.BASE_UNIT,
            //                   CASE WHEN AdviseQty IS NULL THEN 0 ELSE CEIL(AdviseQty/nvl(c.min_ordqty,1)) * nvl(c.min_ordqty,1) END AS AdviseQty,
            //                   CASE WHEN AdviseQty*M_CONTPRICE IS NULL THEN 0 ELSE AdviseQty*M_CONTPRICE END AS AdviseMoney,
            //                   CASE WHEN EstimateQty IS NULL THEN CEIL(AdviseQty/nvl(c.min_ordqty,1)) * nvl(c.min_ordqty,1) ELSE CEIL(EstimateQty) END ESTQTY,
            //                   CASE WHEN EstimateQty IS NULL THEN AdviseQty*M_CONTPRICE ELSE EstimateQty*M_CONTPRICE END AMOUNT,  m_agenno, agen_namec,a.AGEN_NO||' '||a.AGEN_NAMEC AGEN_NO, contracno, nvl2(EstimateQty,1,0) as flag -- 1 存在 PH_EQPD
            //                FROM
            //                ( select WHINV.wh_no, WHINV.mmcode, MAST.m_purun, MAST.mmname_e, MAST.base_unit, MAST.m_contprice, MAST.m_agenno, VENDER.agen_namec, VENDER.agen_no, WHINV.inv_qty,
            //                nvl2((select contracno from PH_EQPD  where recym=:RECYM and wh_no=WHINV.wh_no and mmcode=WHINV.mmcode), (select contracno from PH_EQPD  where recym=:RECYM and wh_no=WHINV.wh_no and mmcode=WHINV.mmcode),MAST.contracno ) contracno ,   
            //                (select case when EQPD.estqty is null then 0 else EQPD.estqty end from PH_EQPD EQPD where EQPD.recym=:RECYM and EQPD.wh_no=WHINV.wh_no and EQPD.mmcode=WHINV.mmcode  ) EstimateQty,
            //                case when max(WINVMON.QTY)*1.5 is null then 0 else max(WINVMON.QTY)*1.5 end as AdviseQty  from MI_WHINV WHINV, MI_MAST MAST, PH_VENDER  VENDER, 
            //                 ( select case when apl_inqty is null then 0 else apl_inqty end as QTY,           wh_no, mmcode, data_ym from MI_WINVMON where data_ym >=:RECMINUS3M and data_ym <= :RECMINUS1M
            //                    ) WINVMON 
            //                  where  WHINV.wh_no = WINVMON.wh_no (+) and WHINV.mmcode = WINVMON.mmcode (+)
            //                and WHINV.wh_no = :WH_NO
            //                and MAST.e_orderdcflag ='N'
            //                and WHINV.mmcode = MAST.mmcode
            //                and MAST.m_agenno=VENDER.agen_no
            //                and substr(WHINV.mmcode,1,3) in ('005','006','007') 
            //                and MAST.m_agenno not in ('000','300','990','999')";

            //                  if (contracno == "0Y/0N") { sql += " AND MAST.CONTRACNO IN ('0N','0Y') "; }
            //                  else if (contracno == "0N") { sql += " AND MAST.CONTRACNO IN ('0N') "; }
            //                  else if (contracno == "0Y") { sql += "  AND MAST.CONTRACNO IN ('0Y') "; }


            //                             sql += @" GROUP BY  WHINV.WH_NO, WHINV.MMCODE, MAST.M_PURUN, MAST.MMNAME_E, MAST.BASE_UNIT, MAST.M_CONTPRICE, MAST.M_AGENNO, VENDER.AGEN_NAMEC, MAST.CONTRACNO, AGEN_NO, WHINV.INV_QTY
            //                               )a, mi_winvctl  C
            //      where a.wh_no=c.wh_no(+) and a.mmcode=c.mmcode(+) 
            //                                ORDER BY RECYM, WH_NO, MMCODE
            //                              ";
            #endregion

            string sql = @"    select eqpd.recym,
                                      eqpd.wh_no,
                                     eqpd.mmcode,
                                      eqpd.m_purun,
                                      eqpd.m_contprice,
                                        eqpd.min_ordqty,
                                        mast.mmname_e,
                                        eqpd.m_purun as base_unit, 
                                        inv_qty as INV_QTY,
                                        eqpd.min_ordqty as UNIT_SWAP,
                                        eqpd.adviseqty ,
                                        eqpd.estqty,
                                        eqpd.amount,
                                        eqpd.contracno,
                                       eqpd.ADVISEQTY *eqpd.M_CONTPRICE ADVISEMONEY,
                                        vender.AGEN_NO || '_' || vender.AGEN_NAMEC  AGEN_NO,
                                        '' as flag
                        from ph_eqpd eqpd, mi_whinv whinv, mi_mast mast, ph_vender vender
                          where eqpd.wh_no = whinv.wh_no(+) and eqpd.mmcode = whinv.mmcode(+)
                        and eqpd.mmcode = mast.mmcode
                        and mast.m_agenno = vender.agen_no
                        and mast.e_orderdcflag = 'N'
                        and eqpd.recym = :recym
                        and eqpd.wh_no = :wh_no
                                    and substr(eqpd.mmcode,1,3) in ('005', '006', '007')
                        and mast.m_agenno not in ('000', '300', '990', '999')";

                              if (contracno == "0Y/0N") { sql += " AND MAST.CONTRACNO IN ('0N','0Y') "; }
                              else if (contracno == "0N") { sql += " AND MAST.CONTRACNO IN ('0N') "; }
                              else if (contracno == "0Y") { sql += "  AND MAST.CONTRACNO IN ('0Y') "; }

            sql += " order by eqpd.recym, eqpd.wh_no, eqpd.mmcode";



            DateTime rec = DateTime.Parse(recym);
            string _recYM = (rec.Year - 1911).ToString() + rec.Month.ToString("00");

            p.Add(":recym", string.Format("{0}", _recYM));
            //DateTime recMinus3m=rec.AddMonths(-3);

            //string recMinus3m = (rec.AddMonths(-3).Year - 1911).ToString() + rec.AddMonths(-3).Month.ToString("00");
            //string recMinus1m = (rec.AddMonths(-1).Year - 1911).ToString() + rec.AddMonths(-1).Month.ToString("00");

            //p.Add(":recMinus3m", string.Format("{0}", recMinus3m));
            //p.Add(":recMinus1m", string.Format("{0}", recMinus1m));

            p.Add(":wh_no", string.Format("{0}", wh_no));


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_EQPD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_EQPD> GetSum(string recym, string wh_no, string contracno)
        {
            //           CASE WHEN AdviseQty*M_CONTPRICE IS NULL THEN 0 ELSE AdviseQty*M_CONTPRICE END AS AdviseMoney ,
           // CASE WHEN EstimateQty IS NULL THEN AdviseQty* M_CONTPRICE ELSE EstimateQty*M_CONTPRICE END AMOUNT

            var p = new DynamicParameters();

            string sql = @"    select 
                            TO_CHAR(round(SUM(eqpd.adviseqty*eqpd.m_contprice))) SUM1,
                            TO_CHAR(round(SUM(eqpd.estqty*eqpd.m_contprice))) SUM2
                        from ph_eqpd eqpd, mi_whinv whinv, mi_mast mast, ph_vender vender
                          where eqpd.wh_no = whinv.wh_no(+) and eqpd.mmcode = whinv.mmcode(+)
                        and eqpd.mmcode = mast.mmcode
                        and mast.m_agenno = vender.agen_no
                        and mast.e_orderdcflag = 'N'
                        and eqpd.recym = :recym
                        and eqpd.wh_no = :wh_no
                                    and substr(eqpd.mmcode,1,3) in ('005', '006', '007')
                        and mast.m_agenno not in ('000', '300', '990', '999')";

            if (contracno == "0Y/0N") { sql += " AND MAST.CONTRACNO IN ('0N','0Y') "; }
            else if (contracno == "0N") { sql += " AND MAST.CONTRACNO IN ('0N') "; }
            else if (contracno == "0Y") { sql += "  AND MAST.CONTRACNO IN ('0Y') "; }



            DateTime rec = DateTime.Parse(recym);
            //DateTime recMinus3m=rec.AddMonths(-3);
            string _recYM = (rec.Year - 1911).ToString() + rec.Month.ToString("00");
            //string recMinus3m = (rec.AddMonths(-3).Year - 1911).ToString() + rec.AddMonths(-3).Month.ToString("00");
            //string recMinus1m = (rec.AddMonths(-1).Year - 1911).ToString() + rec.AddMonths(-1).Month.ToString("00");

            p.Add(":recym", string.Format("{0}", _recYM));
            //p.Add(":recMinus3m", string.Format("{0}", recMinus3m));
            //p.Add(":recMinus1m", string.Format("{0}", recMinus1m));

            p.Add(":wh_no", string.Format("{0}", wh_no));


            return DBWork.Connection.Query<PH_EQPD>(sql, p, DBWork.Transaction);
        }

        public DataTable GetReportSum(string recym, string wh_no, string contracno)
        {
            //           CASE WHEN AdviseQty*M_CONTPRICE IS NULL THEN 0 ELSE AdviseQty*M_CONTPRICE END AS AdviseMoney ,
            // CASE WHEN EstimateQty IS NULL THEN AdviseQty* M_CONTPRICE ELSE EstimateQty*M_CONTPRICE END AMOUNT

            var p = new DynamicParameters();

            string sql = @"    select 
                            round(SUM(eqpd.adviseqty*eqpd.m_contprice)) ADVISEMONEY,
                            round(SUM(eqpd.estqty*eqpd.m_contprice)) AMOUNT
                        from ph_eqpd eqpd, mi_whinv whinv, mi_mast mast, ph_vender vender
                          where eqpd.wh_no = whinv.wh_no(+) and eqpd.mmcode = whinv.mmcode(+)
                        and eqpd.mmcode = mast.mmcode
                        and mast.m_agenno = vender.agen_no
                        and mast.e_orderdcflag = 'N'
                        and eqpd.recym = :recym
                        and eqpd.wh_no = :wh_no
                                    and substr(eqpd.mmcode,1,3) in ('005', '006', '007')
                        and mast.m_agenno not in ('000', '300', '990', '999')";

            if (contracno == "0Y/0N") { sql += " AND MAST.CONTRACNO IN ('0N','0Y') "; }
            else if (contracno == "0N") { sql += " AND MAST.CONTRACNO IN ('0N') "; }
            else if (contracno == "0Y") { sql += "  AND MAST.CONTRACNO IN ('0Y') "; }



            DateTime rec = DateTime.Parse(recym);
            //DateTime recMinus3m=rec.AddMonths(-3);
            string _recYM = (rec.Year - 1911).ToString() + rec.Month.ToString("00");
            //string recMinus3m = (rec.AddMonths(-3).Year - 1911).ToString() + rec.AddMonths(-3).Month.ToString("00");
            //string recMinus1m = (rec.AddMonths(-1).Year - 1911).ToString() + rec.AddMonths(-1).Month.ToString("00");

            p.Add(":recym", string.Format("{0}", _recYM));
            //p.Add(":recMinus3m", string.Format("{0}", recMinus3m));
            //p.Add(":recMinus1m", string.Format("{0}", recMinus1m));

            p.Add(":wh_no", string.Format("{0}", wh_no));


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<PH_EQPD> Report(string recym, string wh_no, string contracno ,string orderby)
        {

            var p = new DynamicParameters();

            string sql = @"    select eqpd.recym,
                                      eqpd.wh_no,
                                     eqpd.mmcode,
                                      eqpd.m_purun,
                                      eqpd.m_contprice,
                                        eqpd.min_ordqty,
                                        mast.mmname_e,
                                        eqpd.m_purun as base_unit, 
                                        inv_qty as INV_QTY,
                                        eqpd.min_ordqty as UNIT_SWAP,
                                        eqpd.adviseqty ,
                                        eqpd.estqty,
                                        eqpd.amount,
                                        eqpd.contracno,
                                       eqpd.ADVISEQTY *eqpd.M_CONTPRICE ADVISEMONEY,
                                        vender.AGEN_NO || '_' || vender.EASYNAME  AGEN_NO,
                                        '' as flag
                        from ph_eqpd eqpd, mi_whinv whinv, mi_mast mast, ph_vender vender
                          where eqpd.wh_no = whinv.wh_no(+) and eqpd.mmcode = whinv.mmcode(+)
                        and eqpd.mmcode = mast.mmcode
                        and mast.m_agenno = vender.agen_no
                        and mast.e_orderdcflag = 'N'
                        and eqpd.recym = :recym
                        and eqpd.wh_no = :wh_no
                                    and substr(eqpd.mmcode,1,3) in ('005', '006', '007')
                        and mast.m_agenno not in ('000', '300', '990', '999')
                        and eqpd.estqty > 0 ";

            if (contracno == "0Y/0N") { sql += " AND MAST.CONTRACNO IN ('0N','0Y') "; }
            else if (contracno == "0N") { sql += " AND MAST.CONTRACNO IN ('0N') "; }
            else if (contracno == "0Y") { sql += "  AND MAST.CONTRACNO IN ('0Y') "; }

            sql += " order by  ";



            DateTime rec = DateTime.Parse(recym);
            //DateTime recMinus3m=rec.AddMonths(-3);
            string _recYM = (rec.Year - 1911).ToString() + rec.Month.ToString("00");
            //string recMinus3m = (rec.AddMonths(-3).Year - 1911).ToString() + rec.AddMonths(-3).Month.ToString("00");
            //string recMinus1m = (rec.AddMonths(-1).Year - 1911).ToString() + rec.AddMonths(-1).Month.ToString("00");

            p.Add(":recym", string.Format("{0}", _recYM));
            //p.Add(":recMinus3m", string.Format("{0}", recMinus3m));
            //p.Add(":recMinus1m", string.Format("{0}", recMinus1m));

            p.Add(":wh_no", string.Format("{0}", wh_no));

            switch (orderby)
            {
                case "1":
                    orderby = "agen_no";
                    break;
                case "2":
                    orderby = "mmcode";
                    break;
                case "3":
                    orderby = "amount desc";
                    break;
            }
            sql += orderby;


            return DBWork.Connection.Query<PH_EQPD>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BD0008> Report2(string parWH_NO)
        {

            var p = new DynamicParameters();

            string sql = @"select x.mmcode, m.mmname_e, m.contracno, sum(x.qty) QTY, sum(y.uprice*x.qty) tot from 
                           (select TWN_YYYMM(sysdate) data_ym, a.mmcode, sum(APL_INQTY) qty
                             from mi_mast a, mi_whinv b
                             where b.wh_no=:wh_no and a.mmcode=b.mmcode and a.contracno in ('0N', '0Y')  
                             and substr(a.mmcode,1,3) in ('005', '006', '007')
                             and a.m_agenno not in ('000', '300', '990', '999')  
                             and a.e_orderdcflag = 'N'
                             group by  TWN_YYYMM(sysdate), a.mmcode
                           union 
                           select b.data_ym, a.mmcode, sum(APL_INQTY) qty
                             from mi_mast a, mi_winvmon b
                             where b.wh_no=:wh_no and a.mmcode=b.mmcode and a.contracno in ('0N', '0Y')   
                             and  b.data_ym >=TWN_YYY01(sysdate)   and  b.data_ym <=TWN_YYYMM(ADD_MONTHS(sysdate,-1))    
                             and substr(a.mmcode,1,3) in ('005', '006', '007')
                             and a.m_agenno not in ('000', '300', '990', '999') 
                             and a.e_orderdcflag = 'N'
                             group by  b.data_ym,a.mmcode  
                           ) x, mi_whcost y, mi_mast m
                          where x.data_ym=y.data_ym and x.mmcode=y.mmcode and x.mmcode=m.mmcode
                          and x.qty >0
                          group by x.mmcode, m.mmname_e, m.contracno
                          order by tot desc";

            p.Add(":wh_no", string.Format("{0}", parWH_NO));
            return DBWork.Connection.Query<BD0008>(sql, p, DBWork.Transaction);
        }
        public int Update(PH_EQPD ph_eqpd)
        {
            var sql = "";

            if (!CheckDetailMmcodedExists(ph_eqpd))
            {
                sql = @"Update  ph_eqpd set ESTQTY = :ESTQTY ,STKQTY=:INV_QTY,AMOUNT=:AMOUNT, UPDATE_USER =:UPDATE_USER , UPDATE_TIME = sysdate, UPDATE_IP =:UPDATE_IP
                           where recym = :recym and wh_no = :wh_no and mmcode = :mmcode ";
            }
            else
            {
                sql = @"INSERT INTO ph_eqpd (
                            recym, wh_no,  mmcode, m_purun,  m_contprice, adviseqty, stkqty,
                            estqty, contracno, create_time, create_user, update_ip      
                            ,amount
                        ) VALUES (
                            :recym,:wh_no,  :mmcode, :m_purun,  :m_contprice, :adviseqty, :INV_QTY,
                            :estqty, :contracno,sysdate ,: create_user,: update_ip
                            ,:amount
                        )";
            }

            //if (ph_eqpd.FLAG == 1)
            //{
            //    sql = @"Update  ph_eqpd set ESTQTY = :ESTQTY , UPDATE_USER =:UPDATE_USER , UPDATE_TIME = sysdate, UPDATE_IP =:UPDATE_IP
            //               where recym = :recym and wh_no = :wh_no and mmcode = :mmcode ";
            //}
            //else
            //{
            //    sql = @"INSERT INTO ph_eqpd (
            //                recym, wh_no,  mmcode, m_purun,  m_contprice, adviseqty, stkqty,
            //                estqty, contracno, create_time, create_user, update_ip      
            //                ,amount
            //            ) VALUES (
            //                :recym,:wh_no,  :mmcode, :m_purun,  :m_contprice, :adviseqty, :stkqty,
            //                :estqty, :contracno,sysdate ,: create_user,: update_ip
            //                ,:amount
            //            )";
            //}


                return DBWork.Connection.Execute(sql, ph_eqpd, DBWork.Transaction);
        }



        public bool CheckDetailMmcodedExists(PH_EQPD ph_eqpd)
        {
            string sql = @"SELECT 1 FROM ph_eqpd WHERE recym = :recym and wh_no = :wh_no and mmcode = :mmcode";
            return DBWork.Connection.ExecuteScalar(sql, ph_eqpd, DBWork.Transaction) == null;
        }

        public IEnumerable<ComboItemModel> GetWhnoCombo()
        {
            var p = new DynamicParameters();


            string sql = @"select B.WH_NO ||' '||B.WH_NAME TEXT, B.WH_NO VALUE  from  MI_WHMAST B
                            where B.WH_GRADE in ('1','5') and B.WH_KIND='0' ";
            sql += " order by Value ";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
        }

        public IEnumerable<string> GetCount(string wh_no, string RECYM,string CONTRACNO)
        {
            var p = new DynamicParameters();

            string sql = "";

            if (CONTRACNO == "0Y/0N")
            {
                sql = @" select count(*) from  PH_EQPD where wh_no=:wh_no and  RECYM=:RECYM";

            }
            else
            {
                sql = @" select count(*) from  PH_EQPD where wh_no=:wh_no and  RECYM=:RECYM and CONTRACNO=:CONTRACNO";
            }


            return DBWork.Connection.Query<string>(sql, new { wh_no = wh_no, RECYM= RECYM, CONTRACNO= CONTRACNO }, DBWork.Transaction);
        }

        public bool CheckMasterdExists(string RecYM)
        {
            DateTime rec = DateTime.Parse(RecYM);
            string _recYM = (rec.Year - 1911).ToString() + rec.Month.ToString("00");
            string sql = @"select 1 from PH_EQPD where RecYM=:RecYM ";
            return DBWork.Connection.ExecuteScalar(sql, new { RecYM= _recYM }, DBWork.Transaction) != null;
        }


        public int InsertRecYM(string recym,string wh_no)
        {
            var p = new DynamicParameters();


            var sql = @"insert into PH_EQPD (recym, wh_no, mmcode, m_purun, m_contprice, min_ordqty, adviseqty, estqty, amount, contracno, stkqty, create_time, create_user, update_ip)
                        select  :recym as recym, a.wh_no, a.mmcode, a.base_unit as m_purun, a.m_contprice, c. min_ordqty,
                           case when adviseqty is null then 0 else ceil(adviseqty/nvl(c.min_ordqty,1)) * nvl(c.min_ordqty,1) end as adviseqty,
                           case when adviseqty is null then 0 else ceil(adviseqty/nvl(c.min_ordqty,1)) * nvl(c.min_ordqty,1) end as adviseqty,   
                           case when adviseqty is null then 0 else ceil(adviseqty/nvl(c.min_ordqty,1)) * nvl(c.min_ordqty,1) *m_contprice end as amount,      
                           contracno, inv_qty, sysdate, :userId, :userIp
                        from
                        ( select whinv.wh_no, whinv.mmcode, mast.base_unit, mast.m_contprice, mast.m_agenno, whinv.inv_qty,
                        nvl2((select contracno from ph_eqpd  where recym= :recym and wh_no=whinv.wh_no and mmcode=whinv.mmcode), (select contracno from ph_eqpd  where recym= :recym and wh_no=whinv.wh_no and mmcode=whinv.mmcode),mast.contracno ) contracno ,   
                        (select case when eqpd.estqty is null then 0 else eqpd.estqty end from ph_eqpd eqpd where eqpd.recym= :recym and eqpd.wh_no=whinv.wh_no and eqpd.mmcode=whinv.mmcode  ) estimateqty,
                        case when max(winvmon.qty)*1.5 is null then 0 else max(winvmon.qty)*1.5 end as adviseqty  from mi_whinv whinv, mi_mast mast, ph_vender  vender, 
                         ( select case when apl_inqty is null then 0 else apl_inqty end as qty,           wh_no, mmcode, data_ym from mi_winvmon where data_ym >= :recMinus3m and data_ym <= :recMinus1m
                            ) winvmon 
                          where  whinv.wh_no = winvmon.wh_no (+) and whinv.mmcode = winvmon.mmcode (+)
                        and whinv.wh_no =:wh_no
                        and mast.e_orderdcflag ='N'
                        and whinv.mmcode = mast.mmcode
                        and mast.m_agenno=vender.agen_no
                        and substr(whinv.mmcode,1,3) in ('005','006','007') 
                        and mast.m_agenno not in ('000','300','990','999')
                        and mast.contracno in ('0N','0Y')
                        group by  whinv.wh_no, whinv.mmcode, mast.base_unit, mast.m_contprice, mast.m_agenno, contracno, whinv.inv_qty
                                                 )a, mi_winvctl  c
                        where a.wh_no=c.wh_no(+) and a.mmcode=c.mmcode(+) 
                                                  order by recym, wh_no, mmcode
                        ";

            DateTime rec = DateTime.Parse(recym);

            string _recYM = (rec.Year - 1911).ToString() + rec.Month.ToString("00");
            string recMinus3m = (rec.AddMonths(-3).Year - 1911).ToString() + rec.AddMonths(-3).Month.ToString("00");
            string recMinus1m = (rec.AddMonths(-1).Year - 1911).ToString() + rec.AddMonths(-1).Month.ToString("00");

            p.Add(":recym", string.Format("{0}", _recYM));
            p.Add(":recMinus3m", string.Format("{0}", recMinus3m));
            p.Add(":recMinus1m", string.Format("{0}", recMinus1m));

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":userId", string.Format("{0}", DBWork.ProcUser));
            p.Add(":userIp", string.Format("{0}", DBWork.ProcIP));


            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
    }
}


