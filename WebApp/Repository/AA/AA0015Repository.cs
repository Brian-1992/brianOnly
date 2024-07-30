using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using JCLib.DB;
using JCLib.Mvc;
using Dapper;
using WebApp.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp.Repository.AA
{
    public class AA0015Repository : JCLib.Mvc.BaseRepository
    {
        public AA0015Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query, string postid, string userId, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.DOCNO, a.APPTIME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID and DOCTYPE=A.DOCTYPE ) FLOWID
                    , APPID || ' ' || USER_NAME(APPID) AS APP_NAME
                    , USER_NAME(APPID) AS APPID 
                    , APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME
                    , USEDEPT || ' ' || WH_NAME(USEDEPT) AS USEDEPT_NAME
                    , FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME
                    , TOWH || ' ' || WH_NAME(TOWH) AS TOWH_NAME
                    FROM ME_DOCM a 
                   WHERE 1=1 
                     and exists (select 1 from MI_WHID b, MI_WHMAST c
                                  where b.wh_userId = :userId 
                                    and b.wh_no = a.frwh
                                    and c.wh_no = b.wh_no
                                    and c.cancel_id = 'N')";
            p.Add(":userId", userId);
            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", query.DOCNO));
            }

            if (query.APPID != "")
            {
                sql += " AND a.APPID = user_id( :p1 ) ";
                p.Add(":p1", string.Format("{0}", query.APPID));
            }

            if (query.APPDEPT != "")
            {
                sql += " AND a.APPDEPT LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", query.APPDEPT));
            }

            //if (query.DOCTYPE != "")
            //{
            //    sql += " AND a.DOCTYPE LIKE :p3 ";
            //    p.Add(":p3", string.Format("%{0}%", query.DOCTYPE));
            //}

            //if (query.FLOWID != "")
            //{
            //    sql += " AND a.FLOWID LIKE :p4 ";
            //    p.Add(":p4", string.Format("%{0}%", query.FLOWID));
            //}

            if (query.DOCTYPE != "")
            {
                string[] tmp = query.DOCTYPE.Split(',');
                sql += " AND A.DOCTYPE IN :DOCTYPE";
                p.Add(":DOCTYPE", tmp);
            }

            if (query.FLOWID != "")
            {
                string[] tmp = query.FLOWID.Split(',');
                sql += " AND A.FLOWID IN :FLOWID";
                p.Add(":FLOWID", tmp);
            }

            //if (query.USEDEPT != "")
            //{
            //    sql += " AND a.USEDEPT LIKE :p5 ";
            //    p.Add(":p5", string.Format("%{0}%", query.USEDEPT));
            //}

            if (query.WH_NO != "")
            {
                string[] tmp = query.WH_NO.Split(',');
                sql += " AND A.FRWH IN :WH_NO";
                p.Add(":WH_NO", tmp);
            }

            if (query.FRWH != "")
            {
                sql += " AND a.FRWH LIKE :p6 ";
                p.Add(":p6", string.Format("%{0}%", query.FRWH));
            }

            if (query.TOWH != "")
            {
                sql += " AND a.TOWH LIKE :p7 ";
                p.Add(":p7", string.Format("%{0}%", query.TOWH));
            }

            if (query.APPTIME_S != "" && query.APPTIME_E != "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE(:APPTIME_S, 'yyyy/mm/dd') AND TO_DATE(:APPTIME_E, 'yyyy/mm/dd')";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }
            if (query.APPTIME_S != "" && query.APPTIME_E == "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE(:APPTIME_S, 'yyyy/mm/dd') AND TO_DATE('3000/01/01', 'yyyy/mm/dd')";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
            }
            if (query.APPTIME_S == "" && query.APPTIME_E != "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE('1900/01/01', 'yyyy/mm/dd') AND TO_DATE(:APPTIME_E, 'yyyy/mm/dd')";
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }

            string addPostN = "";
            if (postid != "")
            {
                string postidFilter = "";
                if (postid.Contains("Y")) // 已核撥
                {
                    postidFilter += "4C";
                }
                if (postid.Contains("N")) // 待核撥
                {
                    postidFilter += "3";
                    addPostN = " OR EXISTS (SELECT 1 FROM ME_DOCD B WHERE B.DOCNO=A.DOCNO AND (B.POSTID is NULL ) ) ";
                }
                if (postid.Contains("D")) // 已點收
                {
                    postidFilter += "D";
                }

                sql += " AND  (POSTID(a.DOCNO,'" + postidFilter + "') = 'Y' " + addPostN + " ) ";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAllMeDocd(ME_DOCD_QUERY_PARAMS query, string postid)
        {
            var p = new DynamicParameters();
            //var sql = @"SELECT a.DOCNO, A.MMCODE, A.SEQ, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT
            //            , NVL(A.APPQTY, 0) AS APPQTY
            //            , NVL(A.APVQTY, 0) AS APVQTY
            //            , NVL(A.SAFE_QTY, 0) as SAFE_QTY
            //            , NVL(A.OPER_QTY, 0) AS OPER_QTY
            //            , NVL(INV_QTY(a.FRWH_D, a.MMCODE), 0) S_INV_QTY
            //            , NVL(ARMY_QTY(a.MMCODE), 0) AS ARMY_QTY
            //            , NVL(INV_QTY(a.FRWH_D, a.MMCODE), 0) + NVL(ARMY_QTY(a.MMCODE), 0) AS ARMY_TOTAL_QTY
            //            , (SELECT STORE_LOC FROM MI_WLOCINV WHERE WH_NO = :WH_NO AND MMCODE = a.MMCODE) AS STORE_LOC
            //             FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE = b.MMCODE WHERE 1 = 1 ";

            //即時動態撈取SAFE_QTY, OPER_QTY
            var sql = @"SELECT a.DOCNO, A.MMCODE, A.SEQ, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT
                        , A.POSTID AS POSTIDC
                        , CASE WHEN A.POSTID IS NULL THEN '待核撥' 
                        ELSE (SELECT DATA_VALUE || ' ' || DATA_DESC from PARAM_D WHERE GRP_CODE = 'ME_DOCD' AND DATA_NAME = 'POSTID' AND DATA_VALUE = A.POSTID) END AS POSTID
                        , NVL(A.APPQTY, 0) AS APPQTY
                        , NVL(A.APVQTY, 0) AS APVQTY
                        , NVL(A.ACKQTY, 0) AS ACKQTY
                        , NVL(A.ONWAY_QTY, 0) AS ONWAY_QTY
                        , NVL((SELECT SAFE_QTY FROM MI_WINVCTL WHERE WH_NO=c.towh AND MMCODE=A.MMCODE), 0) as SAFE_QTY
                        , NVL((SELECT OPER_QTY FROM MI_WINVCTL WHERE WH_NO=c.towh AND MMCODE=A.MMCODE), 0) as OPER_QTY
                        , NVL(INV_QTY(c.FRWH, a.MMCODE), 0) S_INV_QTY
                        , NVL(ARMY_QTY(a.MMCODE), 0) AS ARMY_QTY
                        , NVL(INV_QTY(c.FRWH, a.MMCODE), 0) + NVL(ARMY_QTY(a.MMCODE), 0) AS ARMY_TOTAL_QTY
                        , GET_STORELOC(:WH_NO, a.mmcode) AS STORE_LOC
                        , (SELECT WEXP_ID FROM MI_MAST WHERE MMCODE = a.MMCODE) AS WEXP_ID
                        , a.APVTIME
                        , (select UNA from UR_ID where TUSER=a.APVID) as APVID
                        , (case when a.UPDATE_USER='強迫點收' then a.UPDATE_TIME
                                else a.ACKTIME
                           end) as ACKTIME      --點收時間  
                        , (case when a.UPDATE_USER='強迫點收' then a.UPDATE_USER
                                else USER_NAME(a.ACKID)
                           end) as ACKID        --點收人員
                        , (case when a.UPDATE_USER='強迫點收' then a.UPDATE_IP
                                else ''
                           end) as ACKSYSQTY  --強迫點收數量
                         FROM ME_DOCD a 
                         INNER JOIN ME_DOCM c on (a.DOCNO=C.DOCNO and doctype in ('MR', 'MS'))
                         LEFT JOIN MI_MAST b ON a.MMCODE = b.MMCODE WHERE 1 = 1 ";
            //AND ((C.FLOWID in ('0104', '0604') and A.ONWAY_QTY<>0) or (C.FLOWID in ('0102', '0602', '0103', '0603')))

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", query.DOCNO));
            }
            // 點選上畫面(ME_DOCM)，下畫面(ME_DOCD)該DOCNO所有資料，不受限於過帳狀態，都要顯示出來。
            //if (postid != "")
            //{
            //    if (postid == "Y")
            //    {
            //        sql += " AND  POSTID(a.DOCNO,'C') = 'Y' ";
            //    }
            //    else if (postid == "N")
            //    {
            //        sql += " AND  (a.POSTID = '3' or a.POSTID is NULL ) ";
            //    }
            //    else
            //    {
            //        sql += " AND  POSTID(a.DOCNO,'3C') = 'Y' ";
            //    }
            //}
            p.Add(":WH_NO", query.WH_NO);

            return DBWork.PagingQuery<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetPrintData_NON(ME_DOCM_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT B.MMCODE,           
                       MMCODE_NAME(B.MMCODE) AS MMNAME_E,
                       C.BASE_UNIT,
                       NVL(B.SAFE_QTY, 0) SAFE_QTY,
                       NVL(B.OPER_QTY, 0) OPER_QTY,
                       NVL(INV_QTY(A.TOWH, B.MMCODE), 0) INV_QTY,
                       NVL(ARMY_QTY(B.MMCODE), 0) ARMY_QTY,
                       NVL(INV_QTY(A.FRWH, B.MMCODE), 0)+NVL(ARMY_QTY(B.MMCODE), 0) ARMY_TOTAL_QTY,
                       NVL(INV_QTY(A.FRWH, B.MMCODE), 0) S_INV_QTY,
                       B.APPQTY,
                       B.APVQTY,
                        GET_STORELOC(:WH_NO, b.mmcode)  AS STORE_LOC
                FROM ME_DOCM A, ME_DOCD B
                LEFT JOIN MI_MAST C ON B.MMCODE = C.MMCODE
                WHERE 1 = 1
                AND A.DOCNO = :DOCNO
                 AND A.DOCNO = B.DOCNO AND (POSTID='D' or POSTID is NULL) ORDER BY STORE_LOC";

            p.Add(":DOCNO", query.DOCNO);
            p.Add(":WH_NO", query.FRWH);


            //IEnumerable<ME_DOCD> tmp = DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);

            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
            //return tmp;
        }

        public IEnumerable<ME_DOCD> GetPrintDataM_NON(ME_DOCM_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT  A.DOCNO,B.MMCODE,           
                       MMCODE_NAME(B.MMCODE) AS MMNAME_E,
                       C.BASE_UNIT,
                       NVL(B.SAFE_QTY, 0) SAFE_QTY,
                       NVL(B.OPER_QTY, 0) OPER_QTY,
                       NVL(INV_QTY(A.TOWH, B.MMCODE), 0) INV_QTY,
                       NVL(ARMY_QTY(B.MMCODE), 0) ARMY_QTY,
                       NVL(INV_QTY(A.FRWH, B.MMCODE), 0)+NVL(ARMY_QTY(B.MMCODE), 0) ARMY_TOTAL_QTY,
                       NVL(INV_QTY(A.FRWH, B.MMCODE), 0) S_INV_QTY,
                       B.APPQTY,
                       B.APVQTY,
                        GET_STORELOC(A.FRWH, B.MMCODE)AS STORE_LOC,
                       A.FRWH || ' ' ||WH_NAME(A.FRWH) as FRWH,
                       A.TOWH || ' ' || WH_NAME(A.TOWH) as TOWH,
                       WH_NAME(A.FRWH) as FRWH_NAME,
                       WH_NAME(A.TOWH) as TOWH_NAME,
                       TWN_TIME(A.APPTIME) APPTIME,
                        B.APLYITEM_NOTE
                FROM ME_DOCM A, ME_DOCD B
                LEFT JOIN MI_MAST C ON B.MMCODE = C.MMCODE
                WHERE 1 = 1 
                 AND A.DOCTYPE IN ('MR','MS') 
                 AND A.FLOWID IN ('0102','0602','0103','0603','0104','0604','0199','0699')
                 AND A.DOCNO = B.DOCNO 
                 AND (B.POSTID='3' or B.POSTID is NULL)  ";


            if (query.DOCNO_S != "" & query.DOCNO_E != "")
            {
                sql += " AND A.DOCNO BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO_S));
                p.Add(":p1", string.Format("{0}", query.DOCNO_E));
            }
            if (query.DOCNO_S != "" & query.DOCNO_E == "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO_S));
            }
            if (query.DOCNO_S == "" & query.DOCNO_E != "")
            {
                sql += " AND A.DOCNO = :p1 ";
                p.Add(":p1", string.Format("{0}", query.DOCNO_E));
            }

            if (query.APPTIME_S != "" & query.APPTIME_E != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :p2 AND :p3 ";
                p.Add(":p2", string.Format("{0}", query.APPTIME_S));
                p.Add(":p3", string.Format("{0}", query.APPTIME_E));
            }
            if (query.APPTIME_S != "" & query.APPTIME_E == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :p2 ";
                p.Add(":p2", string.Format("{0}", query.APPTIME_S));
            }
            if (query.APPTIME_S == "" & query.APPTIME_E != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :p3 ";
                p.Add(":p3", string.Format("{0}", query.APPTIME_E));
            }

            if (query.WH_NO != "")
            {
                sql += " AND A.TOWH = :p4 ";
                p.Add(":p4", string.Format("{0}", query.WH_NO));
            }
            if (query.FRWH != "" && query.FRWH != null)
            {
                sql += " AND A.FRWH = :p6 ";
                p.Add(":p6", string.Format("{0}", query.FRWH));
            }
            if (query.FLOWID != "")
            {
                sql += " AND A.FLOWID = :p11 ";
                p.Add(":p11", string.Format("{0}", query.FLOWID));
            }
            if (query.APPID != "")
            {
                sql += " AND A.APPID = :p12 ";
                p.Add(":p12", string.Format("{0}", query.APPID));
            }
            if (query.SORT == "1")
            {
                sql += @" ORDER BY case (select 1 from MI_WHMAST 
                                where WH_KIND = '0' and WH_GRADE = '1'
                                  and WH_NO = A.FRWH)
                              when 1 then substr(STORE_LOC,3,18)
                              else STORE_LOC
                          end ";
            }
            else
            {
                sql += @" ORDER BY MMCODE, case (select 1 from MI_WHMAST 
                                where WH_KIND = '0' and WH_GRADE = '1'
                                  and WH_NO = A.FRWH)
                              when 1 then substr(STORE_LOC,3,18)
                              else STORE_LOC
                          end ";
            }

            //BARCODE REF. CB0010
            Barcode b = new Barcode();
            b.IncludeLabel = true;
            List<ME_DOCD> list = new List<ME_DOCD>();
            foreach (ME_DOCD _ME_DOCD in DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction))
            {

                _ME_DOCD.DOCNO_BARCODE = "";

                BarcodeLib.TYPE type = BarcodeLib.TYPE.UNSPECIFIED;
                type = BarcodeLib.TYPE.CODE128;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_ME_DOCD.DOCNO))
                        { _ME_DOCD.DOCNO_BARCODE = ""; }
                        else
                        {
                            //Bitmap image = (Bitmap)b.Encode(type, _ME_DOCD.DOCNO, 550, 45);
                            Bitmap image = (Bitmap)b.Encode(type, _ME_DOCD.DOCNO, 150, 50);
                            //image.Save(ms, ImageFormat.Bmp);
                            image.Save(ms, ImageFormat.Jpeg);
                            byte[] byteImage = new Byte[ms.Length];
                            byteImage = ms.ToArray();
                            string strB64 = Convert.ToBase64String(byteImage);
                            _ME_DOCD.DOCNO_BARCODE = strB64;
                        }
                    }
                    catch (FormatException ex)
                    {
                        _ME_DOCD.DOCNO_BARCODE = "";
                    }
                }

                list.Add(_ME_DOCD);
            }
            return list;

            //return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetPrintDataM(ME_DOCM_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.DOCNO, B.MMCODE,           
                            MMCODE_NAME(B.MMCODE) AS MMNAME_E,
                            C.BASE_UNIT,
                            NVL(B.SAFE_QTY, 0) SAFE_QTY,
                            NVL(B.OPER_QTY, 0) OPER_QTY,
                            NVL(INV_QTY(A.TOWH, B.MMCODE), 0) INV_QTY,
                            NVL(ARMY_QTY(B.MMCODE), 0) ARMY_QTY,
                            NVL(INV_QTY(A.FRWH, B.MMCODE), 0)+NVL(ARMY_QTY(B.MMCODE), 0) ARMY_TOTAL_QTY,
                            NVL(INV_QTY(A.FRWH, B.MMCODE), 0) S_INV_QTY,
                            B.APPQTY,
                            B.APVQTY,
                            GET_STORELOC(A.FRWH, B.MMCODE) AS STORE_LOC,
                            A.FRWH || ' ' ||WH_NAME(A.FRWH) as FRWH,
                            A.TOWH || ' ' || WH_NAME(A.TOWH) as TOWH,
                            TWN_TIME(A.APPTIME) APPTIME ,
                            WH_NAME(A.FRWH) as FRWH_NAME,
                            WH_NAME(A.TOWH) as TOWH_NAME,
                            B.APLYITEM_NOTE
                          FROM ME_DOCM A, ME_DOCD B
                          LEFT JOIN MI_MAST C ON B.MMCODE = C.MMCODE
                          WHERE 1 = 1 
                            AND A.DOCTYPE IN ('MR','MS') 
                            AND A.FLOWID IN ('0102','0602','0103','0603','0104','0604','0199','0699')
                            AND A.DOCNO = B.DOCNO AND B.POSTID IN ('C','4','D') 
                            and exists (select 1 from MI_WHID d, MI_WHMAST e
                                         where d.wh_userId = :userId 
                                           and d.wh_no = a.frwh
                                           and e.wh_no = d.wh_no
                                           and e.cancel_id = 'N')";

            p.Add(":userId", string.Format("{0}", query.USERID));

            if (query.DOCNO_S != "" & query.DOCNO_E != "")
            {
                sql += " AND A.DOCNO BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO_S));
                p.Add(":p1", string.Format("{0}", query.DOCNO_E));
            }
            if (query.DOCNO_S != "" & query.DOCNO_E == "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO_S));
            }
            if (query.DOCNO_S == "" & query.DOCNO_E != "")
            {
                sql += " AND A.DOCNO = :p1 ";
                p.Add(":p1", string.Format("{0}", query.DOCNO_E));
            }

            if (query.APPTIME_S != "" & query.APPTIME_E != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :p2 AND :p3 ";
                p.Add(":p2", string.Format("{0}", query.APPTIME_S));
                p.Add(":p3", string.Format("{0}", query.APPTIME_E));
            }
            if (query.APPTIME_S != "" & query.APPTIME_E == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :p2 ";
                p.Add(":p2", string.Format("{0}", query.APPTIME_S));
            }
            if (query.APPTIME_S == "" & query.APPTIME_E != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :p3 ";
                p.Add(":p3", string.Format("{0}", query.APPTIME_E));
            }

            if (query.WH_NO == "" & query.WH_NO != "")
            {
                sql += " AND A.TOWH = :p4 ";
                p.Add(":p4", string.Format("{0}", query.WH_NO));
            }
            if (query.FRWH == "" & query.FRWH != "")
            {
                sql += " AND A.FRWH = :p6 ";
                p.Add(":p6", string.Format("{0}", query.FRWH));
            }
            if (query.SORT == "1")
            {
                sql += @" ORDER BY case (select 1 from MI_WHMAST 
                                where WH_KIND = '0' and WH_GRADE = '1'
                                  and WH_NO = A.FRWH)
                              when 1 then substr(STORE_LOC,3,18)
                              else STORE_LOC
                          end ";
            }
            else
            {
                sql += @" ORDER BY MMCODE, case (select 1 from MI_WHMAST 
                                where WH_KIND = '0' and WH_GRADE = '1'
                                  and WH_NO = A.FRWH)
                              when 1 then substr(STORE_LOC,3,18)
                              else STORE_LOC
                          end ";
            }

            //BARCODE REF. CB0010
            Barcode b = new Barcode();
            b.IncludeLabel = true;
            List<ME_DOCD> list = new List<ME_DOCD>();
            foreach (ME_DOCD _ME_DOCD in DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction))
            {
                BarcodeLib.TYPE type = BarcodeLib.TYPE.UNSPECIFIED;
                type = BarcodeLib.TYPE.CODE128;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_ME_DOCD.DOCNO))
                        { _ME_DOCD.DOCNO_BARCODE = ""; }
                        else
                        {
                            //Bitmap image = (Bitmap)b.Encode(type, _ME_DOCD.DOCNO, 550, 45);
                            Bitmap image = (Bitmap)b.Encode(type, _ME_DOCD.DOCNO, 150, 50);
                            //image.Save(ms, ImageFormat.Bmp);
                            image.Save(ms, ImageFormat.Jpeg);
                            byte[] byteImage = new Byte[ms.Length];
                            byteImage = ms.ToArray();
                            string strB64 = Convert.ToBase64String(byteImage);
                            _ME_DOCD.DOCNO_BARCODE = strB64;
                        }
                    }
                    catch (FormatException ex)
                    {
                        _ME_DOCD.DOCNO_BARCODE = "";
                    }
                }
                list.Add(_ME_DOCD);
            }
            return list;
            //return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetPrintData(ME_DOCM_QUERY_PARAMS query, string userId)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT B.MMCODE,           
                           MMCODE_NAME(B.MMCODE) AS MMNAME_E,
                           C.BASE_UNIT,
                           NVL(B.SAFE_QTY, 0) SAFE_QTY,
                           NVL(B.OPER_QTY, 0) OPER_QTY,
                           NVL(INV_QTY(A.TOWH, B.MMCODE), 0) INV_QTY,
                           NVL(ARMY_QTY(B.MMCODE), 0) ARMY_QTY,
                           NVL(INV_QTY(A.TOWH, B.MMCODE), 0)+NVL(ARMY_QTY(B.MMCODE), 0) ARMY_TOTAL_QTY,
                           NVL(INV_QTY(A.FRWH, B.MMCODE), 0) S_INV_QTY,
                           B.APPQTY,
                           B.APVQTY,
                           GET_STORELOC(:WH_NO, b.mmcode) AS STORE_LOC
                        FROM ME_DOCM A, ME_DOCD B
                        LEFT JOIN MI_MAST C ON B.MMCODE = C.MMCODE
                        WHERE 1 = 1
                          AND A.DOCNO = B.DOCNO AND POSTID='C' 
                          and exists (select 1 from MI_WHID d, MI_WHMAST e
                                       where d.wh_userId = :userId 
                                         and d.wh_no = a.frwh
                                         and e.wh_no = d.wh_no
                                         and e.cancel_id = 'N')
                        ORDER BY STORE_LOC";

            p.Add(":userId", userId);
            p.Add(":DOCNO", query.DOCNO);
            p.Add(":WH_NO", query.FRWH);

            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<string> GetWhid(string userId)
        {
            var sql = @"select WH_NO from MI_WHID WHERE WH_USERID = :WH_USERID";
            return DBWork.Connection.Query<string>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }

        public int UpdateMeDocd(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET APVQTY = :APVQTY, PICK_QTY=:APVQTY,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int UpdatePostid(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET POSTID = :POSTID, APVTIME=SYSDATE, APVID=:UPDATE_USER, PICK_QTY=APVQTY,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO=:DOCNO AND SEQ=:SEQ and NVL(POSTID, ' ')<>'C'";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public int CancelPostid(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET POSTID = :POSTID, APVTIME = null, APVID = null,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO=:DOCNO AND SEQ=:SEQ ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int UpdateAllPostid(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET POSTID = :POSTID, APVTIME=SYSDATE, APVID=:UPDATE_USER, PICK_QTY=APVQTY,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO=:DOCNO and POSTID IS NULL ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int UpdateMeDocmStatus(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int defAckqty(string docno)
        {
            var sql = @"UPDATE ME_DOCD SET ACKQTY=APVQTY
                                WHERE DOCNO=:DOCNO ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int defAckqty(string docno, string seq)
        {
            var sql = @"UPDATE ME_DOCD SET ACKQTY=APVQTY
                                WHERE DOCNO=:DOCNO AND SEQ=:SEQ ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int clearApvqty(string docno, string seq)
        {
            var sql = @"UPDATE ME_DOCD SET APVQTY = APPQTY, PICK_TIME = null, ACKQTY = 0
                                WHERE DOCNO=:DOCNO AND SEQ=:SEQ ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public string GetFlowid(string docno)
        {
            string sql = @"SELECT FLOWID FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        //public string GetPostid(string docno, string seq)
        //{
        //    string sql = @"SELECT POSTID FROM ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
        //    return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        //}

        public SP_MODEL PostDoc(string docno, string updusr, string updip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: docno, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: updusr, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };
            return sp;
        }

        public DataTable GetExcel(string docno, string wh_no, string userId)
        {
            var p = new DynamicParameters();
            //var sql = @"SELECT A.MMCODE AS 院內碼, b.MMNAME_C as 中文品名, b.MMNAME_E as 英文品名, b.BASE_UNIT as 單位
            //            , NVL(A.SAFE_QTY, 0) as 安全量
            //            , NVL(A.OPER_QTY, 0) AS 作業量
            //            , NVL(A.S_INV_QTY, 0) AS 核核庫存量
            //            , NVL(ARMY_QTY(a.MMCODE), 0) AS 軍品量
            //            , NVL(a.INV_QTY + ARMY_QTY(a.MMCODE), 0) AS 軍民總量
            //            , NVL(A.APPQTY, 0) AS 申請量
            //            , NVL(A.APVQTY, 0) AS 核撥量
            //            , (SELECT STORE_LOC FROM MI_WLOCINV WHERE WH_NO = :WH_NO AND MMCODE = a.MMCODE) AS 儲位 
            //             FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE = b.MMCODE WHERE 1 = 1 ";

            //即時動態撈取SAFE_QTY, OPER_QTY
            var sql = @"SELECT A.MMCODE AS 院內碼, b.MMNAME_C as 中文品名, b.MMNAME_E as 英文品名, b.BASE_UNIT as 單位
                        , NVL((SELECT SAFE_QTY FROM MI_WINVCTL WHERE WH_NO=c.towh AND MMCODE=A.MMCODE), 0) as 安全量
                        , NVL((SELECT OPER_QTY FROM MI_WINVCTL WHERE WH_NO=c.towh AND MMCODE=A.MMCODE), 0) as 作業量
                        , NVL(INV_QTY(a.FRWH_D, a.MMCODE), 0) 核核庫存量
                        , NVL(ARMY_QTY(a.MMCODE), 0) AS 軍品量
                        , NVL(INV_QTY(a.FRWH_D, a.MMCODE), 0) + NVL(ARMY_QTY(a.MMCODE), 0) AS 軍民總量
                        , NVL(A.APPQTY, 0) AS 申請量
                        , NVL(A.APVQTY, 0) AS 核撥量
                        , GET_STORELOC(:WH_NO, a.mmcode) AS 儲位 
                         FROM ME_DOCD a 
                        INNER JOIN ME_DOCM c on a.DOCNO=C.DOCNO
                        LEFT JOIN MI_MAST b ON a.MMCODE = b.MMCODE 
                       WHERE 1 = 1 
                         AND ((C.FLOWID in ('0104', '0604') and A.ONWAY_QTY<>0) or (C.FLOWID in ('0102', '0602')))
                         and exists (select 1 from MI_WHID 
                                      where wh_userId = :userId 
                                        and wh_no = c.frwh
                                        and exists (select 1 from MI_WHMAST 
                                                     where wh_no = c.frwh
                                                       and cancel_id = 'N'))";

            if (docno != "")
            {
                sql += " AND a.DOCNO = :p0 ";
                p.Add(":p0", docno);
            }

            p.Add(":WH_NO", wh_no);

            sql += @" ORDER BY seq";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetThisTowh(string docno)
        {
            var sql = @"select TOWH from me_docm where DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetThisFrwh(string docno)
        {
            var sql = @"select FRWH from me_docm where DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetThisApptime(string docno)
        {
            var sql = @"select twn_time(APPTIME) AS APPTIME from me_docm where DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public class ME_DOCM_QUERY_PARAMS
        {
            public string DOCNO;
            public string DOCTYPE;
            public string APPID;
            public string APPDEPT;
            public string USEDEPT;
            public string FLOWID;
            public string FRWH;
            public string TOWH;
            public string APPTIME_S;
            public string APPTIME_E;
            public string WH_NO;
            public string DOCNO_S;
            public string DOCNO_E;
            public string SORT;

            public string USERID;
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
            public string WH_NO;
        }

        public IEnumerable<COMBO_MODEL> GetPostidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='POSTID1'  
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetAppdeptCombo()
        {
            string sql = @"SELECT DISTINCT A.APPDEPT as VALUE, B.INID_NAME as TEXT ,
                        A.APPDEPT || ' ' || B.INID_NAME as COMBITEM 
                        FROM ME_DOCM A ,UR_INID B
                        WHERE A.APPDEPT=B.INID AND A.DOCTYPE IN ('MR','MS')  
                        ORDER BY A.APPDEPT ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetUsedeptCombo()
        {
            string sql = @"SELECT DISTINCT A.USEDEPT as VALUE, B.INID_NAME as TEXT ,
                        A.USEDEPT || ' ' || B.INID_NAME as COMBITEM 
                        FROM ME_DOCM A ,UR_INID B
                        WHERE A.USEDEPT=B.INID AND A.DOCTYPE IN ('MR','MS')  
                        ORDER BY A.USEDEPT ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo(string id)
        {
            string sql = @"SELECT WH_NO as VALUE, WH_NAME(WH_NO) as TEXT ,
                        WH_NO || ' ' || WH_NAME(WH_NO) as COMBITEM 
                        FROM MI_WHID A
                        WHERE WH_USERID = :TUSER  
                        and (select 1 from MI_WHMAST where WH_NO=A.WH_NO and WH_KIND='0')=1
                        ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetTowhCombo()
        {
            string sql = @" SELECT WH_NO as VALUE, WH_NAME(WH_NO) as TEXT ,
                        WH_NO || ' ' || WH_NAME(WH_NO) as COMBITEM 
                        FROM MI_WHMAST 
                        WHERE WH_KIND = '0' AND WH_GRADE in ('2','3','4')
                        ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetDocnoCombo()
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO || '(' || TOWH || ')' as COMBITEM,APPTIME EXTRA1 
                        FROM ME_DOCM 
                        WHERE 1=1 AND DOCTYPE IN ('MR','MS') 
                        AND FLOWID IN ('0102','0602','0103','0603','0104','0604','0199','0699')
                        ORDER BY  EXTRA1 DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        // 計算申請單底下的品項,若為批號管制品項且未建立批號資料者
        public int ChkWexpid(string docno)
        {
            // 條件:
            // 1:WEXP_ID為Y但ME_DOCEXP沒有資料
            // 2:未輸入效期的移出數量(ME_DOCEXP.APVQTY)
            // 3:效期的sum(移出數量)與核撥量不符
            // 4:核撥量=0,則該院內碼不必輸入效期
            string sql = @" select count(*) from ME_DOCD A 
                        where ((select count(*) from ME_DOCEXP 
                         where DOCNO=A.DOCNO and SEQ=A.SEQ) = 0 
                          or (select count(*) from ME_DOCEXP where DOCNO=A.DOCNO and SEQ=A.SEQ
                          and APVQTY is null) > 0
                         or (select sum(APVQTY) from ME_DOCEXP where DOCNO=A.DOCNO and SEQ=A.SEQ) <> A.APVQTY)
                        and (select WEXP_ID from MI_MAST where MMCODE = A.MMCODE) = 'Y'
                        and A.DOCNO = :DOCNO
                        and A.APVQTY > 0 ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int ChkWexpid(string docno, string seq)
        {
            // 條件:
            // 1:WEXP_ID為Y但ME_DOCEXP沒有資料
            // 2:未輸入效期的移出數量(ME_DOCEXP.APVQTY)
            // 3:效期的sum(移出數量)與核撥量不符
            // 4:核撥量=0,則該院內碼不必輸入效期
            string sql = @" select count(*) from ME_DOCD A 
                        where ((select count(*) from ME_DOCEXP 
                         where DOCNO=A.DOCNO and SEQ=A.SEQ) = 0 
                          or (select count(*) from ME_DOCEXP where DOCNO=A.DOCNO and SEQ=A.SEQ
                          and APVQTY is null) > 0
                         or (select sum(APVQTY) from ME_DOCEXP where DOCNO=A.DOCNO and SEQ=A.SEQ) <> A.APVQTY)
                        and (select WEXP_ID from MI_MAST where MMCODE = A.MMCODE) = 'Y'
                        and A.DOCNO = :DOCNO
                        and A.SEQ = :SEQ
                        and A.APVQTY > 0 ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int DeleteDocexp(string docno, string seq)
        {
            var sql = @" delete from ME_DOCEXP where DOCNO=:DOCNO and SEQ=:SEQ ";

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public string chkDefSort(string wh_no)
        {
            string sql = @"select case when (WH_KIND = '0' and WH_GRADE = '2') then '0' else '1' end from MI_WHMAST
                            where WH_NO=:WH_NO ";
            return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        #region 2020-07-20: 新增退回功能
        public int Return(string docno, string return_note, string user_name, string user_id, string update_ip)
        {
            string sql = @"update ME_DOCM
                              set flowid = (case
                                                when flowid = '0102'
                                                    then '0101'
                                                when flowid = '0602'
                                                    then '0601'
                                           end),
                                  return_note = (:user_name || twn_time(sysdate) || '退回：' || :return_note),
                                  update_time = sysdate, 
                                  update_user = :user_id,
                                  update_ip = :update_ip
                            where docno = :docno";
            return DBWork.Connection.Execute(sql, new
            {
                docno = docno,
                return_note = return_note,
                user_name = user_name,
                user_id = user_id,
                update_ip = update_ip
            }, DBWork.Transaction);

        }
        #endregion

        #region 2021-07-29 CheckWhidValid
        public bool CheckWhidValid(string docno, string userId) {
            string sql = @"
                select 1 from ME_DOCM a
                 where a.docno = :docno
                   and exists (select 1 from MI_WHID
                                where wh_userId = :userId
                                  and wh_no = a.frwh)
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno, userId }, DBWork.Transaction) != null;
        }
        #endregion

        #region
        public string GetHospName()
        {
            string sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospName' ";
            return DBWork.Connection.ExecuteScalar<string>(sql, DBWork.Transaction);
        }

        public string GetHospFullName()
        {
            string sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospFullName' ";
            return DBWork.Connection.ExecuteScalar<string>(sql, DBWork.Transaction);
        }
        #endregion
    }
}