using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.C
{
    public class CC0002Repository : JCLib.Mvc.BaseRepository
    {
        public CC0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_PO_M> GetAll(string agen_no, string createtime, string barcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select PO_NO,AGEN_NO, M_CONTID, PO_STATUS 
                            from MM_PO_M
                            where PO_STATUS in ('82','85') and MAT_CLASS <> '01' and substr(po_no,1,3) not in ('TXT')
                              and create_user <> '緊急醫療出貨'";

            if (barcode == "" || barcode == null)
            {
                sql += " AND AGEN_NO=:p0 ";
                p.Add(":p0", agen_no);
            }


            if (createtime != "" && createtime != null && (barcode == "" || barcode == null))
            {
                sql += " AND to_char(CREATE_TIME,'yyyy-mm-dd') >= Substr(:p1, 1, 10) ";
                p.Add(":p1", createtime);
            }

            if (barcode != "" && barcode != null)
            {
                sql += " AND PO_NO = :p2 ";
                p.Add(":p2", barcode);
            }
            sql += " order by PO_NO desc";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PO_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CC0002D> GetDetailAll(string po_no, string mmcode, string agen_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select a.PO_NO, a.MMCODE, b.MMNAME_C, b.MMNAME_E, b.WEXP_ID, a.STOREID as M_STOREID, b.MAT_CLASS, a.PO_QTY, a.M_PURUN, a.UNIT_SWAP, b.BASE_UNIT, 
                            a.PO_QTY*a.UNIT_SWAP as SWAP_PO_QTY, 
                            (select nvl(sum(ACC_QTY),'0')/a.UNIT_SWAP from BC_CS_ACC_LOG where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE) as PO_ACC_QTY,
                            (select nvl(sum(INQTY),'0') from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B') as REPLY_QTY,
                            (select nvl(LOT_NO,'') from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B' and rownum=1) as LOT_NO,
                            (select nvl(EXP_DATE,'') from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B' and rownum=1) as EXP_DATE,
                            (select nvl(INVOICE,'') from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B' and rownum=1) as INVOICE,
                            (select nvl(sum(INQTY),'0') from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B') as INQTY,
                            0 as BW_SQTY
                            from MM_PO_D a, MI_MAST b
                            where a.PO_NO=:p0 and a.MMCODE=b.MMCODE and (b.WEXP_ID is null or b.WEXP_ID<>'Y') ";

            p.Add(":p0", po_no);
            p.Add(":p2", agen_no);

            if (mmcode != "" && mmcode != null)
            {
                sql += " AND A.MMCODE = :p1 ";
                p.Add(":p1", mmcode);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CC0002D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CC0002D> GetDetailAllExp(string po_no, string mmcode, string agen_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select a.PO_NO, a.MMCODE, b.MMNAME_C, b.MMNAME_E, b.WEXP_ID, a.STOREID as M_STOREID, b.MAT_CLASS, a.PO_QTY, a.M_PURUN, a.UNIT_SWAP, b.BASE_UNIT,  
                            a.PO_QTY*a.UNIT_SWAP as SWAP_PO_QTY, 
                            (select nvl(sum(ACC_QTY),'0')/a.UNIT_SWAP from BC_CS_ACC_LOG where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE) as PO_ACC_QTY,
                            (select INQTY from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B' and rownum=1) as REPLY_QTY,
                            (select LOT_NO from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B' and rownum=1) as LOT_NO,
                            (select EXP_DATE from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B' and rownum=1) as EXP_DATE,
                            (select INQTY from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B' and rownum=1) as INQTY,
                            (select INVOICE from PH_REPLY where PO_NO=a.PO_NO and AGEN_NO=:p2 and MMCODE=a.MMCODE and STATUS='B' and rownum=1) as INVOICE,
                            0 as BW_SQTY
                            from MM_PO_D a, MI_MAST b
                            where a.PO_NO=:p0 and a.MMCODE=b.MMCODE and b.WEXP_ID='Y' ";
            p.Add(":p0", po_no);
            p.Add(":p2", agen_no);

            if (mmcode != "" && mmcode != null)
            {
                sql += " AND A.MMCODE = :p1 ";
                p.Add(":p1", mmcode);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CC0002D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CC0002D> GetDistAll(string po_no, string mmcode, string seq, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            //var sql = @" select a.PO_NO, a.MMCODE, a.SEQ, a.PR_DEPT, b.INID_NAME, a.PR_QTY, a.LOT_NO, a.EXP_DATE,
            //                (select MMNAME_C from MI_MAST where MMCODE = a.MMCODE) as MMNAME_C,
            //                a.BW_SQTY, a.DIST_QTY
            //                from BC_CS_DIST_LOG a, UR_INID b
            //                where a.PO_NO=:p0 and a.MMCODE=:p1 and a.SEQ=:p2 and a.PR_DEPT=b.INID ";
            var sql = @" SELECT d.seq, b.INID_NAME, a.PO_QTY as PR_QTY, nvl(c.qty/c.unit_swap,0) as DSUM_QTY,  
                                a.PO_QTY-nvl(c.qty/c.unit_swap,0) as DIST_QTY, b.INID 
                           FROM BC_CS_DIST_LOG d, PH_PO_N a, UR_INID b,
                                (select distinct f.unit_swap, e.*
                                   from BC_CS_ACC_LOG f,
                                        (select pr_dept, mmcode, po_no, nvl(sum(DIST_QTY),0) qty 
                                           from BC_CS_DIST_LOG  
                                          where po_no=:p0 and mmcode =:p1 and dist_status='T'
                                          group by  pr_dept, mmcode, po_no ) e
                                  where f.po_no = e.po_no(+)
                                    and f.mmcode = e.mmcode(+) 
                                ) c                   
                          WHERE d.po_no=a.po_no 
                            and d.mmcode=a.mmcode 
                            and d.pr_dept=a.inid 
                            and d.po_no =:p0 
                            and d.mmcode =:p1 
                            and d.dist_status='L'
                            and a.inid=b.INID 
                            and a.inid=c.pr_dept(+) 
                            and a.mmcode=c.mmcode(+) ";
            p.Add(":p0", po_no);
            p.Add(":p1", mmcode);
            //p.Add(":p2", seq);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CC0002D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<CC0002D> GetDistAll_SCAN(string po_no, string mmcode, string seq, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @" SELECT :seq AS SEQ,
                                b.INID_NAME,
                                a.PO_QTY AS PR_QTY,
                                nvl(c.qty/c.unit_swap,0) as DSUM_QTY,  
                                                         a.PO_QTY-nvl(c.qty/c.unit_swap,0) as DIST_QTY,
                                b.INID
                           FROM PH_PO_N a,
                                UR_INID b,
                                (select distinct f.unit_swap, e.*
                                                            from BC_CS_ACC_LOG f,
                                                                 (select pr_dept, mmcode, po_no, nvl(sum(DIST_QTY),0) qty 
                                                                    from BC_CS_DIST_LOG  
                                                                   where po_no=:p0 and mmcode =:p1 and dist_status='T'
                                                                   group by  pr_dept, mmcode, po_no ) e
                                                           where f.po_no = e.po_no(+)
                                                             and f.mmcode = e.mmcode(+) 
                                                         ) c
                          WHERE     a.po_no = :p0
                                AND a.mmcode = :p1
                                AND a.inid = b.INID
                                AND a.inid = c.pr_dept(+)
                                AND a.mmcode = c.mmcode(+)";
            p.Add(":p0", po_no);
            p.Add(":p1", mmcode);
            p.Add(":seq", seq);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CC0002D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<CC0002D> GetDetailInid(string po_no, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT  b.INID_NAME, a.PO_QTY FROM PH_PO_N a, UR_INID b
                            WHERE a.inid=b.INID
                            AND PO_NO = :p0  AND MMCODE = :p1 ";

            p.Add(":p0", po_no);
            p.Add(":p1", mmcode);

            return DBWork.PagingQuery<CC0002D>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CC0002D> GetBarCodeDetailP1(string mmcode, string start_dt, string end_dt)
        {
            var p = new DynamicParameters();

            var sql = @" select a.PO_NO , a.AGEN_NO, a.MMCODE , a.SEQ , a.lot_no, A.EXP_DATE, b.M_PURUN, b.UNIT_SWAP,   b.PO_QTY, a.INQTY,   c.mmname_c,  c.mmname_e, 
                            c.BASE_UNIT, c.wexp_id ,  b.STOREID, c.MAT_CLASS, a.SEQ as PH_REPLY_SEQ, a.INVOICE,
                            nvl((select (case when SRC_PR_QTY=0 then PR_QTY else PR_QTY-SRC_PR_QTY end) 
                                   from MM_PR_D where PR_NO=b.PR_NO and MMCODE=b.MMCODE)/b.UNIT_SWAP,0) as retain_qty,
                            nvl((select sum(DIST_QTY) from BC_CS_DIST_LOG 
                                  where PO_NO=b.PO_NO and MMCODE=b.MMCODE and PR_DEPT=whno_mm1 group by PO_NO,MMCODE)/b.UNIT_SWAP,0) as retain_distqty
                            from PH_REPLY a , mm_po_d b, mi_mast c
                            where a.po_no=b.po_no and a.mmcode=b.mmcode and a.mmcode= c.mmcode 
                            and (a.MMCODE= trim(:p0) or 
                              a.MMCODE= (select MMCODE from BC_BARCODE where BARCODE=trim(:p0) and status='Y' and rownum=1  ))
                            and a.STATUS='B' 
                            and a.PO_NO =(select PO_NO from MM_PO_M 
                                           where substr(po_no,1,3) not in ('TXT') 
                                             and po_no=a.PO_NO and mat_class<>'01' 
                                             and sysdate-po_time < 45
                                             and CREATE_USER<>'緊急醫療出貨')
                            and a.INQTY > 0     
                            and (b.PO_QTY > b.DELI_QTY or b.DELI_STATUS <>'Y') 
                          ";
            if (start_dt != null || start_dt != "")
            {
                sql += " and substr(a.po_no,5,7) >= :start_dt ";
                p.Add(":start_dt", start_dt);
            }
            if (end_dt != null || end_dt != "")
            {
                sql += " and substr(a.po_no,5,7) <= :end_dt ";
                p.Add(":end_dt", end_dt);
            }
            sql += @" order by substr(a.po_no,5,7) desc ";
            p.Add(":p0", mmcode);

            sql = " select * from ( " + sql + " ) where rownum = 1 ";

            return DBWork.Connection.Query<CC0002D>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CC0002D> GetBarCodeDetailP3(string mmcode, string start_dt, string end_dt)
        {
            var p = new DynamicParameters();

            var sql = @" select a.PO_NO ,  a.AGEN_NO , b.MMCODE ,  b.PO_QTY, '' as lot_no, '' as EXP_DATE, b.M_PURUN, b.UNIT_SWAP,b.PO_QTY-(select nvl(sum(ACC_QTY/UNIT_SWAP),'0') from BC_CS_ACC_LOG where PO_NO=a.PO_NO and AGEN_NO=a.agen_no and MMCODE=b.MMCODE and status='P') as INQTY, c.mmname_c, c.mmname_e,  
                            c.BASE_UNIT , c.wexp_id , b.STOREID, c.MAT_CLASS, 0 as PH_REPLY_SEQ, '' INVOICE,
                            nvl((select (case when SRC_PR_QTY=0 then PR_QTY else PR_QTY-SRC_PR_QTY end) 
                                   from MM_PR_D where PR_NO=b.PR_NO and MMCODE=b.MMCODE)/b.UNIT_SWAP,0) as retain_qty,
                            nvl((select sum(DIST_QTY) from BC_CS_DIST_LOG 
                                  where PO_NO=b.PO_NO and MMCODE=b.MMCODE and PR_DEPT=whno_mm1 group by PO_NO,MMCODE)/b.UNIT_SWAP,0) as retain_distqty
                            from MM_PO_M a, MM_PO_D b, mi_mast c
                            where a.po_no=b.po_no and b.mmcode=c.mmcode and a.mat_class<>'01'
                            and a.PO_STATUS in ('82', '83', '85', '88')
                            and substr(a.po_no,1,3) not in ('TXT') 
                            and b.MMCODE= trim(:p0) 
                            and  (PO_QTY > DELI_QTY or DELI_STATUS <>'Y') 
                            and sysdate-po_time < 45 
                            and a.CREATE_USER<>'緊急醫療出貨'";
            if (start_dt != null || start_dt != "")
            {
                sql += " and substr(a.po_no,5,7) >= :start_dt ";
                p.Add(":start_dt", start_dt);
            }
            if (end_dt != null || end_dt != "")
            {
                sql += " and substr(a.po_no,5,7) <= :end_dt ";
                p.Add(":end_dt", end_dt);
            }
            sql += @" order by substr(a.po_no,5,7) desc ";
            p.Add(":p0", mmcode);

            sql = "select * from ( " + sql + " ) where rownum = 1 ";

            return DBWork.Connection.Query<CC0002D>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<CC0002D> GetTheLastPO(string mmcode, string start_dt, string end_dt)
        {   //參考GetBarCodeDetailP3 找出該院內碼最後訂單
            //SQL 刪除    
            //and  (PO_QTY > DELI_QTY or DELI_STATUS <>'Y') 
            //and sysdate-po_time < 45
            var p = new DynamicParameters();

            var sql = @" select a.PO_NO ,  a.AGEN_NO , b.MMCODE ,  b.PO_QTY, '' as lot_no, '' as EXP_DATE, b.M_PURUN, b.UNIT_SWAP,
                            b.PO_QTY-(select nvl(sum(ACC_QTY/UNIT_SWAP),'0') from BC_CS_ACC_LOG where PO_NO=a.PO_NO and AGEN_NO=a.agen_no and MMCODE=b.MMCODE and status='P') as INQTY,  
                            c.mmname_c, c.mmname_e, c.BASE_UNIT , c.wexp_id , b.STOREID, c.MAT_CLASS, 0 as PH_REPLY_SEQ, '' INVOICE,
                            nvl((select (case when SRC_PR_QTY=0 then PR_QTY else PR_QTY-SRC_PR_QTY end) 
                                   from MM_PR_D where PR_NO=b.PR_NO and MMCODE=b.MMCODE)/b.UNIT_SWAP,0) as retain_qty,
                            nvl((select sum(DIST_QTY) from BC_CS_DIST_LOG 
                                  where PO_NO=b.PO_NO and MMCODE=b.MMCODE and PR_DEPT=whno_mm1 group by PO_NO,MMCODE)/b.UNIT_SWAP,0) as retain_distqty
                            from MM_PO_M a, MM_PO_D b, mi_mast c
                            where a.po_no=b.po_no and b.mmcode=c.mmcode and a.mat_class<>'01'
                            and substr(a.po_no,1,3) not in ('TXT') 
                            and b.MMCODE= trim(:p0) 
                            and a.CREATE_USER<>'緊急醫療出貨'
                         ";
            if (start_dt != null || start_dt != "")
            {
                sql += " and substr(a.po_no,5,7) >= :start_dt ";
                p.Add(":start_dt", start_dt);
            }
            if (end_dt != null || end_dt != "")
            {
                sql += " and substr(a.po_no,5,7) <= :end_dt ";
                p.Add(":end_dt", end_dt);
            }
            sql += @" order by substr(a.po_no,5,7) desc ";
            p.Add(":p0", mmcode);

            sql = "select * from ( " + sql + " ) where rownum = 1 ";

            return DBWork.Connection.Query<CC0002D>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<DecodeResponse> GetUdiLog(string wmmid, string wmlot)
        {
            var p = new DynamicParameters();

            var sql = @" select WMMID as WmMid, WMCMPY as WmCmpy, WMWHS as WmWhs, WMORG as WmOrg, CRVMPY as CrVmpy, CRITM as CrItm, 
                            WMREFCODE as WmRefCode, WMBOX as WmBox, WMLOC as WmLoc, WMSRV as WmSrv, WMSKU as WmSku, WMMIDNAME as WmMidName, WMMIDNAMEH as WmMidNameH, 
                            WMSKUSPEC as WmSkuSpec, WMBRAND as WmBrand, WMMDL as WmMdl, WMMIDCTG as WmMidCtg, WMEFFCDATE as WmEffcDate, WMLOT as WmLot, WMSENO as WmSeno, 
                            WMPAK as WmPak, WMQY as WmQy, THISBARCODE as ThisBarcode, UDIBARCODES as UdiBarcodes, GTINSTRING as GtinString, NHIBARCODE as NhiBarcode, 
                            NHIBARCODES as NhiBarcodes, BARCODETYPE as BarcodeType, GTININSTRING as GtinInString, RESULT as Result, ERRMSG as ErrMsg 
                            from BC_UDI_LOG
                            where WMMID=:p0 ";

            p.Add(":p0", wmmid);

            if (wmlot == "" || wmlot == null)
                sql += " and (WMLOT = '' or WMLOT is null) ";
            else
                sql += " and WMLOT=:p1 ";
            p.Add(":p1", wmlot);

            sql += " order by LOG_TIME desc ";

            sql = "select * from ( " + sql + " ) where rownum = 1 ";

            return DBWork.Connection.Query<DecodeResponse>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CC0002D> GetLotNoData(string po_no, string mmcode, string agen_no)
        {
            var p = new DynamicParameters();

            var sql = @" select A.PO_NO, A.MMCODE, A.LOT_NO, A.EXP_DATE, A.BW_SQTY, A.SEQ as PH_REPLY_SEQ, A.INVOICE
                            (select sum(INQTY) from PH_REPLY
                            where PO_NO=A.PO_NO and AGEN_NO=A.AGEN_NO and MMCODE=A.MMCODE 
                            and (LOT_NO = A.LOT_NO or LOT_NO is null) and (EXP_DATE = A.EXP_DATE or EXP_DATE is null)) as INQTY
                            from PH_REPLY A
                            where PO_NO=:p0
                            and MMCODE=:p1 
                            and AGEN_NO=:p2 ";
            p.Add(":p0", po_no);
            p.Add(":p1", mmcode);
            p.Add(":p2", agen_no);

            return DBWork.Connection.Query<CC0002D>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CC0002D> GetLotNoDataForm(string po_no, string mmcode, string agen_no)
        {
            var p = new DynamicParameters();

            var sql = @" select PO_NO, MMCODE, LOT_NO, EXP_DATE, BW_SQTY, INQTY, SEQ as PH_REPLY_SEQ, INVOICE
                            from PH_REPLY
                            where PO_NO=:p0
                            and MMCODE=:p1 
                            and AGEN_NO=:p2 
                            and STATUS='B'
                            order by DNO ";
            p.Add(":p0", po_no);
            p.Add(":p1", mmcode);
            p.Add(":p2", agen_no);

            return DBWork.Connection.Query<CC0002D>(sql, p, DBWork.Transaction);
        }

        public int Create(CC0002D cc0002d)
        {
            var sql = @"insert into BC_CS_ACC_LOG 
            (PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, EXP_DATE, BW_SQTY, INQTY, ACC_QTY, CFM_QTY, ACC_BASEUNIT, STATUS, MEMO, ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, UNIT_SWAP, WEXP_ID, TX_QTY_T, INVOICE)  
                                values (:PO_NO, :MMCODE, :SEQ, :AGEN_NO, :LOT_NO, 
                                case when to_char(:EXP_DATE, 'yyyy/mm/dd') = '0001/01/01' then null else to_date(to_char(:EXP_DATE, 'yyyy/mm/dd'), 'yyyy/mm/dd') end, 
                                0, :INQTY, :ACC_QTY*:UNIT_SWAP, :ACC_QTY*:UNIT_SWAP, :BASE_UNIT, 'C', trim(:MEMO), sysdate, :ACC_USER, :M_STOREID, :MAT_CLASS, :PO_QTY, :M_PURUN, :UNIT_SWAP, :WEXP_ID, :ACC_QTY, :INVOICE)";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        public int ScanCreate(CC0002D cc0002d)
        {
            var sql = @"insert into BC_CS_ACC_LOG 
            (PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, EXP_DATE, BW_SQTY, INQTY, ACC_QTY, CFM_QTY, ACC_BASEUNIT, STATUS, MEMO, ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, UNIT_SWAP, WEXP_ID, TX_QTY_T, INVOICE)  
                                values (:PO_NO, :MMCODE, :SEQ, :AGEN_NO, trim(:LOT_NO), 
                                case when to_char(:EXP_DATE, 'yyyy/mm/dd') = '0001/01/01' then null else to_date(to_char(:EXP_DATE, 'yyyy/mm/dd'), 'yyyy/mm/dd') end, 
                                0, :INQTY, :ACC_QTY*:UNIT_SWAP, :ACC_QTY *:UNIT_SWAP,:BASE_UNIT, 'C', '單筆接收', sysdate, :ACC_USER, :M_STOREID, :MAT_CLASS,  :PO_QTY,  :M_PURUN,  
                                :UNIT_SWAP,  :WEXP_ID , :INQTY, :INVOICE )";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        public int AccAllCreate(CC0002D cc0002d)
        {
            var sql = @"insert into BC_CS_ACC_LOG 
            (PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, EXP_DATE, BW_SQTY, INQTY, ACC_QTY, CFM_QTY, ACC_BASEUNIT, STATUS, MEMO, ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, UNIT_SWAP, WEXP_ID, FLAG, TX_QTY_T, INVOICE)
            values (:PO_NO, :MMCODE, :SEQ, :AGEN_NO, trim(:LOT_NO), 
            case when to_char(:EXP_DATE, 'yyyy/mm/dd') = '0001/01/01' then null else to_date(to_char(:EXP_DATE, 'yyyy/mm/dd'), 'yyyy/mm/dd') end, 
            0, :INQTY, :ACC_QTY*:UNIT_SWAP, :ACC_QTY*:UNIT_SWAP,:BASE_UNIT, 'C', '一鍵接收', 
            sysdate, :ACC_USER, :M_STOREID, :MAT_CLASS, :PO_QTY, :M_PURUN, :UNIT_SWAP, :WEXP_ID, 'A', :ACC_QTY, :INVOICE) ";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        public int CreateUdiLog(DecodeResponse deResponse)
        {
            var sql = @"insert into BC_UDI_LOG 
            (LOG_TIME, WMMID, WMCMPY, WMWHS, WMORG, CRVMPY, CRITM, WMREFCODE, WMBOX, WMLOC, WMSRV, WMSKU, WMMIDNAME, WMMIDNAMEH, WMSKUSPEC, WMBRAND, WMMDL, WMMIDCTG, WMEFFCDATE, WMLOT, WMSENO, WMPAK, 
                                WMQY, THISBARCODE, UDIBARCODES, GTINSTRING, NHIBARCODE, NHIBARCODES, BARCODETYPE, GTININSTRING, RESULT, ERRMSG)  
                                values (sysdate, :WmMid, :WmCmpy, :WmWhs, :WmOrg, :CrVmpy, :CrItm, :WmRefCode, :WmBox, :WmLoc, :WmSrv, :WmSku, :WmMidName, :WmMidNameH, :WmSkuSpec, :WmBrand, :WmMdl, :WmMidCtg, :WmEffcDate, :WmLot, :WmSeno, :WmPak, 
                                :WmQy, :ThisBarcode, :UdiBarcodes, :GtinString, :NhiBarcode, :NhiBarcodes, :BarcodeType, :GtinInString, :Result, :ErrMsg)";
            return DBWork.Connection.Execute(sql, deResponse, DBWork.Transaction);
        }

        public int UpdateUdiLog(DecodeResponse deResponse)
        {
            var sql = @" update BC_UDI_LOG set LOG_TIME = sysdate, WMCMPY = :WmCmpy, WMWHS = :WmWhs, WMORG = :WmOrg, CRVMPY = :CrVmpy, CRITM = :CrItm, WMREFCODE = :WmRefCode, WMBOX = :WmBox, WMLOC = :WmLoc, WMSRV = :WmSrv, WMSKU = :WmSku, 
                                WMMIDNAME = :WmMidName, WMMIDNAMEH = :WmMidNameH, WMSKUSPEC = :WmSkuSpec, WMBRAND = :WmBrand, WMMDL = :WmMdl, WMMIDCTG = :WmMidCtg, WMEFFCDATE = :WmEffcDate, WMLOT = :WmLot, WMSENO = :WmSeno, WMPAK = :WmPak, 
                                WMQY = :WmQy, THISBARCODE = :ThisBarcode, UDIBARCODES = :UdiBarcodes, GTINSTRING = :GtinString, NHIBARCODE = :NhiBarcode, NHIBARCODES = :NhiBarcodes, BARCODETYPE = :BarcodeType, GTININSTRING = :GtinInString, RESULT = :Result, ERRMSG = :ErrMsg
                                where WMMID=:WmMid ";

            if (deResponse.WmLot == "" || deResponse.WmLot == null)
                sql += " and (WMLOT = '' or WMLOT is null) ";
            else
                sql += " and WMLOT=:WmLot ";

            return DBWork.Connection.Execute(sql, deResponse, DBWork.Transaction);
        }

        public int UpdatePhReply(CC0002D cc0002d)
        {
            var sql = @" update PH_REPLY set STATUS = 'R', UPDATE_TIME = SYSDATE, UPDATE_USER = :ACC_USER
                                where PO_NO=:PO_NO and AGEN_NO=:AGEN_NO and MMCODE=:MMCODE and STATUS='B' and SEQ=:PH_REPLY_SEQ";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }
        public int UpdatePhReplyNoSeq(CC0002D cc0002d)
        {
            var sql = @" update PH_REPLY set STATUS = 'R', UPDATE_TIME = SYSDATE, UPDATE_USER = :ACC_USER
                                where PO_NO=:PO_NO and AGEN_NO=:AGEN_NO and MMCODE=:MMCODE and STATUS='B' ";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }
        public int CreateDistL(CC0002D cc0002d)
        {
            var sql = @"insert into BC_CS_DIST_LOG (PO_NO, MMCODE, SEQ, LOAD_TIME, AGEN_NO, PR_DEPT, PR_QTY, DOCNO, DIST_BASEUNIT, BW_SQTY, DIST_STATUS, DIST_QTY, LOT_NO, EXP_DATE )  
                                select A.PO_NO, A.MMCODE, :SEQ, sysdate, :AGEN_NO, A.INID, A.APPQTY, A.DOCNO, :BASE_UNIT, '0', 'L', A.APPQTY,
                                (select LOT_NO from BC_CS_ACC_LOG where PO_NO = A.PO_NO and MMCODE = A.MMCODE and SEQ = :SEQ and rownum = 1), 
                                (select EXP_DATE from BC_CS_ACC_LOG where PO_NO = A.PO_NO and MMCODE = A.MMCODE and SEQ = :SEQ and rownum = 1)
                                from PH_PO_N A where A.PO_NO = :PO_NO and A.MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        public int CreateDistC(CC0002D cc0002d)
        {  //DIST_QTY=PH_PO_N[PO_QTY]
            var sql = @"insert into BC_CS_DIST_LOG (PO_NO, MMCODE, SEQ, LOAD_TIME, AGEN_NO, PR_DEPT, PR_QTY, DOCNO, DIST_BASEUNIT, BW_SQTY, LOT_NO, EXP_DATE, DIST_STATUS, DIST_QTY, DIST_TIME, DIST_USER)  
                                select A.PO_NO, A.MMCODE, :SEQ, sysdate, :AGEN_NO, A.INID, A.APPQTY, A.DOCNO, :BASE_UNIT, '0', 
                                (select LOT_NO from BC_CS_ACC_LOG where PO_NO = A.PO_NO and MMCODE = A.MMCODE and SEQ = :SEQ and rownum = 1), 
                                (select EXP_DATE from BC_CS_ACC_LOG where PO_NO = A.PO_NO and MMCODE = A.MMCODE and SEQ = :SEQ and rownum = 1), 'C', A.PO_QTY, sysdate, :ACC_USER
                                from PH_PO_N A where A.PO_NO = :PO_NO and A.MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }
        public int UpdateDist(CC0002D cc0002d)
        {   // LOT_NO, EXP_DATE 已先 insert
            var sql = @"update BC_CS_DIST_LOG set  LOAD_TIME=sysdate,                            
                                DIST_QTY=:DIST_QTY, DIST_STATUS='C', 
                                DIST_TIME=sysdate, DIST_USER=:ACC_USER
                                where PO_NO=:PO_NO and MMCODE=:MMCODE and SEQ=:SEQ and PR_DEPT=:INID
                                        and DIST_STATUS='L' ";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        public int InsertDIST_LOG(CC0002D cc0002d, string deli_status)
        {
            var sql = "";
            if (deli_status == "C")
                sql = @"insert into BC_CS_DIST_LOG (PO_NO, MMCODE, SEQ, LOAD_TIME, AGEN_NO, PR_DEPT, PR_QTY, DOCNO, DIST_BASEUNIT, BW_SQTY, LOT_NO, EXP_DATE, DIST_QTY, DIST_TIME, DIST_USER, DIST_STATUS)  
                            select A.PO_NO, A.MMCODE, :SEQ, sysdate, :AGEN_NO, A.INID, A.APPQTY, A.DOCNO, :BASE_UNIT, '0', 
                            (select LOT_NO from BC_CS_ACC_LOG where PO_NO = A.PO_NO and MMCODE = A.MMCODE and SEQ = :SEQ and rownum = 1), 
                            (select EXP_DATE from BC_CS_ACC_LOG where PO_NO = A.PO_NO and MMCODE = A.MMCODE and SEQ = :SEQ and rownum = 1), :DIST_QTY, sysdate, :ACC_USER, 'C'
                        from PH_PO_N A where A.PO_NO = :PO_NO and A.MMCODE = :MMCODE and INID=:INID ";
            else
                sql = @"insert into BC_CS_DIST_LOG (PO_NO, MMCODE, SEQ, LOAD_TIME, AGEN_NO, PR_DEPT, PR_QTY, DOCNO, DIST_BASEUNIT, BW_SQTY, LOT_NO, EXP_DATE, DIST_QTY, DIST_TIME, DIST_USER, DIST_STATUS)  
                            select A.PO_NO, A.MMCODE, :SEQ, sysdate, :AGEN_NO, A.INID, A.APPQTY, A.DOCNO, :BASE_UNIT, '0', 
                            (select LOT_NO from BC_CS_ACC_LOG where PO_NO = A.PO_NO and MMCODE = A.MMCODE and SEQ = :SEQ and rownum = 1), 
                            (select EXP_DATE from BC_CS_ACC_LOG where PO_NO = A.PO_NO and MMCODE = A.MMCODE and SEQ = :SEQ and rownum = 1), :DIST_QTY * :UNIT_SWAP, sysdate, :ACC_USER, 'L'
                        from PH_PO_N A where A.PO_NO = :PO_NO and A.MMCODE = :MMCODE and INID=:INID ";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        public int InsertDIST_LOG_WhnoMm1(CC0002D cc0002d)
        {
            var p = new DynamicParameters();
            var sql = @"insert into BC_CS_DIST_LOG (PO_NO, MMCODE, SEQ, LOAD_TIME, AGEN_NO, PR_DEPT, PR_QTY, DOCNO, DIST_BASEUNIT, BW_SQTY, LOT_NO, EXP_DATE, DIST_QTY, DIST_TIME, DIST_USER, DIST_STATUS)  
                            select :PO_NO, :MMCODE, :SEQ, sysdate, :AGEN_NO, whno_mm1,  
                            nvl((select sum((case when SRC_PR_QTY=0 then 0 else PR_QTY-SRC_PR_QTY end)) 
                                   from MM_PR_D where PR_NO in (select PR_NO from MM_PO_D where PO_NO=:PO_NO and MMCODE=:MMCODE ) and MMCODE=:MMCODE and AGEN_NO=:AGEN_NO),0),
                            '', :BASE_UNIT, '0', 
                            (select LOT_NO from BC_CS_ACC_LOG where PO_NO = :PO_NO and MMCODE = :MMCODE and SEQ = :SEQ and rownum = 1), 
                            (select EXP_DATE from BC_CS_ACC_LOG where PO_NO = :PO_NO and MMCODE = :MMCODE and SEQ = :SEQ and rownum = 1), :DIST_QTY, sysdate, :ACC_USER, 'T'
                         from dual";

  
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

            public int DeleteDist(CC0002D cc0002d)
        {
            var sql = @"delete from BC_CS_DIST_LOG  
                                Where PO_NO = :PO_NO and MMCODE = :MMCODE and SEQ=:SEQ and PR_DEPT=:INID and DIST_STATUS='L' ";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        public IEnumerable<string> ChkAgenno(string agenno)
        {
            var p = new DynamicParameters();
            var sql = @" select '(' || AGEN_NO || ')' || replace(replace(AGEN_NAMEC, '股份有限公司', ''), '有限公司', '') from PH_VENDER where 1=1 ";

            sql += " AND AGEN_NO=trim(:p0) ";
            p.Add(":p0", agenno);

            return DBWork.Connection.Query<string>(sql, p, DBWork.Transaction);
        }

        public string ChkMmcodeBarcode(string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select case when (select count(*) from BC_BARCODE where (BARCODE=trim(:p0) or MMCODE=trim(:p0)) and status='Y') > 0
                            then (select MMCODE from BC_BARCODE where (BARCODE=trim(:p0) or MMCODE=trim(:p0)) and status='Y' and rownum = 1)
                            else 'notfound' end as MMCODE  from dual ";

            p.Add(":p0", mmcode);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }
        public string ChkMmcodeBarcode(string mmcode, string po_no)
        {
            var p = new DynamicParameters();
            var sql = @" select case when (select count(*) from BC_BARCODE where BARCODE=:p0 or MMCODE=:p0) > 0
                            then (select MMCODE from BC_BARCODE where BARCODE=:p0 and status='Y' and rownum = 1)
                            when (select count(*) from PH_REPLY where BARCODE=:p0 and PO_NO=:p1 and status='B' ) > 0
                            then (select MMCODE from PH_REPLY where BARCODE=:p0 and PO_NO=:p1 and status='B' and rownum = 1 )
                            else 'notfound' end as MMCODE
                            from dual ";

            p.Add(":p0", mmcode);
            p.Add(":p1", po_no);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public int ChkMmcodePo(string mmcode, string po_no)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from MM_PO_D where MMCODE = :p0 and PO_NO = :p1 ";

            p.Add(":p0", mmcode);
            p.Add(":p1", po_no);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public int getUdiMmcodeCnt(string mmcode, string lot_no)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from BC_UDI_LOG where WMMID = :p0 ";

            if (lot_no == "" || lot_no == null)
                sql += " and (WMLOT = '' or WMLOT is null) ";
            else
                sql += " and WMLOT=:p1 ";

            p.Add(":p0", mmcode);
            p.Add(":p1", lot_no);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public string GetHadDistQty(string po_no, string mmcode, string seq)
        {
            var p = new DynamicParameters();
            var sql = @" select case when (select count(DIST_QTY) from BC_CS_DIST_LOG where PO_NO = :p0 and MMCODE = :p1 and SEQ = :p2) = 0
                            then 0
                            else (select sum(DIST_QTY) from BC_CS_DIST_LOG where PO_NO = :p0 and MMCODE = :p1 and SEQ = :p2) end as HAD_DIST_QTY
                            from dual ";

            p.Add(":p0", po_no);
            p.Add(":p1", mmcode);
            p.Add(":p2", seq);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public string getSeq()
        {
            // 2019/07/01指示:SEQ不需看PO_NO和MMCODE,是整個table的流水號
            var p = new DynamicParameters();
            var sql = @" select BC_CS_ACC_LOG_SEQ.nextval as SEQ
                            from dual ";

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public string procDocin(string PO_NO, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PONO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDIP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.PO_DOCIN", p, commandType: CommandType.StoredProcedure);
            string RetId = p.Get<OracleString>("O_RETID").Value;
            string RetMsg = p.Get<OracleString>("O_RETMSG").Value;

            if (RetId == "N")
                return "SP:" + RetMsg;
            else if (RetId == "Y")
                return "接收數量存檔..成功";
            else
                return "";
        }

        public string procDistin(string PO_NO, string MMCODE, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PONO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_MMCODE", value: MMCODE, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDIP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.DIST_IN", p, commandType: CommandType.StoredProcedure);
            string RetId = p.Get<OracleString>("O_RETID").Value;
            string RetMsg = p.Get<OracleString>("O_RETMSG").Value;

            if (RetId == "N")
                return "SP:" + RetMsg;
            else if (RetId == "Y")
                return "分配數量..成功";
            else
                return "";
        }
        public string procCC0002(string PO_NO, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PO_NO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_IP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("RET_CODE", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("CC0002_SUMBIT", p, commandType: CommandType.StoredProcedure);
            string RetCode = p.Get<OracleString>("RET_CODE").Value;

            return RetCode;
        }
        public int ChkDELI_QTY(CC0002D cc0002d)  //檢查 table MM_PO_D[DELI_QTY] 是否會超過
        {
            var sql = @" select DELI_QTY from MM_PO_D
                         where po_no= :PO_NO and mmcode= :MMCODE ";
            return DBWork.Connection.QueryFirst<int>(sql, cc0002d, DBWork.Transaction);
        }
        public int ChkAccLogExist(CC0002D cc0002d)  //檢查 BC_CS_ACC_LOG 是否已新增, 避免重複進貨的問題
        {
            var sql = @" select count(*) from BC_CS_ACC_LOG
                         where po_no= :PO_NO and mmcode= :MMCODE                    
                         and (sysdate-acc_time) < 0.00035  ";  //30秒內有接收 
            if (cc0002d.LOT_NO != null)
            {
                sql += " and lot_no =:LOT_NO ";
            }
            return DBWork.Connection.QueryFirst<int>(sql, cc0002d, DBWork.Transaction);
        }

        public string DoubleIN(CC0002D cc0002d)  //檢查 進貨數量,避免重複進貨的問題
        {
            var ret = "N";
            if (ChkDELI_QTY(cc0002d) + Convert.ToInt32(cc0002d.ACC_QTY) > Convert.ToInt32(cc0002d.PO_QTY))
                ret = "FULL";
            if (ChkAccLogExist(cc0002d) != 0)
                ret = "DOUBLE";
            return ret;
        }
        public int CC0006_ChkINV_QTY(CC0002D cc0002d)
        {
            //進貨負值,檢查庫存是否足夠
            var sql = @"select INV_QTY from MI_WHINV
                        where wh_no = (select wh_no from MM_PO_M 
                                        where po_no=:PO_NO 
                                          and CREATE_USER<>'緊急醫療出貨')
                           and mmcode=:MMCODE ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, cc0002d, DBWork.Transaction);
        }
        public int CC0006_ChkONWAY_QTY(CC0002D cc0002d)  //檢查 進貨數量,避免重複進貨的問題
        {
            //進貨負值,檢查庫存是否足夠
            var sql = @"select sum(ONWAY_QTY) from MI_WHINV
                        where wh_no not in (select wh_no from MM_PO_M 
                                             where po_no=:PO_NO
                                               and CREATE_USER<>'緊急醫療出貨')
                           and mmcode=:MMCODE ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, cc0002d, DBWork.Transaction);
        }
        public int CC0006_ChkONWAY_QTY_INID(CC0002D cc0002d)  //檢查 進貨數量,避免重複進貨的問題
        {
            //進貨負值,檢查庫存是否足夠
            var sql = @"select  ONWAY_QTY from MI_WHINV
                        where wh_no =WHNO_INID(:INID) and mmcode=:MMCODE ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, cc0002d, DBWork.Transaction);
        }

        public string GetAppqty(string po_no, string mmcode) {
            string sql = @"select sum(appqty)
                             from ME_DOCD a
                            where exists (select 1 from MM_PO_D
                                           where pr_no = a.rdocno
                                             and mmcode = a.mmcode
                                             and po_no = :po_no
                                             and mmcode = :mmcode)
                            group by rdocno, mmcode                       
                    ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { po_no = po_no, mmcode = mmcode}, DBWork.Transaction);
        }
        public string getRetainQty(string po_no, string mmcode)
        {
            string sql = @"select nvl((select (case when SRC_PR_QTY=0 then PR_QTY else PR_QTY-SRC_PR_QTY end) 
                                         from MM_PR_D where PR_NO=a.PR_NO and MMCODE=a.MMCODE)/a.UNIT_SWAP,0) as retain_qty
                             from MM_PO_D a
                            where po_no = :po_no and mmcode = :mmcode                    
                    ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { po_no = po_no, mmcode = mmcode }, DBWork.Transaction);
        }
        public string getCanDistQty(string po_no, string mmcode, string agen_no, string inid)
        {
            string sql = @"select APPQTY-nvl((select sum(DIST_QTY) from BC_CS_DIST_LOG 
                                               where PO_NO=a.PO_NO and MMCODE=a.MMCODE and AGEN_NO=:agen_no and PR_DEPT=a.INID),0) as candistqty
                             from PH_PO_N a WHERE PO_NO=:po_no and MMCODE=:mmcode and INID=:inid
                          ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { po_no = po_no, mmcode = mmcode, agen_no=agen_no, inid=inid }, DBWork.Transaction);
        }
        public string getInidName(string inid)
        {
            string sql = @"select INID_NAME from UR_INID where INID=:inid";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { inid = inid }, DBWork.Transaction);
        }
    }
}