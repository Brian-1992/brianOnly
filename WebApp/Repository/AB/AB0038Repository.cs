using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0038Repository : JCLib.Mvc.BaseRepository
    {
        public AB0038Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //一進入程式，將TEMP檔清空
        public int DeleteTemp(string UPDATE_IP)
        {
            var sql = @"DELETE FROM TMP_AB0038
                        WHERE IP = :UPDATE_IP";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        //一進入程式，將預設欄位匯入TEMP檔
        public int CreateTemp(string UPDATE_IP)
        {
            var sql = @"INSERT INTO TMP_AB0038 (IP,
                                                SEQ,
                                                CNAME,
                                                ENAME,
                                                CHK)
                        SELECT :UPDATE_IP,
                               SEQ,
                               CNAME,
                               ENAME,
                               CHK
                        FROM TMP_AB0038P";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        //欄位設定左視窗
        public IEnumerable<AB0038VM> GetColList(string UPDATE_IP, string P0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT SEQ, CNAME, ENAME
                        FROM TMP_AB0038
                        WHERE (CHK <> 'X'
                               OR CHK IS NULL)
                        AND IP = :UPDATE_IP ";

            if (P0 != "")
            {
                sql += @" AND (CNAME LIKE :P0 
                              OR ENAME LIKE :P0)";
                p.Add(":P0", string.Format("%{0}%", P0));
            }

            p.Add(":UPDATE_IP", string.Format("{0}", UPDATE_IP));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0038VM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //欄位設定右視窗
        public IEnumerable<AB0038VM> GetDefaultList(string UPDATE_IP, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT SEQ, CNAME, ENAME
                        FROM TMP_AB0038
                        WHERE CHK = 'X'
                        AND IP = :UPDATE_IP";

            p.Add(":UPDATE_IP", string.Format("{0}", UPDATE_IP));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0038VM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //取得目前欲新增的SEQ -- 加入欄位STEP 1
        public string GetNewSeq(string UPDATE_IP)
        {
            var sql = @"SELECT NVL (MAX (SEQ), 0) + 1 SEQ
                        FROM TMP_AB0038
                        WHERE IP = :UPDATE_IP
                        AND CHK = 'X'";

            return DBWork.Connection.QueryFirst<string>(sql, new { UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        //先將該序號原本的項目SEQ設為1000 -- 加入欄位STEP 2
        public int SEQ1000(string UPDATE_IP, string NEWSEQ)
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = '1000'
                        WHERE IP = :UPDATE_IP
                        AND SEQ = :NEWSEQ";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, NEWSEQ = NEWSEQ }, DBWork.Transaction);
        }

        //將要新增的項目改為新SEQ及FLAG -- 加入欄位STEP 3
        public int JoinCol(string UPDATE_IP, string ENAME, string SEQ, string NEWSEQ) //修改
        {
            var sql = @"UPDATE TMP_AB0038
                        SET CHK = 'X',
                            SEQ = :NEWSEQ
                        WHERE IP = :UPDATE_IP
                        AND ENAME = :ENAME
                        AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, ENAME = ENAME, SEQ = SEQ, NEWSEQ = NEWSEQ }, DBWork.Transaction);
        }

        //將原本的項目改回被替換的SEQ -- 加入欄位STEP 4
        public int ChangeSeq(string UPDATE_IP, string ENAME, string SEQ) //修改
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = :SEQ
                        WHERE IP = :UPDATE_IP
                        AND SEQ = '1000'";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, ENAME = ENAME, SEQ = SEQ }, DBWork.Transaction);
        }

        //將該項目的SEQ設為1000 -- 刪除/上移/下移欄位STEP 1
        public int SEQ1000_D(string UPDATE_IP, string SEQ)
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = '1000'
                        WHERE IP = :UPDATE_IP
                        AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, SEQ = SEQ }, DBWork.Transaction);
        }

        //將該序號之後的所有SEQ - 1 -- 刪除欄位STEP 2
        public int SetAllSeq(string UPDATE_IP, string SEQ)
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = SEQ - 1
                        WHERE IP = :UPDATE_IP
                        AND SEQ > :SEQ";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, SEQ = SEQ }, DBWork.Transaction);
        }

        //將該項目設為最後一個SEQ -- 刪除欄位STEP 3
        public int SetLastSeq(string UPDATE_IP)
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = ( SELECT MAX (SEQ) + 1
                                    FROM TMP_AB0038
                                    WHERE IP = :UPDATE_IP
                                    AND SEQ <> '999'),
                            CHK = ''
                        WHERE IP = :UPDATE_IP
                        AND SEQ = '999'";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        //將該項目的上一項SEQ + 1 -- 欄位上移STEP 2
        public int SetUpSeq_1(string UPDATE_IP, string SEQ)
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = SEQ + 1
                        WHERE IP = :UPDATE_IP
                        AND SEQ = :SEQ - 1";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, SEQ = SEQ }, DBWork.Transaction);
        }

        //將該項目的SEQ - 1 -- 欄位上移STEP 3
        public int SetUpSeq_2(string UPDATE_IP, string SEQ)
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = :SEQ - 1
                        WHERE IP = :UPDATE_IP
                        AND SEQ = '1000'";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, SEQ = SEQ }, DBWork.Transaction);
        }

        //將該項目的下一項SEQ - 1 -- 欄位下移STEP 2
        public int SetDownSeq_1(string UPDATE_IP, string SEQ)
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = SEQ - 1
                        WHERE IP = :UPDATE_IP
                        AND SEQ = :SEQ + 1";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, SEQ = SEQ }, DBWork.Transaction);
        }

        //將該項目的SEQ + 1 -- 欄位下移STEP 3
        public int SetDownSeq_2(string UPDATE_IP, string SEQ)
        {
            var sql = @"UPDATE TMP_AB0038
                        SET SEQ = :SEQ + 1
                        WHERE IP = :UPDATE_IP
                        AND SEQ = '1000'";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP, SEQ = SEQ }, DBWork.Transaction);
        }

        //載入後取得已選的欄位
        public IEnumerable<AB0038VM> GetCol(string UPDATE_IP)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT CNAME, ENAME
                        FROM TMP_AB0038
                        WHERE CHK = 'X' AND IP = :UPDATE_IP
                        ORDER BY SEQ";

            p.Add(":UPDATE_IP", string.Format("{0}", UPDATE_IP));

            return DBWork.Connection.Query<AB0038VM>(sql, p, DBWork.Transaction);
        }

        //檢查院內代碼是否存在於HIS_BASORDM中
        public bool Check_HIS_BASORDM(string ORDERCODE)
        {
            string sql = @"SELECT 1
                           FROM HIS_BASORDM
                           WHERE ORDERCODE = :ORDERCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { ORDERCODE = ORDERCODE }, DBWork.Transaction) == null);
        }

        //檢查院內代碼是否存在於HIS_BASORDD中
        public bool Check_HIS_BASORDD(string ORDERCODE)
        {
            string sql = @"SELECT 1
                           FROM HIS_BASORDD
                           WHERE ORDERCODE = :ORDERCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { ORDERCODE = ORDERCODE }, DBWork.Transaction) == null);
        }

        //檢查院內代碼是否存在於HIS_STKDMIT中
        public bool Check_HIS_STKDMIT(string SKORDERCODE)
        {
            string sql = @"SELECT 1
                           FROM HIS_STKDMIT
                           WHERE SKORDERCODE = :SKORDERCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { SKORDERCODE = SKORDERCODE }, DBWork.Transaction) == null);
        }

        //比對是否無異動
        public bool Check_NO_Change(AB0038 AB0038)
        {
            string sql = @"SELECT 1
  FROM HIS_BASORDM HM, HIS_BASORDD HD, HIS_STKDMIT HT
 WHERE     HM.ORDERCODE = HD.ORDERCODE
       AND HD.ORDERCODE = HT.SKORDERCODE
       AND HM.ORDERCODE = :ORDERCODE
       AND NVL (TO_CHAR (HD.AGENTNAME), '-1') =
              NVL (TO_CHAR ( :AGENTNAME), NVL (TO_CHAR (HD.AGENTNAME), '-1'))
       AND NVL (TO_CHAR (HD.ARMYINSUAMOUNT), '-1') =
              NVL (TO_CHAR ( :ARMYINSUAMOUNT),
                   NVL (TO_CHAR (HD.ARMYINSUAMOUNT), '-1'))
       AND NVL (TO_CHAR (HD.ARMYINSUORDERCODE), '-1') =
              NVL (TO_CHAR ( :ARMYINSUORDERCODE),
                   NVL (TO_CHAR (HD.ARMYINSUORDERCODE), '-1'))
       AND NVL (TO_CHAR (HD.ATTACHTRANSQTYO), '-1') =
              NVL (TO_CHAR ( :ATTACHTRANSQTYO),
                   NVL (TO_CHAR (HD.ATTACHTRANSQTYO), '-1'))
       AND NVL (TO_CHAR (HD.ATTACHTRANSQTYI), '-1') =
              NVL (TO_CHAR ( :ATTACHTRANSQTYI),
                   NVL (TO_CHAR (HD.ATTACHTRANSQTYI), '-1'))
       AND NVL (TO_CHAR (HD.CASEFROM), '-1') =
              NVL (TO_CHAR ( :CASEFROM), NVL (TO_CHAR (HD.CASEFROM), '-1'))
       AND NVL (TO_CHAR (HD.CONTRACNO), '-1') =
              NVL (TO_CHAR ( :CONTRACNO), NVL (TO_CHAR (HD.CONTRACNO), '-1'))
       AND NVL (TO_CHAR (HD.CONTRACTPRICE), '-1') =
              NVL (TO_CHAR ( :CONTRACTPRICE),
                   NVL (TO_CHAR (HD.CONTRACTPRICE), '-1'))
       AND NVL (TO_CHAR (HD.COSTAMOUNT), '-1') =
              NVL (TO_CHAR ( :COSTAMOUNT),
                   NVL (TO_CHAR (HD.COSTAMOUNT), '-1'))
       AND NVL (TO_CHAR (HD.DENTALREFFLAG), '-1') =
              NVL (TO_CHAR ( :DENTALREFFLAG),
                   NVL (TO_CHAR (HD.DENTALREFFLAG), '-1'))
       AND NVL (TO_CHAR (HD.DRUGCASEFROM), '-1') =
              NVL (TO_CHAR ( :DRUGCASEFROM),
                   NVL (TO_CHAR (HD.DRUGCASEFROM), '-1'))
       AND NVL (TO_CHAR (HD.ENDDATETIME), '-1') =
              NVL (TO_CHAR ( :ENDDATETIME),
                   NVL (TO_CHAR (HD.ENDDATETIME), '-1'))
       AND NVL (TO_CHAR (HD.EXAMINEDISCFLAG), '-1') =
              NVL (TO_CHAR ( :EXAMINEDISCFLAG),
                   NVL (TO_CHAR (HD.EXAMINEDISCFLAG), '-1'))
       AND NVL (TO_CHAR (HD.EXECFLAG), '-1') =
              NVL (TO_CHAR ( :EXECFLAG), NVL (TO_CHAR (HD.EXECFLAG), '-1'))
       AND NVL (TO_CHAR (HD.HOSPEMGFLAG), '-1') =
              NVL (TO_CHAR ( :HOSPEMGFLAG),
                   NVL (TO_CHAR (HD.HOSPEMGFLAG), '-1'))
       AND NVL (TO_CHAR (HD.HOSPKIDFLAG), '-1') =
              NVL (TO_CHAR ( :HOSPKIDFLAG),
                   NVL (TO_CHAR (HD.HOSPKIDFLAG), '-1'))
       AND NVL (TO_CHAR (HD.INSUAMOUNT1), '-1') =
              NVL (TO_CHAR ( :INSUAMOUNT1),
                   NVL (TO_CHAR (HD.INSUAMOUNT1), '-1'))
       AND NVL (TO_CHAR (HD.INSUAMOUNT2), '-1') =
              NVL (TO_CHAR ( :INSUAMOUNT2),
                   NVL (TO_CHAR (HD.INSUAMOUNT2), '-1'))
       AND NVL (TO_CHAR (HD.INSUEMGFLAG), '-1') =
              NVL (TO_CHAR ( :INSUEMGFLAG),
                   NVL (TO_CHAR (HD.INSUEMGFLAG), '-1'))
       AND NVL (TO_CHAR (HD.INSUKIDFLAG), '-1') =
              NVL (TO_CHAR ( :INSUKIDFLAG),
                   NVL (TO_CHAR (HD.INSUKIDFLAG), '-1'))
       AND NVL (TO_CHAR (HD.INSUORDERCODE), '-1') =
              NVL (TO_CHAR ( :INSUORDERCODE),
                   NVL (TO_CHAR (HD.INSUORDERCODE), '-1'))
       AND NVL (TO_CHAR (HD.INSUSIGNI), '-1') =
              NVL (TO_CHAR ( :INSUSIGNI), NVL (TO_CHAR (HD.INSUSIGNI), '-1'))
       AND NVL (TO_CHAR (HD.INSUSIGNO), '-1') =
              NVL (TO_CHAR ( :INSUSIGNO), NVL (TO_CHAR (HD.INSUSIGNO), '-1'))
       AND NVL (TO_CHAR (HD.MAMAGEFLAG), '-1') =
              NVL (TO_CHAR ( :MAMAGEFLAG),
                   NVL (TO_CHAR (HD.MAMAGEFLAG), '-1'))
       AND NVL (TO_CHAR (HD.MAMAGERATE), '-1') =
              NVL (TO_CHAR ( :MAMAGERATE),
                   NVL (TO_CHAR (HD.MAMAGERATE), '-1'))
       AND NVL (TO_CHAR (HD.ORIGINALPRODUCER), '-1') =
              NVL (TO_CHAR ( :ORIGINALPRODUCER),
                   NVL (TO_CHAR (HD.ORIGINALPRODUCER), '-1'))
       AND NVL (TO_CHAR (HD.PAYAMOUNT1), '-1') =
              NVL (TO_CHAR ( :PAYAMOUNT1),
                   NVL (TO_CHAR (HD.PAYAMOUNT1), '-1'))
       AND NVL (TO_CHAR (HD.PAYAMOUNT2), '-1') =
              NVL (TO_CHAR ( :PAYAMOUNT2),
                   NVL (TO_CHAR (HD.PAYAMOUNT2), '-1'))
       AND NVL (TO_CHAR (HD.PPFPERCENTAGE), '-1') =
              NVL (TO_CHAR ( :PPFPERCENTAGE),
                   NVL (TO_CHAR (HD.PPFPERCENTAGE), '-1'))
       AND NVL (TO_CHAR (HD.PPFTYPE), '-1') =
              NVL (TO_CHAR ( :PPFTYPE), NVL (TO_CHAR (HD.PPFTYPE), '-1'))
       AND NVL (TO_CHAR (HD.PTRESOLUTIONCLASS), '-1') =
              NVL (TO_CHAR ( :PTRESOLUTIONCLASS),
                   NVL (TO_CHAR (HD.PTRESOLUTIONCLASS), '-1'))
       AND NVL (TO_CHAR (HD.STOCKTRANSQTYI), '-1') =
              NVL (TO_CHAR ( :STOCKTRANSQTYI),
                   NVL (TO_CHAR (HD.STOCKTRANSQTYI), '-1'))
       AND NVL (TO_CHAR (HD.STOCKTRANSQTYO), '-1') =
              NVL (TO_CHAR ( :STOCKTRANSQTYO),
                   NVL (TO_CHAR (HD.STOCKTRANSQTYO), '-1'))
       AND NVL (TO_CHAR (HD.SUPPLYNO), '-1') =
              NVL (TO_CHAR ( :SUPPLYNO), NVL (TO_CHAR (HD.SUPPLYNO), '-1'))
       AND NVL (TO_CHAR (HM.MAXQTYPERTIME), '-1') =
              NVL (TO_CHAR ( :MAXQTYPERTIME),
                   NVL (TO_CHAR (HM.MAXQTYPERTIME), '-1'))
       AND NVL (TO_CHAR (HM.MAXQTYPERDAY), '-1') =
              NVL (TO_CHAR ( :MAXQTYPERDAY),
                   NVL (TO_CHAR (HM.MAXQTYPERDAY), '-1'))
       AND NVL (TO_CHAR (HM.ONLYROUNDFLAG), '-1') =
              NVL (TO_CHAR ( :ONLYROUNDFLAG),
                   NVL (TO_CHAR (HM.ONLYROUNDFLAG), '-1'))
       AND NVL (TO_CHAR (HM.UNABLEPOWDERFLAG), '-1') =
              NVL (TO_CHAR ( :UNABLEPOWDERFLAG),
                   NVL (TO_CHAR (HM.UNABLEPOWDERFLAG), '-1'))
       AND NVL (TO_CHAR (HM.COLDSTORAGEFLAG), '-1') =
              NVL (TO_CHAR ( :COLDSTORAGEFLAG),
                   NVL (TO_CHAR (HM.COLDSTORAGEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.LIGHTAVOIDFLAG), '-1') =
              NVL (TO_CHAR ( :LIGHTAVOIDFLAG),
                   NVL (TO_CHAR (HM.LIGHTAVOIDFLAG), '-1'))
       AND NVL (TO_CHAR (HM.WEIGHTTYPE), '-1') =
              NVL (TO_CHAR ( :WEIGHTTYPE),
                   NVL (TO_CHAR (HM.WEIGHTTYPE), '-1'))
       AND NVL (TO_CHAR (HM.WEIGHTUNITLIMIT), '-1') =
              NVL (TO_CHAR ( :WEIGHTUNITLIMIT),
                   NVL (TO_CHAR (HM.WEIGHTUNITLIMIT), '-1'))
       AND NVL (TO_CHAR (HM.DANGERDRUGFLAG), '-1') =
              NVL (TO_CHAR ( :DANGERDRUGFLAG),
                   NVL (TO_CHAR (HM.DANGERDRUGFLAG), '-1'))
       AND NVL (TO_CHAR (HM.DANGERDRUGMEMO), '-1') =
              NVL (TO_CHAR ( :DANGERDRUGMEMO),
                   NVL (TO_CHAR (HM.DANGERDRUGMEMO), '-1'))
       AND NVL (TO_CHAR (HM.SYMPTOMCHIN), '-1') =
              NVL (TO_CHAR ( :SYMPTOMCHIN),
                   NVL (TO_CHAR (HM.SYMPTOMCHIN), '-1'))
       AND NVL (TO_CHAR (HM.SYMPTOMENG), '-1') =
              NVL (TO_CHAR ( :SYMPTOMENG),
                   NVL (TO_CHAR (HM.SYMPTOMENG), '-1'))
       AND NVL (TO_CHAR (HM.TDMFLAG), '-1') =
              NVL (TO_CHAR ( :TDMFLAG), NVL (TO_CHAR (HM.TDMFLAG), '-1'))
       AND NVL (TO_CHAR (HM.UDPOWDERFLAG), '-1') =
              NVL (TO_CHAR ( :UDPOWDERFLAG),
                   NVL (TO_CHAR (HM.UDPOWDERFLAG), '-1'))
       AND NVL (TO_CHAR (HM.MACHINEFLAG), '-1') =
              NVL (TO_CHAR ( :MACHINEFLAG),
                   NVL (TO_CHAR (HM.MACHINEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.AGGREGATECODE), '-1') =
              NVL (TO_CHAR ( :AGGREGATECODE),
                   NVL (TO_CHAR (HM.AGGREGATECODE), '-1'))
       AND NVL (TO_CHAR (HM.AHFSCODE1), '-1') =
              NVL (TO_CHAR ( :AHFSCODE1), NVL (TO_CHAR (HM.AHFSCODE1), '-1'))
       AND NVL (TO_CHAR (HM.AHFSCODE2), '-1') =
              NVL (TO_CHAR ( :AHFSCODE2), NVL (TO_CHAR (HM.AHFSCODE2), '-1'))
       AND NVL (TO_CHAR (HM.AHFSCODE3), '-1') =
              NVL (TO_CHAR ( :AHFSCODE3), NVL (TO_CHAR (HM.AHFSCODE3), '-1'))
       AND NVL (TO_CHAR (HM.AHFSCODE4), '-1') =
              NVL (TO_CHAR ( :AHFSCODE4), NVL (TO_CHAR (HM.AHFSCODE4), '-1'))
       AND NVL (TO_CHAR (HM.AIRDELIVERY), '-1') =
              NVL (TO_CHAR ( :AIRDELIVERY),
                   NVL (TO_CHAR (HM.AIRDELIVERY), '-1'))
       AND NVL (TO_CHAR (HM.ANTIBIOTICSCODE), '-1') =
              NVL (TO_CHAR ( :ANTIBIOTICSCODE),
                   NVL (TO_CHAR (HM.ANTIBIOTICSCODE), '-1'))
       AND NVL (TO_CHAR (HM.APPENDMATERIALFLAG), '-1') =
              NVL (TO_CHAR ( :APPENDMATERIALFLAG),
                   NVL (TO_CHAR (HM.APPENDMATERIALFLAG), '-1'))
       AND NVL (TO_CHAR (HM.ATCCODE1), '-1') =
              NVL (TO_CHAR ( :ATCCODE1), NVL (TO_CHAR (HM.ATCCODE1), '-1'))
       AND NVL (TO_CHAR (HM.ATCCODE2), '-1') =
              NVL (TO_CHAR ( :ATCCODE2), NVL (TO_CHAR (HM.ATCCODE2), '-1'))
       AND NVL (TO_CHAR (HM.ATCCODE3), '-1') =
              NVL (TO_CHAR ( :ATCCODE3), NVL (TO_CHAR (HM.ATCCODE3), '-1'))
       AND NVL (TO_CHAR (HM.ATCCODE4), '-1') =
              NVL (TO_CHAR ( :ATCCODE4), NVL (TO_CHAR (HM.ATCCODE4), '-1'))
       AND NVL (TO_CHAR (HM.ATTACHUNIT), '-1') =
              NVL (TO_CHAR ( :ATTACHUNIT),
                   NVL (TO_CHAR (HM.ATTACHUNIT), '-1'))
       AND NVL (TO_CHAR (HM.BATCHNOFLAG), '-1') =
              NVL (TO_CHAR ( :BATCHNOFLAG),
                   NVL (TO_CHAR (HM.BATCHNOFLAG), '-1'))
       AND NVL (TO_CHAR (HM.BIOLOGICALAGENT), '-1') =
              NVL (TO_CHAR ( :BIOLOGICALAGENT),
                   NVL (TO_CHAR (HM.BIOLOGICALAGENT), '-1'))
       AND NVL (TO_CHAR (HM.BLOODPRODUCT), '-1') =
              NVL (TO_CHAR ( :BLOODPRODUCT),
                   NVL (TO_CHAR (HM.BLOODPRODUCT), '-1'))
       AND NVL (TO_CHAR (HM.BUYORDERFLAG), '-1') =
              NVL (TO_CHAR ( :BUYORDERFLAG),
                   NVL (TO_CHAR (HM.BUYORDERFLAG), '-1'))
       AND NVL (TO_CHAR (HM.CARRYKINDI), '-1') =
              NVL (TO_CHAR ( :CARRYKINDI),
                   NVL (TO_CHAR (HM.CARRYKINDI), '-1'))
       AND NVL (TO_CHAR (HM.CARRYKINDO), '-1') =
              NVL (TO_CHAR ( :CARRYKINDO),
                   NVL (TO_CHAR (HM.CARRYKINDO), '-1'))
       AND NVL (TO_CHAR (HM.CHANGEABLEFLAG), '-1') =
              NVL (TO_CHAR ( :CHANGEABLEFLAG),
                   NVL (TO_CHAR (HM.CHANGEABLEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.CHECKINSWITCH), '-1') =
              NVL (TO_CHAR ( :CHECKINSWITCH),
                   NVL (TO_CHAR (HM.CHECKINSWITCH), '-1'))
       AND NVL (TO_CHAR (HM.COSTEXCLUDECLASS), '-1') =
              NVL (TO_CHAR ( :COSTEXCLUDECLASS),
                   NVL (TO_CHAR (HM.COSTEXCLUDECLASS), '-1'))
       AND NVL (TO_CHAR (HM.CURETYPE), '-1') =
              NVL (TO_CHAR ( :CURETYPE), NVL (TO_CHAR (HM.CURETYPE), '-1'))
       AND NVL (TO_CHAR (HM.DCL), '-1') =
              NVL (TO_CHAR ( :DCL), NVL (TO_CHAR (HM.DCL), '-1'))
       AND NVL (TO_CHAR (HM.DOHLICENSENO), '-1') =
              NVL (TO_CHAR ( :DOHLICENSENO),
                   NVL (TO_CHAR (HM.DOHLICENSENO), '-1'))
       AND NVL (TO_CHAR (HM.DOSE), '-1') =
              NVL (TO_CHAR ( :DOSE), NVL (TO_CHAR (HM.DOSE), '-1'))
       AND NVL (TO_CHAR (HM.DRUGELEMCODE1), '-1') =
              NVL (TO_CHAR ( :DRUGELEMCODE1),
                   NVL (TO_CHAR (HM.DRUGELEMCODE1), '-1'))
       AND NVL (TO_CHAR (HM.DRUGELEMCODE2), '-1') =
              NVL (TO_CHAR ( :DRUGELEMCODE2),
                   NVL (TO_CHAR (HM.DRUGELEMCODE2), '-1'))
       AND NVL (TO_CHAR (HM.DRUGELEMCODE3), '-1') =
              NVL (TO_CHAR ( :DRUGELEMCODE3),
                   NVL (TO_CHAR (HM.DRUGELEMCODE3), '-1'))
       AND NVL (TO_CHAR (HM.DRUGELEMCODE4), '-1') =
              NVL (TO_CHAR ( :DRUGELEMCODE4),
                   NVL (TO_CHAR (HM.DRUGELEMCODE4), '-1'))
       AND NVL (TO_CHAR (HM.DRUGHOSPBEGINDATE), '-1') =
              NVL (TO_CHAR ( :DRUGHOSPBEGINDATE),
                   NVL (TO_CHAR (HM.DRUGHOSPBEGINDATE), '-1'))
       AND NVL (TO_CHAR (HM.DRUGHOSPENDDATE), '-1') =
              NVL (TO_CHAR ( :DRUGHOSPENDDATE),
                   NVL (TO_CHAR (HM.DRUGHOSPENDDATE), '-1'))
       AND NVL (TO_CHAR (HM.DRUGPARENTCODE1), '-1') =
              NVL (TO_CHAR ( :DRUGPARENTCODE1),
                   NVL (TO_CHAR (HM.DRUGPARENTCODE1), '-1'))
       AND NVL (TO_CHAR (HM.DRUGPARENTCODE2), '-1') =
              NVL (TO_CHAR ( :DRUGPARENTCODE2),
                   NVL (TO_CHAR (HM.DRUGPARENTCODE2), '-1'))
       AND NVL (TO_CHAR (HM.DRUGPARENTCODE3), '-1') =
              NVL (TO_CHAR ( :DRUGPARENTCODE3),
                   NVL (TO_CHAR (HM.DRUGPARENTCODE3), '-1'))
       AND NVL (TO_CHAR (HM.DRUGPARENTCODE4), '-1') =
              NVL (TO_CHAR ( :DRUGPARENTCODE4),
                   NVL (TO_CHAR (HM.DRUGPARENTCODE4), '-1'))
       AND NVL (TO_CHAR (HM.EXAMINEDRUGFLAG), '-1') =
              NVL (TO_CHAR ( :EXAMINEDRUGFLAG),
                   NVL (TO_CHAR (HM.EXAMINEDRUGFLAG), '-1'))
       AND NVL (TO_CHAR (HM.EXCEPTIONCODE), '-1') =
              NVL (TO_CHAR ( :EXCEPTIONCODE),
                   NVL (TO_CHAR (HM.EXCEPTIONCODE), '-1'))
       AND NVL (TO_CHAR (HM.EXCEPTIONFLAG), '-1') =
              NVL (TO_CHAR ( :EXCEPTIONFLAG),
                   NVL (TO_CHAR (HM.EXCEPTIONFLAG), '-1'))
       AND NVL (TO_CHAR (HM.EXCLUDEFLAG), '-1') =
              NVL (TO_CHAR ( :EXCLUDEFLAG),
                   NVL (TO_CHAR (HM.EXCLUDEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.EXORDERFLAG), '-1') =
              NVL (TO_CHAR ( :EXORDERFLAG),
                   NVL (TO_CHAR (HM.EXORDERFLAG), '-1'))
       AND NVL (TO_CHAR (HM.FIXDOSEFLAG), '-1') =
              NVL (TO_CHAR ( :FIXDOSEFLAG),
                   NVL (TO_CHAR (HM.FIXDOSEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.FIXPATHNOFLAG), '-1') =
              NVL (TO_CHAR ( :FIXPATHNOFLAG),
                   NVL (TO_CHAR (HM.FIXPATHNOFLAG), '-1'))
       AND NVL (TO_CHAR (HM.FREEZING), '-1') =
              NVL (TO_CHAR ( :FREEZING), NVL (TO_CHAR (HM.FREEZING), '-1'))
       AND NVL (TO_CHAR (HM.FREQNOI), '-1') =
              NVL (TO_CHAR ( :FREQNOI), NVL (TO_CHAR (HM.FREQNOI), '-1'))
       AND NVL (TO_CHAR (HM.FREQNOO), '-1') =
              NVL (TO_CHAR ( :FREQNOO), NVL (TO_CHAR (HM.FREQNOO), '-1'))
       AND NVL (TO_CHAR (HM.GERIATRIC), '-1') =
              NVL (TO_CHAR ( :GERIATRIC), NVL (TO_CHAR (HM.GERIATRIC), '-1'))
       AND NVL (TO_CHAR (HM.HEPATITISCODE), '-1') =
              NVL (TO_CHAR ( :HEPATITISCODE),
                   NVL (TO_CHAR (HM.HEPATITISCODE), '-1'))
       AND NVL (TO_CHAR (HM.HIGHPRICEFLAG), '-1') =
              NVL (TO_CHAR ( :HIGHPRICEFLAG),
                   NVL (TO_CHAR (HM.HIGHPRICEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.HOSPCHARGEID1), '-1') =
              NVL (TO_CHAR ( :HOSPCHARGEID1),
                   NVL (TO_CHAR (HM.HOSPCHARGEID1), '-1'))
       AND NVL (TO_CHAR (HM.HOSPCHARGEID2), '-1') =
              NVL (TO_CHAR ( :HOSPCHARGEID2),
                   NVL (TO_CHAR (HM.HOSPCHARGEID2), '-1'))
       AND NVL (TO_CHAR (HM.HOSPEXAMINEFLAG), '-1') =
              NVL (TO_CHAR ( :HOSPEXAMINEFLAG),
                   NVL (TO_CHAR (HM.HOSPEXAMINEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.HOSPEXAMINEQTYFLAG), '-1') =
              NVL (TO_CHAR ( :HOSPEXAMINEQTYFLAG),
                   NVL (TO_CHAR (HM.HOSPEXAMINEQTYFLAG), '-1'))
       AND NVL (TO_CHAR (HM.INPDISPLAYFLAG), '-1') =
              NVL (TO_CHAR ( :INPDISPLAYFLAG),
                   NVL (TO_CHAR (HM.INPDISPLAYFLAG), '-1'))
       AND NVL (TO_CHAR (HM.INSUCHARGEID1), '-1') =
              NVL (TO_CHAR ( :INSUCHARGEID1),
                   NVL (TO_CHAR (HM.INSUCHARGEID1), '-1'))
       AND NVL (TO_CHAR (HM.INSUCHARGEID2), '-1') =
              NVL (TO_CHAR ( :INSUCHARGEID2),
                   NVL (TO_CHAR (HM.INSUCHARGEID2), '-1'))
       AND NVL (TO_CHAR (HM.INSUOFFERFLAG), '-1') =
              NVL (TO_CHAR ( :INSUOFFERFLAG),
                   NVL (TO_CHAR (HM.INSUOFFERFLAG), '-1'))
       AND NVL (TO_CHAR (HM.ISCURECODE), '-1') =
              NVL (TO_CHAR ( :ISCURECODE),
                   NVL (TO_CHAR (HM.ISCURECODE), '-1'))
       AND NVL (TO_CHAR (HM.LIMITEDQTYI), '-1') =
              NVL (TO_CHAR ( :LIMITEDQTYI),
                   NVL (TO_CHAR (HM.LIMITEDQTYI), '-1'))
       AND NVL (TO_CHAR (HM.LIMITEDQTYO), '-1') =
              NVL (TO_CHAR ( :LIMITEDQTYO),
                   NVL (TO_CHAR (HM.LIMITEDQTYO), '-1'))
       AND NVL (TO_CHAR (HM.LIMITFLAG), '-1') =
              NVL (TO_CHAR ( :LIMITFLAG), NVL (TO_CHAR (HM.LIMITFLAG), '-1'))
       AND NVL (TO_CHAR (HM.LIVERLIMITED), '-1') =
              NVL (TO_CHAR ( :LIVERLIMITED),
                   NVL (TO_CHAR (HM.LIVERLIMITED), '-1'))
       AND NVL (TO_CHAR (HM.MAINCUREITEM), '-1') =
              NVL (TO_CHAR ( :MAINCUREITEM),
                   NVL (TO_CHAR (HM.MAINCUREITEM), '-1'))
       AND NVL (TO_CHAR (HM.MAXDAYSI), '-1') =
              NVL (TO_CHAR ( :MAXDAYSI), NVL (TO_CHAR (HM.MAXDAYSI), '-1'))
       AND NVL (TO_CHAR (HM.MAXDAYSO), '-1') =
              NVL (TO_CHAR ( :MAXDAYSO), NVL (TO_CHAR (HM.MAXDAYSO), '-1'))
       AND NVL (TO_CHAR (HM.MAXQTYI), '-1') =
              NVL (TO_CHAR ( :MAXQTYI), NVL (TO_CHAR (HM.MAXQTYI), '-1'))
       AND NVL (TO_CHAR (HM.MAXQTYO), '-1') =
              NVL (TO_CHAR ( :MAXQTYO), NVL (TO_CHAR (HM.MAXQTYO), '-1'))
       AND NVL (TO_CHAR (HM.MAXTAKETIMES), '-1') =
              NVL (TO_CHAR ( :MAXTAKETIMES),
                   NVL (TO_CHAR (HM.MAXTAKETIMES), '-1'))
       AND NVL (TO_CHAR (HM.MILITARYEXCLUDECLASS), '-1') =
              NVL (TO_CHAR ( :MILITARYEXCLUDECLASS),
                   NVL (TO_CHAR (HM.MILITARYEXCLUDECLASS), '-1'))
       AND NVL (TO_CHAR (HM.NEEDOPDTYPEFLAG), '-1') =
              NVL (TO_CHAR ( :NEEDOPDTYPEFLAG),
                   NVL (TO_CHAR (HM.NEEDOPDTYPEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.NEEDREGIONFLAG), '-1') =
              NVL (TO_CHAR ( :NEEDREGIONFLAG),
                   NVL (TO_CHAR (HM.NEEDREGIONFLAG), '-1'))
       AND NVL (TO_CHAR (HM.OPENDATE), '-1') =
              NVL (TO_CHAR ( :OPENDATE), NVL (TO_CHAR (HM.OPENDATE), '-1'))
       AND NVL (TO_CHAR (HM.OPERATIONFLAG), '-1') =
              NVL (TO_CHAR ( :OPERATIONFLAG),
                   NVL (TO_CHAR (HM.OPERATIONFLAG), '-1'))
       AND NVL (TO_CHAR (HM.ORDERABLEDRUGFORM), '-1') =
              NVL (TO_CHAR ( :ORDERABLEDRUGFORM),
                   NVL (TO_CHAR (HM.ORDERABLEDRUGFORM), '-1'))
       AND NVL (TO_CHAR (HM.ORDERCHINNAME), '-1') =
              NVL (TO_CHAR ( :ORDERCHINNAME),
                   NVL (TO_CHAR (HM.ORDERCHINNAME), '-1'))
       AND NVL (TO_CHAR (HM.ORDERCHINUNIT), '-1') =
              NVL (TO_CHAR ( :ORDERCHINUNIT),
                   NVL (TO_CHAR (HM.ORDERCHINUNIT), '-1'))
       AND NVL (TO_CHAR (HM.ORDERCODESORT), '-1') =
              NVL (TO_CHAR ( :ORDERCODESORT),
                   NVL (TO_CHAR (HM.ORDERCODESORT), '-1'))
       AND NVL (TO_CHAR (HM.ORDERCONDCODE), '-1') =
              NVL (TO_CHAR ( :ORDERCONDCODE),
                   NVL (TO_CHAR (HM.ORDERCONDCODE), '-1'))
       AND NVL (TO_CHAR (HM.ORDERDAYS), '-1') =
              NVL (TO_CHAR ( :ORDERDAYS), NVL (TO_CHAR (HM.ORDERDAYS), '-1'))
       AND NVL (TO_CHAR (HM.ORDERDCFLAG), '-1') =
              NVL (TO_CHAR ( :ORDERDCFLAG),
                   NVL (TO_CHAR (HM.ORDERDCFLAG), '-1'))
       AND NVL (TO_CHAR (HM.ORDEREASYNAME), '-1') =
              NVL (TO_CHAR ( :ORDEREASYNAME),
                   NVL (TO_CHAR (HM.ORDEREASYNAME), '-1'))
       AND NVL (TO_CHAR (HM.ORDERENGNAME), '-1') =
              NVL (TO_CHAR ( :ORDERENGNAME),
                   NVL (TO_CHAR (HM.ORDERENGNAME), '-1'))
       AND NVL (TO_CHAR (HM.ORDERHOSPNAME), '-1') =
              NVL (TO_CHAR ( :ORDERHOSPNAME),
                   NVL (TO_CHAR (HM.ORDERHOSPNAME), '-1'))
       AND NVL (TO_CHAR (HM.ORDERKIND), '-1') =
              NVL (TO_CHAR ( :ORDERKIND), NVL (TO_CHAR (HM.ORDERKIND), '-1'))
       AND NVL (TO_CHAR (HM.ORDERUNIT), '-1') =
              NVL (TO_CHAR ( :ORDERUNIT), NVL (TO_CHAR (HM.ORDERUNIT), '-1'))
       AND NVL (TO_CHAR (HM.ORDERUSETYPE), '-1') =
              NVL (TO_CHAR ( :ORDERUSETYPE),
                   NVL (TO_CHAR (HM.ORDERUSETYPE), '-1'))
       AND NVL (TO_CHAR (HM.PATHNO), '-1') =
              NVL (TO_CHAR ( :PATHNO), NVL (TO_CHAR (HM.PATHNO), '-1'))
       AND NVL (TO_CHAR (HM.PUBLICDRUGFLAG), '-1') =
              NVL (TO_CHAR ( :PUBLICDRUGFLAG),
                   NVL (TO_CHAR (HM.PUBLICDRUGFLAG), '-1'))
       AND NVL (TO_CHAR (HM.RAREDISORDERFLAG), '-1') =
              NVL (TO_CHAR ( :RAREDISORDERFLAG),
                   NVL (TO_CHAR (HM.RAREDISORDERFLAG), '-1'))
       AND NVL (TO_CHAR (HM.RAYPOSITION), '-1') =
              NVL (TO_CHAR ( :RAYPOSITION),
                   NVL (TO_CHAR (HM.RAYPOSITION), '-1'))
       AND NVL (TO_CHAR (HM.RENALLIMITED), '-1') =
              NVL (TO_CHAR ( :RENALLIMITED),
                   NVL (TO_CHAR (HM.RENALLIMITED), '-1'))
       AND NVL (TO_CHAR (HM.REPORTFLAG), '-1') =
              NVL (TO_CHAR ( :REPORTFLAG),
                   NVL (TO_CHAR (HM.REPORTFLAG), '-1'))
       AND NVL (TO_CHAR (HM.RESEARCHDRUGFLAG), '-1') =
              NVL (TO_CHAR ( :RESEARCHDRUGFLAG),
                   NVL (TO_CHAR (HM.RESEARCHDRUGFLAG), '-1'))
       AND NVL (TO_CHAR (HM.RESTRICTCODE), '-1') =
              NVL (TO_CHAR ( :RESTRICTCODE),
                   NVL (TO_CHAR (HM.RESTRICTCODE), '-1'))
       AND NVL (TO_CHAR (HM.RESTRICTTYPE), '-1') =
              NVL (TO_CHAR ( :RESTRICTTYPE),
                   NVL (TO_CHAR (HM.RESTRICTTYPE), '-1'))
       AND NVL (TO_CHAR (HM.RETURNDRUGFLAG), '-1') =
              NVL (TO_CHAR ( :RETURNDRUGFLAG),
                   NVL (TO_CHAR (HM.RETURNDRUGFLAG), '-1'))
       AND NVL (TO_CHAR (HM.RFIDCODE), '-1') =
              NVL (TO_CHAR ( :RFIDCODE), NVL (TO_CHAR (HM.RFIDCODE), '-1'))
       AND NVL (TO_CHAR (HM.SAFETYSYRINGE), '-1') =
              NVL (TO_CHAR ( :SAFETYSYRINGE),
                   NVL (TO_CHAR (HM.SAFETYSYRINGE), '-1'))
       AND NVL (TO_CHAR (HM.SCIENTIFICNAME), '-1') =
              NVL (TO_CHAR ( :SCIENTIFICNAME),
                   NVL (TO_CHAR (HM.SCIENTIFICNAME), '-1'))
       AND NVL (TO_CHAR (HM.SECTIONNO), '-1') =
              NVL (TO_CHAR ( :SECTIONNO), NVL (TO_CHAR (HM.SECTIONNO), '-1'))
       AND NVL (TO_CHAR (HM.SENDUNITFLAG), '-1') =
              NVL (TO_CHAR ( :SENDUNITFLAG),
                   NVL (TO_CHAR (HM.SENDUNITFLAG), '-1'))
       AND NVL (TO_CHAR (HM.SIGNFLAG), '-1') =
              NVL (TO_CHAR ( :SIGNFLAG), NVL (TO_CHAR (HM.SIGNFLAG), '-1'))
       AND NVL (TO_CHAR (HM.SINGLEITEMFLAG), '-1') =
              NVL (TO_CHAR ( :SINGLEITEMFLAG),
                   NVL (TO_CHAR (HM.SINGLEITEMFLAG), '-1'))
       AND NVL (TO_CHAR (HM.SOONCULLFLAG), '-1') =
              NVL (TO_CHAR ( :SOONCULLFLAG),
                   NVL (TO_CHAR (HM.SOONCULLFLAG), '-1'))
       AND NVL (TO_CHAR (HM.SPECIALORDERKIND), '-1') =
              NVL (TO_CHAR ( :SPECIALORDERKIND),
                   NVL (TO_CHAR (HM.SPECIALORDERKIND), '-1'))
       AND NVL (TO_CHAR (HM.STOCKUNIT), '-1') =
              NVL (TO_CHAR ( :STOCKUNIT), NVL (TO_CHAR (HM.STOCKUNIT), '-1'))
       AND NVL (TO_CHAR (HM.SUBSTITUTE1), '-1') =
              NVL (TO_CHAR ( :SUBSTITUTE1),
                   NVL (TO_CHAR (HM.SUBSTITUTE1), '-1'))
       AND NVL (TO_CHAR (HM.SUBSTITUTE2), '-1') =
              NVL (TO_CHAR ( :SUBSTITUTE2),
                   NVL (TO_CHAR (HM.SUBSTITUTE2), '-1'))
       AND NVL (TO_CHAR (HM.SUBSTITUTE3), '-1') =
              NVL (TO_CHAR ( :SUBSTITUTE3),
                   NVL (TO_CHAR (HM.SUBSTITUTE3), '-1'))
       AND NVL (TO_CHAR (HM.SUBSTITUTE4), '-1') =
              NVL (TO_CHAR ( :SUBSTITUTE4),
                   NVL (TO_CHAR (HM.SUBSTITUTE4), '-1'))
       AND NVL (TO_CHAR (HM.SUBSTITUTE5), '-1') =
              NVL (TO_CHAR ( :SUBSTITUTE5),
                   NVL (TO_CHAR (HM.SUBSTITUTE5), '-1'))
       AND NVL (TO_CHAR (HM.SUBSTITUTE6), '-1') =
              NVL (TO_CHAR ( :SUBSTITUTE6),
                   NVL (TO_CHAR (HM.SUBSTITUTE6), '-1'))
       AND NVL (TO_CHAR (HM.SUBSTITUTE7), '-1') =
              NVL (TO_CHAR ( :SUBSTITUTE7),
                   NVL (TO_CHAR (HM.SUBSTITUTE7), '-1'))
       AND NVL (TO_CHAR (HM.SUBSTITUTE8), '-1') =
              NVL (TO_CHAR ( :SUBSTITUTE8),
                   NVL (TO_CHAR (HM.SUBSTITUTE8), '-1'))
       AND NVL (TO_CHAR (HM.TAKEKIND), '-1') =
              NVL (TO_CHAR ( :TAKEKIND), NVL (TO_CHAR (HM.TAKEKIND), '-1'))
       AND NVL (TO_CHAR (HM.TRANSCOMPUTEFLAG), '-1') =
              NVL (TO_CHAR ( :TRANSCOMPUTEFLAG),
                   NVL (TO_CHAR (HM.TRANSCOMPUTEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.UDSERVICEFLAG), '-1') =
              NVL (TO_CHAR ( :UDSERVICEFLAG),
                   NVL (TO_CHAR (HM.UDSERVICEFLAG), '-1'))
       AND NVL (TO_CHAR (HM.VACCINE), '-1') =
              NVL (TO_CHAR ( :VACCINE), NVL (TO_CHAR (HM.VACCINE), '-1'))
       AND NVL (TO_CHAR (HM.VACCINECLASS), '-1') =
              NVL (TO_CHAR ( :VACCINECLASS),
                   NVL (TO_CHAR (HM.VACCINECLASS), '-1'))
       AND NVL (TO_CHAR (HM.VALIDDAYSI), '-1') =
              NVL (TO_CHAR ( :VALIDDAYSI),
                   NVL (TO_CHAR (HM.VALIDDAYSI), '-1'))
       AND NVL (TO_CHAR (HM.VALIDDAYSO), '-1') =
              NVL (TO_CHAR ( :VALIDDAYSO),
                   NVL (TO_CHAR (HM.VALIDDAYSO), '-1'))
       AND NVL (TO_CHAR (HM.ZEROCASESTATE), '-1') =
              NVL (TO_CHAR ( :ZEROCASESTATE),
                   NVL (TO_CHAR (HM.ZEROCASESTATE), '-1'))
       AND NVL (TO_CHAR (HT.DRUGEXTERIOR), '-1') =
              NVL (TO_CHAR ( :DRUGEXTERIOR),
                   NVL (TO_CHAR (HT.DRUGEXTERIOR), '-1'))
       AND NVL (TO_CHAR (HT.DRUGENGEXTERIOR), '-1') =
              NVL (TO_CHAR ( :DRUGENGEXTERIOR),
                   NVL (TO_CHAR (HT.DRUGENGEXTERIOR), '-1'))
       AND NVL (TO_CHAR (HT.CHINSIDEEFFECT), '-1') =
              NVL (TO_CHAR ( :CHINSIDEEFFECT),
                   NVL (TO_CHAR (HT.CHINSIDEEFFECT), '-1'))
       AND NVL (TO_CHAR (HT.ENGSIDEEFFECT), '-1') =
              NVL (TO_CHAR ( :ENGSIDEEFFECT),
                   NVL (TO_CHAR (HT.ENGSIDEEFFECT), '-1'))
       AND NVL (TO_CHAR (HT.CHINATTENTION), '-1') =
              NVL (TO_CHAR ( :CHINATTENTION),
                   NVL (TO_CHAR (HT.CHINATTENTION), '-1'))
       AND NVL (TO_CHAR (HT.ENGATTENTION), '-1') =
              NVL (TO_CHAR ( :ENGATTENTION),
                   NVL (TO_CHAR (HT.ENGATTENTION), '-1'))
       AND NVL (TO_CHAR (HT.DOHSYMPTOM), '-1') =
              NVL (TO_CHAR ( :DOHSYMPTOM),
                   NVL (TO_CHAR (HT.DOHSYMPTOM), '-1'))
       AND NVL (TO_CHAR (HT.FDASYMPTOM), '-1') =
              NVL (TO_CHAR ( :FDASYMPTOM),
                   NVL (TO_CHAR (HT.FDASYMPTOM), '-1'))
       AND NVL (TO_CHAR (HT.DRUGMEMO), '-1') =
              NVL (TO_CHAR ( :DRUGMEMO), NVL (TO_CHAR (HT.DRUGMEMO), '-1'))
       AND NVL (TO_CHAR (HT.SUCKLESECURITY), '-1') =
              NVL (TO_CHAR ( :SUCKLESECURITY),
                   NVL (TO_CHAR (HT.SUCKLESECURITY), '-1'))
       AND NVL (TO_CHAR (HT.PREGNANTGRADE), '-1') =
              NVL (TO_CHAR ( :PREGNANTGRADE),
                   NVL (TO_CHAR (HT.PREGNANTGRADE), '-1'))
       AND NVL (TO_CHAR (HT.DRUGPICTURELINK), '-1') =
              NVL (TO_CHAR ( :DRUGPICTURELINK),
                   NVL (TO_CHAR (HT.DRUGPICTURELINK), '-1'))
       AND NVL (TO_CHAR (HT.DRUGLEAFLETLINK), '-1') =
              NVL (TO_CHAR ( :DRUGLEAFLETLINK),
                   NVL (TO_CHAR (HT.DRUGLEAFLETLINK), '-1'))
       AND NVL (TO_CHAR (HT.MAXCURECONSISTENCY), '-1') =
              NVL (TO_CHAR ( :MAXCURECONSISTENCY),
                   NVL (TO_CHAR (HT.MAXCURECONSISTENCY), '-1'))
       AND NVL (TO_CHAR (HT.MINCURECONSISTENCY), '-1') =
              NVL (TO_CHAR ( :MINCURECONSISTENCY),
                   NVL (TO_CHAR (HT.MINCURECONSISTENCY), '-1'))
       AND NVL (TO_CHAR (HT.PEARBEGIN), '-1') =
              NVL (TO_CHAR ( :PEARBEGIN), NVL (TO_CHAR (HT.PEARBEGIN), '-1'))
       AND NVL (TO_CHAR (HT.PEAREND), '-1') =
              NVL (TO_CHAR ( :PEAREND), NVL (TO_CHAR (HT.PEAREND), '-1'))
       AND NVL (TO_CHAR (HT.TROUGHBEGIN), '-1') =
              NVL (TO_CHAR ( :TROUGHBEGIN),
                   NVL (TO_CHAR (HT.TROUGHBEGIN), '-1'))
       AND NVL (TO_CHAR (HT.TROUGHEND), '-1') =
              NVL (TO_CHAR ( :TROUGHEND), NVL (TO_CHAR (HT.TROUGHEND), '-1'))
       AND NVL (TO_CHAR (HT.DANGERBEGIN), '-1') =
              NVL (TO_CHAR ( :DANGERBEGIN),
                   NVL (TO_CHAR (HT.DANGERBEGIN), '-1'))
       AND NVL (TO_CHAR (HT.DANGEREND), '-1') =
              NVL (TO_CHAR ( :DANGEREND), NVL (TO_CHAR (HT.DANGEREND), '-1'))
       AND NVL (TO_CHAR (HT.TDMMEMO1), '-1') =
              NVL (TO_CHAR ( :TDMMEMO1), NVL (TO_CHAR (HT.TDMMEMO1), '-1'))
       AND NVL (TO_CHAR (HT.TDMMEMO2), '-1') =
              NVL (TO_CHAR ( :TDMMEMO2), NVL (TO_CHAR (HT.TDMMEMO2), '-1'))
       AND NVL (TO_CHAR (HT.TDMMEMO3), '-1') =
              NVL (TO_CHAR ( :TDMMEMO3), NVL (TO_CHAR (HT.TDMMEMO3), '-1'))
       AND NVL (TO_CHAR (HT.APPLYTRANSQTY), '-1') =
              NVL (TO_CHAR ( :APPLYTRANSQTY),
                   NVL (TO_CHAR (HT.APPLYTRANSQTY), '-1'))
       AND NVL (TO_CHAR (HT.APPLYUNIT), '-1') =
              NVL (TO_CHAR ( :APPLYUNIT), NVL (TO_CHAR (HT.APPLYUNIT), '-1'))
       AND NVL (TO_CHAR (HT.ARMYORDERCODE), '-1') =
              NVL (TO_CHAR ( :ARMYORDERCODE),
                   NVL (TO_CHAR (HT.ARMYORDERCODE), '-1'))
       AND NVL (TO_CHAR (HT.CLASSIFIEDARMYNO), '-1') =
              NVL (TO_CHAR ( :CLASSIFIEDARMYNO),
                   NVL (TO_CHAR (HT.CLASSIFIEDARMYNO), '-1'))
       AND NVL (TO_CHAR (HT.COMMITTEECODE), '-1') =
              NVL (TO_CHAR ( :COMMITTEECODE),
                   NVL (TO_CHAR (HT.COMMITTEECODE), '-1'))
       AND NVL (TO_CHAR (HT.COMMITTEEMEMO), '-1') =
              NVL (TO_CHAR ( :COMMITTEEMEMO),
                   NVL (TO_CHAR (HT.COMMITTEEMEMO), '-1'))
       AND NVL (TO_CHAR (HT.COMPONENTNUNIT), '-1') =
              NVL (TO_CHAR ( :COMPONENTNUNIT),
                   NVL (TO_CHAR (HT.COMPONENTNUNIT), '-1'))
       AND NVL (TO_CHAR (HT.COMPONENTNUNIT2), '-1') =
              NVL (TO_CHAR ( :COMPONENTNUNIT2),
                   NVL (TO_CHAR (HT.COMPONENTNUNIT2), '-1'))
       AND NVL (TO_CHAR (HT.COMPONENTNUNIT3), '-1') =
              NVL (TO_CHAR ( :COMPONENTNUNIT3),
                   NVL (TO_CHAR (HT.COMPONENTNUNIT3), '-1'))
       AND NVL (TO_CHAR (HT.COMPONENTNUNIT4), '-1') =
              NVL (TO_CHAR ( :COMPONENTNUNIT4),
                   NVL (TO_CHAR (HT.COMPONENTNUNIT4), '-1'))
       AND NVL (TO_CHAR (HT.CONTRACTEFFECTIVEDATE), '-1') =
              NVL (TO_CHAR ( :CONTRACTEFFECTIVEDATE),
                   NVL (TO_CHAR (HT.CONTRACTEFFECTIVEDATE), '-1'))
       AND NVL (TO_CHAR (HT.DRUGAPPLYTYPE), '-1') =
              NVL (TO_CHAR ( :DRUGAPPLYTYPE),
                   NVL (TO_CHAR (HT.DRUGAPPLYTYPE), '-1'))
       AND NVL (TO_CHAR (HT.DRUGCLASS), '-1') =
              NVL (TO_CHAR ( :DRUGCLASS), NVL (TO_CHAR (HT.DRUGCLASS), '-1'))
       AND NVL (TO_CHAR (HT.DRUGCLASSIFY), '-1') =
              NVL (TO_CHAR ( :DRUGCLASSIFY),
                   NVL (TO_CHAR (HT.DRUGCLASSIFY), '-1'))
       AND NVL (TO_CHAR (HT.DRUGFORM), '-1') =
              NVL (TO_CHAR ( :DRUGFORM), NVL (TO_CHAR (HT.DRUGFORM), '-1'))
       AND NVL (TO_CHAR (HT.DRUGPACKAGE), '-1') =
              NVL (TO_CHAR ( :DRUGPACKAGE),
                   NVL (TO_CHAR (HT.DRUGPACKAGE), '-1'))
       AND NVL (TO_CHAR (HT.DRUGTOTALAMOUNT), '-1') =
              NVL (TO_CHAR ( :DRUGTOTALAMOUNT),
                   NVL (TO_CHAR (HT.DRUGTOTALAMOUNT), '-1'))
       AND NVL (TO_CHAR (HT.DRUGTOTALAMOUNTUNIT), '-1') =
              NVL (TO_CHAR ( :DRUGTOTALAMOUNTUNIT),
                   NVL (TO_CHAR (HT.DRUGTOTALAMOUNTUNIT), '-1'))
       AND NVL (TO_CHAR (HT.GROUPARMYNO), '-1') =
              NVL (TO_CHAR ( :GROUPARMYNO),
                   NVL (TO_CHAR (HT.GROUPARMYNO), '-1'))
       AND NVL (TO_CHAR (HT.INVENTORYFLAG), '-1') =
              NVL (TO_CHAR ( :INVENTORYFLAG),
                   NVL (TO_CHAR (HT.INVENTORYFLAG), '-1'))
       AND NVL (TO_CHAR (HT.ITEMARMYNO), '-1') =
              NVL (TO_CHAR ( :ITEMARMYNO),
                   NVL (TO_CHAR (HT.ITEMARMYNO), '-1'))
       AND NVL (TO_CHAR (HT.MANUFACTURER), '-1') =
              NVL (TO_CHAR ( :MANUFACTURER),
                   NVL (TO_CHAR (HT.MANUFACTURER), '-1'))
       AND NVL (TO_CHAR (HT.MULTIPRESCRIPTIONCODE), '-1') =
              NVL (TO_CHAR ( :MULTIPRESCRIPTIONCODE),
                   NVL (TO_CHAR (HT.MULTIPRESCRIPTIONCODE), '-1'))
       AND NVL (TO_CHAR (HT.PARENTCODE), '-1') =
              NVL (TO_CHAR ( :PARENTCODE),
                   NVL (TO_CHAR (HT.PARENTCODE), '-1'))
       AND NVL (TO_CHAR (HT.PARENTORDERCODE), '-1') =
              NVL (TO_CHAR ( :PARENTORDERCODE),
                   NVL (TO_CHAR (HT.PARENTORDERCODE), '-1'))
       AND NVL (TO_CHAR (HT.PURCHASECASETYPE), '-1') =
              NVL (TO_CHAR ( :PURCHASECASETYPE),
                   NVL (TO_CHAR (HT.PURCHASECASETYPE), '-1'))
       AND NVL (TO_CHAR (HT.PURCHASETRANSQTY), '-1') =
              NVL (TO_CHAR ( :PURCHASETRANSQTY),
                   NVL (TO_CHAR (HT.PURCHASETRANSQTY), '-1'))
       AND NVL (TO_CHAR (HT.PURCHASEUNIT), '-1') =
              NVL (TO_CHAR ( :PURCHASEUNIT),
                   NVL (TO_CHAR (HT.PURCHASEUNIT), '-1'))
       AND NVL (TO_CHAR (HT.SONTRANSQTY), '-1') =
              NVL (TO_CHAR ( :SONTRANSQTY),
                   NVL (TO_CHAR (HT.SONTRANSQTY), '-1'))
       AND NVL (TO_CHAR (HT.SPECNUNIT), '-1') =
              NVL (TO_CHAR ( :SPECNUNIT), NVL (TO_CHAR (HT.SPECNUNIT), '-1'))
       AND NVL (TO_CHAR (HT.STOCKSOURCECODE), '-1') =
              NVL (TO_CHAR ( :STOCKSOURCECODE),
                   NVL (TO_CHAR (HT.STOCKSOURCECODE), '-1'))
       AND NVL (TO_CHAR (HT.STOCKUSECODE), '-1') =
              NVL (TO_CHAR ( :STOCKUSECODE),
                   NVL (TO_CHAR (HT.STOCKUSECODE), '-1'))
       AND NVL (TO_CHAR (HT.WARN), '-1') =
              NVL (TO_CHAR ( :WARN), NVL (TO_CHAR (HT.WARN), '-1'))
       AND NVL (TO_CHAR (HT.YEARARMYNO), '-1') =
              NVL (TO_CHAR ( :YEARARMYNO),
                   NVL (TO_CHAR (HT.YEARARMYNO), '-1'))";

            return !(DBWork.Connection.ExecuteScalar(sql, AB0038, DBWork.Transaction) == null);
        }

        //判斷型態是否為數字
        public bool Check_NUMBER(string COL_NAME)
        {
            string sql = @"SELECT 1
                           FROM V_TAB_COL_PGM
                           WHERE     TBL_NAME IN ('HIS_BASORDM', 'HIS_BASORDD', 'HIS_STKDMIT')
                           AND UPPER (COL_NAME) = UPPER (:COL_NAME)
                           AND DATA_TYPE = 'NUMBER'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { COL_NAME = COL_NAME }, DBWork.Transaction) == null);
        }

        //判斷是否可為空值
        public bool Check_NULL(string COL_NAME)
        {
            string sql = @"SELECT 1
                           FROM V_TAB_COL_PGM
                           WHERE     TBL_NAME IN ('HIS_BASORDM', 'HIS_BASORDD', 'HIS_STKDMIT')
                           AND UPPER (COL_NAME) = UPPER (:COL_NAME)
                           AND IS_NULL = 'NOT NULL'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { COL_NAME = COL_NAME }, DBWork.Transaction) == null);
        }

        //判斷是否有代碼
        public bool Check_CODE(string DATA_NAME)
        {
            string sql = @"SELECT 1
                           FROM PARAM_D
                           WHERE  GRP_CODE IN ('HIS_BASORDM', 'HIS_BASORDD', 'HIS_STKDMIT')
                           AND UPPER (DATA_NAME) = UPPER ( :DATA_NAME)";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DATA_NAME = DATA_NAME }, DBWork.Transaction) == null);
        }

        //判斷代碼是否符合
        public bool Check_CODE_2(string DATA_NAME, string DATA_VALUE)
        {
            string sql = @"SELECT 1
                           FROM PARAM_D
                           WHERE     GRP_CODE IN ('HIS_BASORDM', 'HIS_BASORDD', 'HIS_STKDMIT')
                           AND UPPER (DATA_NAME) = UPPER ( :DATA_NAME)
                           AND DATA_VALUE = :DATA_VALUE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DATA_NAME = DATA_NAME, DATA_VALUE = DATA_VALUE }, DBWork.Transaction) == null);
        }

        //取得字串欄位設定的長度
        public string GetVARCHAR_LENGH(string COL_NAME)
        {
            var sql = @"SELECT CHAR_LENGTH
                        FROM V_TAB_COL_PGM
                        WHERE   TBL_NAME IN ('HIS_BASORDM', 'HIS_BASORDD', 'HIS_STKDMIT')
                        AND COL_NAME = :COL_NAME
                        AND DATA_TYPE IN ('VARCHAR2', 'CHAR')";

            return DBWork.Connection.QueryFirst<string>(sql, new { COL_NAME = COL_NAME }, DBWork.Transaction);
        }

        //儲存 HIS_BASORDM
        public int SetCommit_HIS_BASORDM(AB0038 AB0038)
        {
            var sql = @"UPDATE HIS_BASORDM
   SET MAXQTYPERTIME = NVL ( :MAXQTYPERTIME, MAXQTYPERTIME),
       MAXQTYPERDAY = NVL ( :MAXQTYPERDAY, MAXQTYPERDAY),
       ONLYROUNDFLAG = NVL ( :ONLYROUNDFLAG, ONLYROUNDFLAG),
       UNABLEPOWDERFLAG = NVL ( :UNABLEPOWDERFLAG, UNABLEPOWDERFLAG),
       COLDSTORAGEFLAG = NVL ( :COLDSTORAGEFLAG, COLDSTORAGEFLAG),
       LIGHTAVOIDFLAG = NVL ( :LIGHTAVOIDFLAG, LIGHTAVOIDFLAG),
       WEIGHTTYPE = NVL ( :WEIGHTTYPE, WEIGHTTYPE),
       WEIGHTUNITLIMIT = NVL ( :WEIGHTUNITLIMIT, WEIGHTUNITLIMIT),
       DANGERDRUGFLAG = NVL ( :DANGERDRUGFLAG, DANGERDRUGFLAG),
       DANGERDRUGMEMO = NVL ( :DANGERDRUGMEMO, DANGERDRUGMEMO),
       SYMPTOMCHIN = NVL ( :SYMPTOMCHIN, SYMPTOMCHIN),
       SYMPTOMENG = NVL ( :SYMPTOMENG, SYMPTOMENG),
       TDMFLAG = NVL ( :TDMFLAG, TDMFLAG),
       UDPOWDERFLAG = NVL ( :UDPOWDERFLAG, UDPOWDERFLAG),
       MACHINEFLAG = NVL ( :MACHINEFLAG, MACHINEFLAG),
       AGGREGATECODE = NVL ( :AGGREGATECODE, AGGREGATECODE),
       AHFSCODE1 = NVL ( :AHFSCODE1, AHFSCODE1),
       AHFSCODE2 = NVL ( :AHFSCODE2, AHFSCODE2),
       AHFSCODE3 = NVL ( :AHFSCODE3, AHFSCODE3),
       AHFSCODE4 = NVL ( :AHFSCODE4, AHFSCODE4),
       AIRDELIVERY = NVL ( :AIRDELIVERY, AIRDELIVERY),
       ANTIBIOTICSCODE = NVL ( :ANTIBIOTICSCODE, ANTIBIOTICSCODE),
       APPENDMATERIALFLAG = NVL ( :APPENDMATERIALFLAG, APPENDMATERIALFLAG),
       ATCCODE1 = NVL ( :ATCCODE1, ATCCODE1),
       ATCCODE2 = NVL ( :ATCCODE2, ATCCODE2),
       ATCCODE3 = NVL ( :ATCCODE3, ATCCODE3),
       ATCCODE4 = NVL ( :ATCCODE4, ATCCODE4),
       ATTACHUNIT = NVL ( :ATTACHUNIT, ATTACHUNIT),
       BATCHNOFLAG = NVL ( :BATCHNOFLAG, BATCHNOFLAG),
       BIOLOGICALAGENT = NVL ( :BIOLOGICALAGENT, BIOLOGICALAGENT),
       BLOODPRODUCT = NVL ( :BLOODPRODUCT, BLOODPRODUCT),
       BUYORDERFLAG = NVL ( :BUYORDERFLAG, BUYORDERFLAG),
       CARRYKINDI = NVL ( :CARRYKINDI, CARRYKINDI),
       CARRYKINDO = NVL ( :CARRYKINDO, CARRYKINDO),
       CHANGEABLEFLAG = NVL ( :CHANGEABLEFLAG, CHANGEABLEFLAG),
       CHECKINSWITCH = NVL ( :CHECKINSWITCH, CHECKINSWITCH),
       COSTEXCLUDECLASS = NVL ( :COSTEXCLUDECLASS, COSTEXCLUDECLASS),
       CURETYPE = NVL ( :CURETYPE, CURETYPE),
       DCL = NVL ( :DCL, DCL),
       DOHLICENSENO = NVL ( :DOHLICENSENO, DOHLICENSENO),
       DOSE = NVL ( :DOSE, DOSE),
       DRUGELEMCODE1 = NVL ( :DRUGELEMCODE1, DRUGELEMCODE1),
       DRUGELEMCODE2 = NVL ( :DRUGELEMCODE2, DRUGELEMCODE2),
       DRUGELEMCODE3 = NVL ( :DRUGELEMCODE3, DRUGELEMCODE3),
       DRUGELEMCODE4 = NVL ( :DRUGELEMCODE4, DRUGELEMCODE4),
       DRUGHOSPBEGINDATE = NVL ( :DRUGHOSPBEGINDATE, DRUGHOSPBEGINDATE),
       DRUGHOSPENDDATE = NVL ( :DRUGHOSPENDDATE, DRUGHOSPENDDATE),
       DRUGPARENTCODE1 = NVL ( :DRUGPARENTCODE1, DRUGPARENTCODE1),
       DRUGPARENTCODE2 = NVL ( :DRUGPARENTCODE2, DRUGPARENTCODE2),
       DRUGPARENTCODE3 = NVL ( :DRUGPARENTCODE3, DRUGPARENTCODE3),
       DRUGPARENTCODE4 = NVL ( :DRUGPARENTCODE4, DRUGPARENTCODE4),
       EXAMINEDRUGFLAG = NVL ( :EXAMINEDRUGFLAG, EXAMINEDRUGFLAG),
       EXCEPTIONCODE = NVL ( :EXCEPTIONCODE, EXCEPTIONCODE),
       EXCEPTIONFLAG = NVL ( :EXCEPTIONFLAG, EXCEPTIONFLAG),
       EXCLUDEFLAG = NVL ( :EXCLUDEFLAG, EXCLUDEFLAG),
       EXORDERFLAG = NVL ( :EXORDERFLAG, EXORDERFLAG),
       FIXDOSEFLAG = NVL ( :FIXDOSEFLAG, FIXDOSEFLAG),
       FIXPATHNOFLAG = NVL ( :FIXPATHNOFLAG, FIXPATHNOFLAG),
       FREEZING = NVL ( :FREEZING, FREEZING),
       FREQNOI = NVL ( :FREQNOI, FREQNOI),
       FREQNOO = NVL ( :FREQNOO, FREQNOO),
       GERIATRIC = NVL ( :GERIATRIC, GERIATRIC),
       HEPATITISCODE = NVL ( :HEPATITISCODE, HEPATITISCODE),
       HIGHPRICEFLAG = NVL ( :HIGHPRICEFLAG, HIGHPRICEFLAG),
       HOSPCHARGEID1 = NVL ( :HOSPCHARGEID1, HOSPCHARGEID1),
       HOSPCHARGEID2 = NVL ( :HOSPCHARGEID2, HOSPCHARGEID2),
       HOSPEXAMINEFLAG = NVL ( :HOSPEXAMINEFLAG, HOSPEXAMINEFLAG),
       HOSPEXAMINEQTYFLAG = NVL ( :HOSPEXAMINEQTYFLAG, HOSPEXAMINEQTYFLAG),
       INPDISPLAYFLAG = NVL ( :INPDISPLAYFLAG, INPDISPLAYFLAG),
       INSUCHARGEID1 = NVL ( :INSUCHARGEID1, INSUCHARGEID1),
       INSUCHARGEID2 = NVL ( :INSUCHARGEID2, INSUCHARGEID2),
       INSUOFFERFLAG = NVL ( :INSUOFFERFLAG, INSUOFFERFLAG),
       ISCURECODE = NVL ( :ISCURECODE, ISCURECODE),
       LIMITEDQTYI = NVL ( :LIMITEDQTYI, LIMITEDQTYI),
       LIMITEDQTYO = NVL ( :LIMITEDQTYO, LIMITEDQTYO),
       LIMITFLAG = NVL ( :LIMITFLAG, LIMITFLAG),
       LIVERLIMITED = NVL ( :LIVERLIMITED, LIVERLIMITED),
       MAINCUREITEM = NVL ( :MAINCUREITEM, MAINCUREITEM),
       MAXDAYSI = NVL ( :MAXDAYSI, MAXDAYSI),
       MAXDAYSO = NVL ( :MAXDAYSO, MAXDAYSO),
       MAXQTYI = NVL ( :MAXQTYI, MAXQTYI),
       MAXQTYO = NVL ( :MAXQTYO, MAXQTYO),
       MAXTAKETIMES = NVL ( :MAXTAKETIMES, MAXTAKETIMES),
       MILITARYEXCLUDECLASS =
          NVL ( :MILITARYEXCLUDECLASS, MILITARYEXCLUDECLASS),
       NEEDOPDTYPEFLAG = NVL ( :NEEDOPDTYPEFLAG, NEEDOPDTYPEFLAG),
       NEEDREGIONFLAG = NVL ( :NEEDREGIONFLAG, NEEDREGIONFLAG),
       OPENDATE = NVL ( :OPENDATE, OPENDATE),
       OPERATIONFLAG = NVL ( :OPERATIONFLAG, OPERATIONFLAG),
       ORDERABLEDRUGFORM = NVL ( :ORDERABLEDRUGFORM, ORDERABLEDRUGFORM),
       ORDERCHINNAME = NVL ( :ORDERCHINNAME, ORDERCHINNAME),
       ORDERCHINUNIT = NVL ( :ORDERCHINUNIT, ORDERCHINUNIT),
       ORDERCODESORT = NVL ( :ORDERCODESORT, ORDERCODESORT),
       ORDERCONDCODE = NVL ( :ORDERCONDCODE, ORDERCONDCODE),
       ORDERDAYS = NVL ( :ORDERDAYS, ORDERDAYS),
       ORDERDCFLAG = NVL ( :ORDERDCFLAG, ORDERDCFLAG),
       ORDEREASYNAME = NVL ( :ORDEREASYNAME, ORDEREASYNAME),
       ORDERENGNAME = NVL ( :ORDERENGNAME, ORDERENGNAME),
       ORDERHOSPNAME = NVL ( :ORDERHOSPNAME, ORDERHOSPNAME),
       ORDERKIND = NVL ( :ORDERKIND, ORDERKIND),
       ORDERUNIT = NVL ( :ORDERUNIT, ORDERUNIT),
       ORDERUSETYPE = NVL ( :ORDERUSETYPE, ORDERUSETYPE),
       PATHNO = NVL ( :PATHNO, PATHNO),
       PUBLICDRUGFLAG = NVL ( :PUBLICDRUGFLAG, PUBLICDRUGFLAG),
       RAREDISORDERFLAG = NVL ( :RAREDISORDERFLAG, RAREDISORDERFLAG),
       RAYPOSITION = NVL ( :RAYPOSITION, RAYPOSITION),
       RENALLIMITED = NVL ( :RENALLIMITED, RENALLIMITED),
       REPORTFLAG = NVL ( :REPORTFLAG, REPORTFLAG),
       RESEARCHDRUGFLAG = NVL ( :RESEARCHDRUGFLAG, RESEARCHDRUGFLAG),
       RESTRICTCODE = NVL ( :RESTRICTCODE, RESTRICTCODE),
       RESTRICTTYPE = NVL ( :RESTRICTTYPE, RESTRICTTYPE),
       RETURNDRUGFLAG = NVL ( :RETURNDRUGFLAG, RETURNDRUGFLAG),
       RFIDCODE = NVL ( :RFIDCODE, RFIDCODE),
       SAFETYSYRINGE = NVL ( :SAFETYSYRINGE, SAFETYSYRINGE),
       SCIENTIFICNAME = NVL ( :SCIENTIFICNAME, SCIENTIFICNAME),
       SECTIONNO = NVL ( :SECTIONNO, SECTIONNO),
       SENDUNITFLAG = NVL ( :SENDUNITFLAG, SENDUNITFLAG),
       SIGNFLAG = NVL ( :SIGNFLAG, SIGNFLAG),
       SINGLEITEMFLAG = NVL ( :SINGLEITEMFLAG, SINGLEITEMFLAG),
       SOONCULLFLAG = NVL ( :SOONCULLFLAG, SOONCULLFLAG),
       SPECIALORDERKIND = NVL ( :SPECIALORDERKIND, SPECIALORDERKIND),
       STOCKUNIT = NVL ( :STOCKUNIT, STOCKUNIT),
       SUBSTITUTE1 = NVL ( :SUBSTITUTE1, SUBSTITUTE1),
       SUBSTITUTE2 = NVL ( :SUBSTITUTE2, SUBSTITUTE2),
       SUBSTITUTE3 = NVL ( :SUBSTITUTE3, SUBSTITUTE3),
       SUBSTITUTE4 = NVL ( :SUBSTITUTE4, SUBSTITUTE4),
       SUBSTITUTE5 = NVL ( :SUBSTITUTE5, SUBSTITUTE5),
       SUBSTITUTE6 = NVL ( :SUBSTITUTE6, SUBSTITUTE6),
       SUBSTITUTE7 = NVL ( :SUBSTITUTE7, SUBSTITUTE7),
       SUBSTITUTE8 = NVL ( :SUBSTITUTE8, SUBSTITUTE8),
       TAKEKIND = NVL ( :TAKEKIND, TAKEKIND),
       TRANSCOMPUTEFLAG = NVL ( :TRANSCOMPUTEFLAG, TRANSCOMPUTEFLAG),
       UDSERVICEFLAG = NVL ( :UDSERVICEFLAG, UDSERVICEFLAG),
       VACCINE = NVL ( :VACCINE, VACCINE),
       VACCINECLASS = NVL ( :VACCINECLASS, VACCINECLASS),
       VALIDDAYSI = NVL ( :VALIDDAYSI, VALIDDAYSI),
       VALIDDAYSO = NVL ( :VALIDDAYSO, VALIDDAYSO),
       ZEROCASESTATE = NVL ( :ZEROCASESTATE, ZEROCASESTATE)
 WHERE ORDERCODE = :ORDERCODE";

            return DBWork.Connection.Execute(sql, AB0038, DBWork.Transaction);
        }

        //儲存 HIS_BASORDD
        public int SetCommit_HIS_BASORDD(AB0038 AB0038)
        {
            var sql = @"UPDATE HIS_BASORDD
   SET AGENTNAME = NVL ( :AGENTNAME, AGENTNAME),
       ARMYINSUAMOUNT = NVL ( :ARMYINSUAMOUNT, ARMYINSUAMOUNT),
       ARMYINSUORDERCODE = NVL ( :ARMYINSUORDERCODE, ARMYINSUORDERCODE),
       ATTACHTRANSQTYO = NVL ( :ATTACHTRANSQTYO, ATTACHTRANSQTYO),
       ATTACHTRANSQTYI = NVL ( :ATTACHTRANSQTYI, ATTACHTRANSQTYI),
       CASEFROM = NVL ( :CASEFROM, CASEFROM),
       CONTRACNO = NVL ( :CONTRACNO, CONTRACNO),
       CONTRACTPRICE = NVL ( :CONTRACTPRICE, CONTRACTPRICE),
       COSTAMOUNT = NVL ( :COSTAMOUNT, COSTAMOUNT),
       DENTALREFFLAG = NVL ( :DENTALREFFLAG, DENTALREFFLAG),
       DRUGCASEFROM = NVL ( :DRUGCASEFROM, DRUGCASEFROM),
       ENDDATETIME = NVL ( :ENDDATETIME, ENDDATETIME),
       EXAMINEDISCFLAG = NVL ( :EXAMINEDISCFLAG, EXAMINEDISCFLAG),
       EXECFLAG = NVL ( :EXECFLAG, EXECFLAG),
       HOSPEMGFLAG = NVL ( :HOSPEMGFLAG, HOSPEMGFLAG),
       HOSPKIDFLAG = NVL ( :HOSPKIDFLAG, HOSPKIDFLAG),
       INSUAMOUNT1 = NVL ( :INSUAMOUNT1, INSUAMOUNT1),
       INSUAMOUNT2 = NVL ( :INSUAMOUNT2, INSUAMOUNT2),
       INSUEMGFLAG = NVL ( :INSUEMGFLAG, INSUEMGFLAG),
       INSUKIDFLAG = NVL ( :INSUKIDFLAG, INSUKIDFLAG),
       INSUORDERCODE = NVL ( :INSUORDERCODE, INSUORDERCODE),
       INSUSIGNI = NVL ( :INSUSIGNI, INSUSIGNI),
       INSUSIGNO = NVL ( :INSUSIGNO, INSUSIGNO),
       MAMAGEFLAG = NVL ( :MAMAGEFLAG, MAMAGEFLAG),
       MAMAGERATE = NVL ( :MAMAGERATE, MAMAGERATE),
       ORIGINALPRODUCER = NVL ( :ORIGINALPRODUCER, ORIGINALPRODUCER),
       PAYAMOUNT1 = NVL ( :PAYAMOUNT1, PAYAMOUNT1),
       PAYAMOUNT2 = NVL ( :PAYAMOUNT2, PAYAMOUNT2),
       PPFPERCENTAGE = NVL ( :PPFPERCENTAGE, PPFPERCENTAGE),
       PPFTYPE = NVL ( :PPFTYPE, PPFTYPE),
       PTRESOLUTIONCLASS = NVL ( :PTRESOLUTIONCLASS, PTRESOLUTIONCLASS),
       STOCKTRANSQTYI = NVL ( :STOCKTRANSQTYI, STOCKTRANSQTYI),
       STOCKTRANSQTYO = NVL ( :STOCKTRANSQTYO, STOCKTRANSQTYO),
       SUPPLYNO = NVL ( :SUPPLYNO, SUPPLYNO)
 WHERE ORDERCODE = :ORDERCODE";
            return DBWork.Connection.Execute(sql, AB0038, DBWork.Transaction);
        }

        //儲存 HIS_STKDMIT
        public int SetCommit_HIS_STKDMIT(AB0038 AB0038)
        {
            var sql = @"UPDATE HIS_STKDMIT
   SET DRUGEXTERIOR = NVL ( :DRUGEXTERIOR, DRUGEXTERIOR),
       DRUGENGEXTERIOR = NVL ( :DRUGENGEXTERIOR, DRUGENGEXTERIOR),
       CHINSIDEEFFECT = NVL ( :CHINSIDEEFFECT, CHINSIDEEFFECT),
       ENGSIDEEFFECT = NVL ( :ENGSIDEEFFECT, ENGSIDEEFFECT),
       CHINATTENTION = NVL ( :CHINATTENTION, CHINATTENTION),
       ENGATTENTION = NVL ( :ENGATTENTION, ENGATTENTION),
       DOHSYMPTOM = NVL ( :DOHSYMPTOM, DOHSYMPTOM),
       FDASYMPTOM = NVL ( :FDASYMPTOM, FDASYMPTOM),
       DRUGMEMO = NVL ( :DRUGMEMO, DRUGMEMO),
       SUCKLESECURITY = NVL ( :SUCKLESECURITY, SUCKLESECURITY),
       PREGNANTGRADE = NVL ( :PREGNANTGRADE, PREGNANTGRADE),
       DRUGPICTURELINK = NVL ( :DRUGPICTURELINK, DRUGPICTURELINK),
       DRUGLEAFLETLINK = NVL ( :DRUGLEAFLETLINK, DRUGLEAFLETLINK),
       MAXCURECONSISTENCY = NVL ( :MAXCURECONSISTENCY, MAXCURECONSISTENCY),
       MINCURECONSISTENCY = NVL ( :MINCURECONSISTENCY, MINCURECONSISTENCY),
       PEARBEGIN = NVL ( :PEARBEGIN, PEARBEGIN),
       PEAREND = NVL ( :PEAREND, PEAREND),
       TROUGHBEGIN = NVL ( :TROUGHBEGIN, TROUGHBEGIN),
       TROUGHEND = NVL ( :TROUGHEND, TROUGHEND),
       DANGERBEGIN = NVL ( :DANGERBEGIN, DANGERBEGIN),
       DANGEREND = NVL ( :DANGEREND, DANGEREND),
       TDMMEMO1 = NVL ( :TDMMEMO1, TDMMEMO1),
       TDMMEMO2 = NVL ( :TDMMEMO2, TDMMEMO2),
       TDMMEMO3 = NVL ( :TDMMEMO3, TDMMEMO3),
       APPLYTRANSQTY = NVL ( :APPLYTRANSQTY, APPLYTRANSQTY),
       APPLYUNIT = NVL ( :APPLYUNIT, APPLYUNIT),
       ARMYORDERCODE = NVL ( :ARMYORDERCODE, ARMYORDERCODE),
       CLASSIFIEDARMYNO = NVL ( :CLASSIFIEDARMYNO, CLASSIFIEDARMYNO),
       COMMITTEECODE = NVL ( :COMMITTEECODE, COMMITTEECODE),
       COMMITTEEMEMO = NVL ( :COMMITTEEMEMO, COMMITTEEMEMO),
       COMPONENTNUNIT = NVL ( :COMPONENTNUNIT, COMPONENTNUNIT),
       COMPONENTNUNIT2 = NVL ( :COMPONENTNUNIT2, COMPONENTNUNIT2),
       COMPONENTNUNIT3 = NVL ( :COMPONENTNUNIT3, COMPONENTNUNIT3),
       COMPONENTNUNIT4 = NVL ( :COMPONENTNUNIT4, COMPONENTNUNIT4),
       CONTRACTEFFECTIVEDATE =
          NVL ( :CONTRACTEFFECTIVEDATE, CONTRACTEFFECTIVEDATE),
       DRUGAPPLYTYPE = NVL ( :DRUGAPPLYTYPE, DRUGAPPLYTYPE),
       DRUGCLASS = NVL ( :DRUGCLASS, DRUGCLASS),
       DRUGCLASSIFY = NVL ( :DRUGCLASSIFY, DRUGCLASSIFY),
       DRUGFORM = NVL ( :DRUGFORM, DRUGFORM),
       DRUGPACKAGE = NVL ( :DRUGPACKAGE, DRUGPACKAGE),
       DRUGTOTALAMOUNT = NVL ( :DRUGTOTALAMOUNT, DRUGTOTALAMOUNT),
       DRUGTOTALAMOUNTUNIT = NVL ( :DRUGTOTALAMOUNTUNIT, DRUGTOTALAMOUNTUNIT),
       GROUPARMYNO = NVL ( :GROUPARMYNO, GROUPARMYNO),
       INVENTORYFLAG = NVL ( :INVENTORYFLAG, INVENTORYFLAG),
       ITEMARMYNO = NVL ( :ITEMARMYNO, ITEMARMYNO),
       MANUFACTURER = NVL ( :MANUFACTURER, MANUFACTURER),
       MULTIPRESCRIPTIONCODE =
          NVL ( :MULTIPRESCRIPTIONCODE, MULTIPRESCRIPTIONCODE),
       PARENTCODE = NVL ( :PARENTCODE, PARENTCODE),
       PARENTORDERCODE = NVL ( :PARENTORDERCODE, PARENTORDERCODE),
       PURCHASECASETYPE = NVL ( :PURCHASECASETYPE, PURCHASECASETYPE),
       PURCHASETRANSQTY = NVL ( :PURCHASETRANSQTY, PURCHASETRANSQTY),
       PURCHASEUNIT = NVL ( :PURCHASEUNIT, PURCHASEUNIT),
       SONTRANSQTY = NVL ( :SONTRANSQTY, SONTRANSQTY),
       SPECNUNIT = NVL ( :SPECNUNIT, SPECNUNIT),
       STOCKSOURCECODE = NVL ( :STOCKSOURCECODE, STOCKSOURCECODE),
       STOCKUSECODE = NVL ( :STOCKUSECODE, STOCKUSECODE),
       WARN = NVL ( :WARN, WARN),
       YEARARMYNO = NVL ( :YEARARMYNO, YEARARMYNO)
 WHERE SKORDERCODE = :ORDERCODE";
            return DBWork.Connection.Execute(sql, AB0038, DBWork.Transaction);
        }
    }
}
