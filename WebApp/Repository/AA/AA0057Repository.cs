using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0057Repository : JCLib.Mvc.BaseRepository
    {
        public AA0057Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0057M> GetAll(string p00, string p01, string p02, string p03, string p04, string p05, string p06, string p07, string p08, string p09, string p10, string p11, string p12, string p13, string p14, string p15, string p16, string p17)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT                   
                                B.BEGINDATE, -- ""生效起日""
                                B.ENDDATE, -- ""生效迄日""
                                A.ORDERCODE, -- ""院內代碼""
                                A.ORDERENGNAME, -- ""英文名稱""
                                A.ORDERCHINNAME, -- ""中文名稱""
                                A.SCIENTIFICNAME, -- ""成份名稱""
                                A.ORDERUNIT, -- ""醫囑單位""
                                A.ORDERCHINUNIT, -- ""中文單位""
                                A.ATTACHUNIT, -- ""劑型單位""
                                A.STOCKUNIT, -- ""扣庫單位""
                                A.SKORDERCODE, -- ""藥品藥材代碼""
                                A.UDSERVICEFLAG, -- ""UD服務""
                                A.TAKEKIND, -- ""服用藥別""
                                A.LIMITEDQTYO, -- ""門診倍數核發""
                                A.LIMITEDQTYI, -- ""住院倍數核發""
                                A.BUYORDERFLAG, -- ""買斷藥""
                                A.OPENDATE, -- ""系統啟用日期""
                                A.PUBLICDRUGFLAG, -- ""公藥否(Y/N)""
                                A.STARTDATE, -- ""開始日期""
                                A.ORDERHOSPNAME, -- ""院內名稱""
                                A.ORDEREASYNAME, -- ""簡稱""
                                A.INSUOFFERFLAG, -- ""保險給付否(Y/N)""
                                A.DCL, -- ""DCL""
                                A.APPENDMATERIALFLAG, -- ""健保帶材料否""
                                A.EXORDERFLAG, -- ""特殊品項""
                                A.ORDERDCFLAG, -- ""停用碼""
                                A.MAXQTYPERTIME, -- ""一次極量""
                                A.MAXQTYPERDAY, -- ""一日極量""
                                A.MAXTAKETIMES, -- ""限制次數""
                                A.DOHLICENSENO, -- ""衛生署核准字號""
                                A.RFIDCODE, -- ""RFID條碼""
                                A.PATHNO, -- ""給藥途徑(部位)代碼""
                                A.AGGREGATECODE, -- ""累計用藥""
                                A.LIMITFLAG, -- ""開立專用限制""
                                A.RESTRICTCODE, -- ""管制用藥""
                                A.ANTIBIOTICSCODE, -- ""抗生素等級""
                                A.CARRYKINDI, -- ""住消耗歸整""
                                A.CARRYKINDO, -- ""門急消耗歸整""
                                A.UDPOWDERFLAG, -- ""UD磨粉""
                                A.RETURNDRUGFLAG, -- ""合理回流藥""
                                A.RESEARCHDRUGFLAG, -- ""研究用藥""
                                A.MACHINEFLAG, -- ""藥包機品項""
                                A.TRANSCOMPUTEFLAG, -- ""結轉計價""
                                A.FIXPATHNOFLAG, -- ""限制途徑Y/N""
                                A.SYMPTOMCHIN, -- ""適應症(中文)""
                                A.SYMPTOMENG, -- ""適應症(英文)""
                                A.ONLYROUNDFLAG, -- ""不可剝半""
                                A.UNABLEPOWDERFLAG, -- ""不可磨粉""
                                A.DANGERDRUGFLAG, -- ""高警訊藥品""
                                A.DANGERDRUGMEMO, -- ""高警訊藥品提示""
                                A.COLDSTORAGEFLAG, -- ""冷藏存放""
                                A.LIGHTAVOIDFLAG, -- ""避光存放""
                                A.CHANGESTATUS, -- ""異動狀態""
                                A.FREQNOO, -- ""門診給藥頻率""
                                A.FREQNOI, -- ""住院給藥頻率""
                                A.ORDERDAYS, -- ""預設開立天數""
                                A.DOSE, -- ""劑量""
                                A.HOSPCHARGEID1, -- ""院內費用類別""
                                A.HOSPCHARGEID2, -- ""院內費用""
                                A.INSUCHARGEID1, -- ""健保費用類別""
                                A.INSUCHARGEID2, -- ""健保費用""
                                A.ORDERTYPE, -- ""醫令類別""
                                A.ORDERKIND, -- ""醫令類別(申報定義)""
                                A.HIGHPRICEFLAG, -- ""高價用藥""
                                A.CURETYPE, -- ""特殊治療種類""
                                A.INPDISPLAYFLAG, -- ""住院醫囑顯示""
                                A.SOONCULLFLAG, -- ""開立醫令即為報到""
                                A.SUBSTITUTE1, -- ""替代院內代碼1""
                                A.SUBSTITUTE2, -- ""替代院內代碼2""
                                A.SUBSTITUTE3, -- ""替代院內代碼3""
                                A.SUBSTITUTE4, -- ""替代院內代碼4""
                                A.SUBSTITUTE5, -- ""替代院內代碼5""
                                A.RELATEFLAGO, -- ""連帶否(門診)""
                                A.RELATEFLAGI, -- ""連帶否(住院)""
                                A.WEIGHTTYPE, -- ""體重及安全量：計算別""
                                A.WEIGHTUNITLIMIT, -- ""體重,安全量:限制數量""
                                A.RESTRICTTYPE, -- ""限制狀態""
                                A.MAXQTYO, -- ""門診限制開立數量""
                                A.MAXQTYI, -- ""住院限制開立數量""
                                A.MAXDAYSO, -- ""門診限制開立日數""
                                A.MAXDAYSI, -- ""住院限制開立日數""
                                A.VALIDDAYSO, -- ""門診效期日數""
                                A.VALIDDAYSI, -- ""住院效期日數""
                                A.CHECKINSWITCH, -- ""是否需報到""
                                A.REPORTFLAG, -- ""是否發報告Y/N""
                                A.SINGLEITEMFLAG, -- ""SINGLEITEM""
                                A.OPERATIONFLAG, -- ""手術碼""
                                A.EXAMINEDRUGFLAG, -- ""檢驗用藥""
                                A.MAINCUREITEM, -- ""特定治療項目""
                                A.RAYPOSITION, -- ""放射部位""
                                A.CCORDERCODE, -- ""空針代碼""
                                A.XRAYORDERCODE, -- ""放射第二張代碼""
                                A.ORDIAGCODE, -- ""手術診斷碼""
                                A.ORDERCODESORT, -- ""醫令排序""
                                A.SENDUNITFLAG, -- ""傳送單位否""
                                A.SIGNFLAG, -- ""是否需SIGNIN""
                                A.EXCLUDEFLAG, -- ""除外項目""
                                A.NEEDOPDTYPEFLAG, -- ""處置需報調劑方式""
                                A.DRUGELEMCODE1, -- ""藥品成份ELEMENT1""
                                A.DRUGELEMCODE2, -- ""藥品成份ELEMENT2""
                                A.DRUGELEMCODE3, -- ""藥品成份ELEMENT3""
                                A.DRUGELEMCODE4, -- ""藥品成份ELEMENT4""
                                A.CHANGEABLEFLAG, -- ""可線上異動否""
                                A.TDMFLAG, -- ""TDM藥品""
                                A.SPECIALORDERKIND, -- ""專案碼""
                                A.NEEDREGIONFLAG, -- ""處置需報部位""
                                A.ORDERUSETYPE, -- ""醫令使用狀態""
                                A.FIXDOSEFLAG, -- ""預設劑量""
                                A.RAREDISORDERFLAG, -- ""罕見疾病用藥""
                                A.HOSPEXAMINEFLAG, -- ""內審用藥""
                                A.ORDERCONDCODE, -- ""給付條文代碼""
                                A.HOSPEXAMINEQTYFLAG, -- ""內審限制用量""
                                A.CREATEDATETIME, -- ""記錄建立日期/時間""
                                A.CREATEOPID, -- ""記錄建立人員""
                                A.PROCDATETIME, -- ""記錄處理日期/時間""
                                A.PROCOPID, -- ""記錄處理人員""
                                A.HEPATITISCODE, -- ""BC肝用藥註記""
    
                                -- B.ORDERCODE, -- ""院內代碼""
                                B.BEGINDATE, -- ""生效起日""
                                B.ENDDATE, -- ""生效迄日""
                                B.STOCKTRANSQTYO, -- ""門診扣庫轉換量""
                                B.STOCKTRANSQTYI, -- ""住院扣庫轉換量""
                                B.ATTACHTRANSQTYO, -- ""門診劑型數量""
                                B.ATTACHTRANSQTYI, -- ""住院劑型數量""
                                B.INSUAMOUNT1, -- ""健保點數一""
                                B.INSUAMOUNT2, -- ""健保點數二""
                                B.PAYAMOUNT1, -- ""自費點數一""
                                B.PAYAMOUNT2, -- ""自費點數二""
                                B.COSTAMOUNT, -- ""進價""
                                B.MAMAGEFLAG, -- ""是否收管理費""
                                B.MAMAGERATE, -- ""管理費%""
                                B.INSUORDERCODE, -- ""健保碼""
                                B.INSUSIGNI, -- ""健保負擔碼(住院)""
                                B.INSUSIGNO, -- ""健保負擔碼(門診)""
                                B.INSUEMGFLAG, -- ""健保急件加成""
                                B.HOSPEMGFLAG, -- ""自費急件加成""
                                B.DENTALREFFLAG, -- ""牙科轉診加成""
                                B.PPFTYPE, -- ""提成類別碼""
                                B.PPFPERCENTAGE, -- ""提成百分比""
                                B.INSUKIDFLAG, -- ""兒童加成否""
                                B.HOSPKIDFLAG, -- ""自費兒童加成""
                                B.CONTRACTPRICE, -- ""合約單價""
                                B.CONTRACNO, -- ""合約碼""
                                B.SUPPLYNO, -- ""廠商代碼(供應商代碼)""
                                B.CASEFROM, -- ""標案來源""
                                B.EXAMINEDISCFLAG, -- ""檢查折扣否""
                                B.ORIGINALPRODUCER, -- ""製造廠名稱""
                                B.AGENTNAME, -- ""申請商名稱""
                                -- B.CREATEDATETIME, -- ""記錄建立日期/時間""
                                -- B.CREATEOPID, -- ""記錄建立人員""
                                -- B.PROCDATETIME, -- ""記錄處理日期/時間""
                                -- B.PROCOPID, -- ""記錄處理人員""
                                B.ENDDATETIME -- ""結束日期時間""
                            FROM 
                                HIS_BASORDM A, HIS_BASORDD B, PH_VENDER C, MI_MAST D
                            WHERE 
                                A.ORDERCODE = B.ORDERCODE
                                --AND B.BEGINDATE <= TWN_DATE(SYSDATE)
                                --AND B.ENDDATE >= TWN_DATE(SYSDATE) 
                                --AND (SELECT COUNT(*) FROM PH_VENDER WHERE AGEN_NO = B.SUPPLYNO) > 0
                                --AND (SELECT COUNT(*) FROM MI_MAST WHERE MMCODE = A.ORDERCODE) > 0 
                                AND A.ORDERCODE = D.MMCODE
                                AND B.SUPPLYNO = C.AGEN_NO ";

            //院內碼範圍
            if (!string.IsNullOrWhiteSpace(p00))
            {
                sql += @" AND A.ORDERCODE >= :p00 ";
                p.Add(":p00", string.Format("{0}", p00));
            }

            if (!string.IsNullOrWhiteSpace(p01))
            {
                sql += @" AND A.ORDERCODE <= :p01 ";
                p.Add(":p01", string.Format("{0}", p01));
            }

            //廠商代碼範圍
            if (!string.IsNullOrWhiteSpace(p02))
            {     
                sql += @" AND C.AGEN_NO >= :p02 ";
                p.Add(":p02", string.Format("{0}", p02));
            }

            if (!string.IsNullOrWhiteSpace(p03))
            {
                sql += @" AND C.AGEN_NO <= :p03 ";             
                p.Add(":p03", string.Format("{0}", p03));
            }

            //健保碼範圍
            if (!string.IsNullOrWhiteSpace(p04))
            {
                sql += @" AND B.INSUORDERCODE >= :p04 ";
                p.Add(":p04", string.Format("{0}", p04));
            }

            if (!string.IsNullOrWhiteSpace(p05))
            {
                sql += @" AND B.INSUORDERCODE <= :p05 ";
                p.Add(":p05", string.Format("{0}", p05));
            }

            //廠牌範圍
            if (!string.IsNullOrWhiteSpace(p06))
            {
                sql += @" AND D.E_MANUFACT >= :p06 ";
                p.Add(":p06", string.Format("{0}", p06));
            }

            if (!string.IsNullOrWhiteSpace(p07))
            {
                sql += @" AND D.E_MANUFACT <= :p07 ";
                p.Add(":p07", string.Format("{0}", p07));
            }

            //建檔日期
            if (!string.IsNullOrWhiteSpace(p08))
            {
                sql += @" AND TO_DATE(SUBSTR(A.CREATEDATETIME, 0, 7)+19110000, 'YYYYMMDD') >= TO_DATE(SUBSTR(:p08,0,10), 'YYYY-MM-DD')";
                p.Add(":p08", string.Format("{0}", p08));
            }

            if (!string.IsNullOrWhiteSpace(p09))
            {
                sql += @" AND TO_DATE(SUBSTR(A.CREATEDATETIME, 0, 7)+19110000, 'YYYYMMDD') <= TO_DATE(SUBSTR(:p09,0,10), 'YYYY-MM-DD')";
                p.Add(":p09", string.Format("{0}", p09));
            }

            //異動日期
            if (!string.IsNullOrWhiteSpace(p10))
            {
                sql += @" AND TO_DATE(SUBSTR(A.PROCDATETIME, 0, 7)+19110000, 'YYYYMMDD') >= TO_DATE(SUBSTR(:p10,0,10), 'YYYY-MM-DD')";
                p.Add(":p10", string.Format("{0}", p10));
            }

            if (!string.IsNullOrWhiteSpace(p11))
            {
                sql += @" AND TO_DATE(SUBSTR(A.PROCDATETIME, 0, 7)+19110000, 'YYYYMMDD') <= TO_DATE(SUBSTR(:p11,0,10), 'YYYY-MM-DD')";
                p.Add(":p11", string.Format("{0}", p11));
            }

            //停用狀況
            if (!string.IsNullOrWhiteSpace(p12))
            {
                sql += @" AND A.ORDERDCFLAG = :p12 ";
                p.Add(":p12", string.Format("{0}", p12));
            }

            //查詢類別 (第1碼 D: 1_藥品，其他: 0_非藥品非衛材)
            if (!string.IsNullOrWhiteSpace(p13))
            {
                if (p13 == "1")
                {
                    sql += @" AND SUBSTR(A.ORDERTYPE, 1, 1) = 'D' ";
                }
                else if (p13 == "0")
                {
                    sql += @" AND SUBSTR(A.ORDERTYPE, 1, 1) != 'D' ";
                }                
                p.Add(":p13", string.Format("{0}", p13));
            }

            //有無健保碼
            if (!string.IsNullOrWhiteSpace(p14))
            {
                if (p14 == "Y")
                {
                    sql += @" AND (B.INSUORDERCODE IS NOT NULL AND TRIM(B.INSUORDERCODE) IS NOT NULL AND TRIM(REPLACE(B.INSUORDERCODE, '　', ' ')) IS NOT NULL) ";
                }
                else if (p14 == "N")
                {
                    sql += @" AND (B.INSUORDERCODE IS NULL OR TRIM(B.INSUORDERCODE) IS NULL OR TRIM(REPLACE(B.INSUORDERCODE, '　', ' ')) IS NULL) ";
                }
                p.Add(":p14", string.Format("{0}", p14));
            }

            //標案來源
            if (!string.IsNullOrWhiteSpace(p15))
            {
                sql += @" AND B.CASEFROM = :p15 ";
                p.Add(":p15", string.Format("{0}", p15));
            }

            //健保給付狀況(住院)
            if (!string.IsNullOrWhiteSpace(p16))
            {
                sql += @" AND B.INSUSIGNI = :p16 ";
                p.Add(":p16", string.Format("{0}", p16));
            }

            //健保給付狀況(門診)
            if (!string.IsNullOrWhiteSpace(p17))
            {
                sql += @" AND B.INSUSIGNO = :p17 ";
                p.Add(":p17", string.Format("{0}", p17));
            }

            sql += @" ORDER BY B.ENDDATE";

            return DBWork.Connection.Query<AA0057M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string p00, string p01, string p02, string p03, string p04, string p05, string p06, string p07, string p08, string p09, string p10, string p11, string p12, string p13, string p14, string p15, string p16, string p17)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT       
                                B.BEGINDATE AS ""生效起日"",
                                B.ENDDATE AS ""生效迄日"",
                                A.ORDERCODE AS ""院內代碼"",
                                A.ORDERENGNAME AS ""英文名稱"",
                                A.ORDERCHINNAME AS ""中文名稱"",
                                A.SCIENTIFICNAME AS ""成份名稱"",
                                A.ORDERUNIT AS ""醫囑單位"",
                                A.ORDERCHINUNIT AS ""中文單位"",
                                A.ATTACHUNIT AS ""劑型單位"",
                                A.STOCKUNIT AS ""扣庫單位"",
                                A.SKORDERCODE AS ""藥品藥材代碼"",
                                A.UDSERVICEFLAG AS ""UD服務"",
                                A.TAKEKIND AS ""服用藥別"",
                                A.LIMITEDQTYO AS ""門診倍數核發"",
                                A.LIMITEDQTYI AS ""住院倍數核發"",
                                A.BUYORDERFLAG AS ""買斷藥"",
                                A.OPENDATE AS ""系統啟用日期"",
                                A.PUBLICDRUGFLAG AS ""公藥否(Y/N)"",
                                A.STARTDATE AS ""開始日期"",
                                A.ORDERHOSPNAME AS ""院內名稱"",
                                A.ORDEREASYNAME AS ""簡稱"",
                                A.INSUOFFERFLAG AS ""保險給付否(Y/N)"",
                                A.DCL AS ""DCL"",
                                A.APPENDMATERIALFLAG AS ""健保帶材料否"",
                                A.EXORDERFLAG AS ""特殊品項"",
                                A.ORDERDCFLAG AS ""停用碼"",
                                A.MAXQTYPERTIME AS ""一次極量"",
                                A.MAXQTYPERDAY AS ""一日極量"",
                                A.MAXTAKETIMES AS ""限制次數"",
                                A.DOHLICENSENO AS ""衛生署核准字號"",
                                A.RFIDCODE AS ""RFID條碼"",
                                A.PATHNO AS ""給藥途徑(部位)代碼"",
                                A.AGGREGATECODE AS ""累計用藥"",
                                A.LIMITFLAG AS ""開立專用限制"",
                                A.RESTRICTCODE AS ""管制用藥"",
                                A.ANTIBIOTICSCODE AS ""抗生素等級"",
                                A.CARRYKINDI AS ""住消耗歸整"",
                                A.CARRYKINDO AS ""門急消耗歸整"",
                                A.UDPOWDERFLAG AS ""UD磨粉"",
                                A.RETURNDRUGFLAG AS ""合理回流藥"",
                                A.RESEARCHDRUGFLAG AS ""研究用藥"",
                                A.MACHINEFLAG AS ""藥包機品項"",
                                A.TRANSCOMPUTEFLAG AS ""結轉計價"",
                                A.FIXPATHNOFLAG AS ""限制途徑Y/N"",
                                A.SYMPTOMCHIN AS ""適應症(中文)"",
                                A.SYMPTOMENG AS ""適應症(英文)"",
                                A.ONLYROUNDFLAG AS ""不可剝半"",
                                A.UNABLEPOWDERFLAG AS ""不可磨粉"",
                                A.DANGERDRUGFLAG AS ""高警訊藥品"",
                                A.DANGERDRUGMEMO AS ""高警訊藥品提示"",
                                A.COLDSTORAGEFLAG AS ""冷藏存放"",
                                A.LIGHTAVOIDFLAG AS ""避光存放"",
                                A.CHANGESTATUS AS ""異動狀態"",
                                A.FREQNOO AS ""門診給藥頻率"",
                                A.FREQNOI AS ""住院給藥頻率"",
                                A.ORDERDAYS AS ""預設開立天數"",
                                A.DOSE AS ""劑量"",
                                A.HOSPCHARGEID1 AS ""院內費用類別"",
                                A.HOSPCHARGEID2 AS ""院內費用"",
                                A.INSUCHARGEID1 AS ""健保費用類別"",
                                A.INSUCHARGEID2 AS ""健保費用"",
                                A.ORDERTYPE AS ""醫令類別"",
                                A.ORDERKIND AS ""醫令類別(申報定義)"",
                                A.HIGHPRICEFLAG AS ""高價用藥"",
                                A.CURETYPE AS ""特殊治療種類"",
                                A.INPDISPLAYFLAG AS ""住院醫囑顯示"",
                                A.SOONCULLFLAG AS ""開立醫令即為報到"",
                                A.SUBSTITUTE1 AS ""替代院內代碼1"",
                                A.SUBSTITUTE2 AS ""替代院內代碼2"",
                                A.SUBSTITUTE3 AS ""替代院內代碼3"",
                                A.SUBSTITUTE4 AS ""替代院內代碼4"",
                                A.SUBSTITUTE5 AS ""替代院內代碼5"",
                                A.RELATEFLAGO AS ""連帶否(門診)"",
                                A.RELATEFLAGI AS ""連帶否(住院)"",
                                A.WEIGHTTYPE AS ""體重及安全量：計算別"",
                                A.WEIGHTUNITLIMIT AS ""體重及安全量：限制數量"",
                                A.RESTRICTTYPE AS ""限制狀態"",
                                A.MAXQTYO AS ""門診限制開立數量"",
                                A.MAXQTYI AS ""住院限制開立數量"",
                                A.MAXDAYSO AS ""門診限制開立日數"",
                                A.MAXDAYSI AS ""住院限制開立日數"",
                                A.VALIDDAYSO AS ""門診效期日數"",
                                A.VALIDDAYSI AS ""住院效期日數"",
                                A.CHECKINSWITCH AS ""是否需報到"",
                                A.REPORTFLAG AS ""是否發報告Y/N"",
                                A.SINGLEITEMFLAG AS ""SINGLEITEM"",
                                A.OPERATIONFLAG AS ""手術碼"",
                                A.EXAMINEDRUGFLAG AS ""檢驗用藥"",
                                A.MAINCUREITEM AS ""特定治療項目"",
                                A.RAYPOSITION AS ""放射部位"",
                                A.CCORDERCODE AS ""空針代碼"",
                                A.XRAYORDERCODE AS ""放射第二張代碼"",
                                A.ORDIAGCODE AS ""手術診斷碼"",
                                A.ORDERCODESORT AS ""醫令排序"",
                                A.SENDUNITFLAG AS ""傳送單位否"",
                                A.SIGNFLAG AS ""是否需SIGNIN"",
                                A.EXCLUDEFLAG AS ""除外項目"",
                                A.NEEDOPDTYPEFLAG AS ""處置需報調劑方式"",
                                A.DRUGELEMCODE1 AS ""藥品成份ELEMENT1"",
                                A.DRUGELEMCODE2 AS ""藥品成份ELEMENT2"",
                                A.DRUGELEMCODE3 AS ""藥品成份ELEMENT3"",
                                A.DRUGELEMCODE4 AS ""藥品成份ELEMENT4"",
                                A.CHANGEABLEFLAG AS ""可線上異動否"",
                                A.TDMFLAG AS ""TDM藥品"",
                                A.SPECIALORDERKIND AS ""專案碼"",
                                A.NEEDREGIONFLAG AS ""處置需報部位"",
                                A.ORDERUSETYPE AS ""醫令使用狀態"",
                                A.FIXDOSEFLAG AS ""預設劑量"",
                                A.RAREDISORDERFLAG AS ""罕見疾病用藥"",
                                A.HOSPEXAMINEFLAG AS ""內審用藥"",
                                A.ORDERCONDCODE AS ""給付條文代碼"",
                                A.HOSPEXAMINEQTYFLAG AS ""內審限制用量"",
                                A.CREATEDATETIME AS ""記錄建立日期/時間"",
                                A.CREATEOPID AS ""記錄建立人員"",
                                A.PROCDATETIME AS ""記錄處理日期/時間"",
                                A.PROCOPID AS ""記錄處理人員"",
                                A.HEPATITISCODE AS ""BC肝用藥註記"",
    
                                -- B.ORDERCODE AS ""院內代碼"",
                                B.BEGINDATE AS ""生效起日"",
                                B.ENDDATE AS ""生效迄日"",
                                B.STOCKTRANSQTYO AS ""門診扣庫轉換量"",
                                B.STOCKTRANSQTYI AS ""住院扣庫轉換量"",
                                B.ATTACHTRANSQTYO AS ""門診劑型數量"",
                                B.ATTACHTRANSQTYI AS ""住院劑型數量"",
                                B.INSUAMOUNT1 AS ""健保點數一"",
                                B.INSUAMOUNT2 AS ""健保點數二"",
                                B.PAYAMOUNT1 AS ""自費點數一"",
                                B.PAYAMOUNT2 AS ""自費點數二"",
                                B.COSTAMOUNT AS ""進價"",
                                B.MAMAGEFLAG AS ""是否收管理費"",
                                B.MAMAGERATE AS ""管理費%"",
                                B.INSUORDERCODE AS ""健保碼"",
                                B.INSUSIGNI AS ""健保負擔碼(住院)"",
                                B.INSUSIGNO AS ""健保負擔碼(門診)"",
                                B.INSUEMGFLAG AS ""健保急件加成"",
                                B.HOSPEMGFLAG AS ""自費急件加成"",
                                B.DENTALREFFLAG AS ""牙科轉診加成"",
                                B.PPFTYPE AS ""提成類別碼"",
                                B.PPFPERCENTAGE AS ""提成百分比"",
                                B.INSUKIDFLAG AS ""兒童加成否"",
                                B.HOSPKIDFLAG AS ""自費兒童加成"",
                                B.CONTRACTPRICE AS ""合約單價"",
                                B.CONTRACNO AS ""合約碼"",
                                B.SUPPLYNO AS ""廠商代碼(供應商代碼)"",
                                B.CASEFROM AS ""標案來源"",
                                B.EXAMINEDISCFLAG AS ""檢查折扣否"",
                                B.ORIGINALPRODUCER AS ""製造廠名稱"",
                                B.AGENTNAME AS ""申請商名稱"",
                                -- B.CREATEDATETIME AS ""記錄建立日期/時間"",
                                -- B.CREATEOPID AS ""記錄建立人員"",
                                -- B.PROCDATETIME AS ""記錄處理日期/時間"",
                                -- B.PROCOPID AS ""記錄處理人員"",
                                B.ENDDATETIME AS ""結束日期時間""
                            FROM 
                                HIS_BASORDM A, HIS_BASORDD B, PH_VENDER C, MI_MAST D
                            WHERE 
                                A.ORDERCODE = B.ORDERCODE                                
                                --AND B.BEGINDATE <= TWN_DATE(SYSDATE)
                                --AND B.ENDDATE >= TWN_DATE(SYSDATE) 
                                --AND (SELECT COUNT(*) FROM PH_VENDER WHERE AGEN_NO = B.SUPPLYNO) > 0
                                --AND (SELECT COUNT(*) FROM MI_MAST WHERE MMCODE = A.ORDERCODE) > 0 
                                AND A.ORDERCODE = D.MMCODE
                                AND B.SUPPLYNO = C.AGEN_NO ";

            //院內碼範圍
            if (!string.IsNullOrWhiteSpace(p00))
            {
                sql += @" AND A.ORDERCODE >= :p00 ";
                p.Add(":p00", string.Format("{0}", p00));
            }

            if (!string.IsNullOrWhiteSpace(p01))
            {
                sql += @" AND A.ORDERCODE <= :p01 ";
                p.Add(":p01", string.Format("{0}", p01));
            }

            //廠商代碼範圍
            if (!string.IsNullOrWhiteSpace(p02))
            {
                sql += @" AND C.AGEN_NO >= :p02 ";
                p.Add(":p02", string.Format("{0}", p02));
            }

            if (!string.IsNullOrWhiteSpace(p03))
            {
                sql += @" AND C.AGEN_NO <= :p03 ";
                p.Add(":p03", string.Format("{0}", p03));
            }

            //健保碼範圍
            if (!string.IsNullOrWhiteSpace(p04))
            {
                sql += @" AND B.INSUORDERCODE >= :p04 ";
                p.Add(":p04", string.Format("{0}", p04));
            }

            if (!string.IsNullOrWhiteSpace(p05))
            {
                sql += @" AND B.INSUORDERCODE <= :p05 ";
                p.Add(":p05", string.Format("{0}", p05));
            }

            //廠牌範圍
            if (!string.IsNullOrWhiteSpace(p06))
            {
                sql += @" AND D.E_MANUFACT >= :p06 ";
                p.Add(":p06", string.Format("{0}", p06));
            }

            if (!string.IsNullOrWhiteSpace(p07))
            {
                sql += @" AND D.E_MANUFACT <= :p07 ";
                p.Add(":p07", string.Format("{0}", p07));
            }

            //建檔日期
            if (!string.IsNullOrWhiteSpace(p08))
            {
                sql += @" AND TO_DATE(SUBSTR(A.CREATEDATETIME, 0, 7)+19110000, 'YYYYMMDD') >= TO_DATE(SUBSTR(:p08,0,10), 'YYYY-MM-DD')";
                p.Add(":p08", string.Format("{0}", p08));
            }

            if (!string.IsNullOrWhiteSpace(p09))
            {
                sql += @" AND TO_DATE(SUBSTR(A.CREATEDATETIME, 0, 7)+19110000, 'YYYYMMDD') <= TO_DATE(SUBSTR(:p09,0,10), 'YYYY-MM-DD')";
                p.Add(":p09", string.Format("{0}", p09));
            }

            //異動日期
            if (!string.IsNullOrWhiteSpace(p10))
            {
                sql += @" AND TO_DATE(SUBSTR(A.PROCDATETIME, 0, 7)+19110000, 'YYYYMMDD') >= TO_DATE(SUBSTR(:p10,0,10), 'YYYY-MM-DD')";
                p.Add(":p10", string.Format("{0}", p10));
            }

            if (!string.IsNullOrWhiteSpace(p11))
            {
                sql += @" AND TO_DATE(SUBSTR(A.PROCDATETIME, 0, 7)+19110000, 'YYYYMMDD') <= TO_DATE(SUBSTR(:p11,0,10), 'YYYY-MM-DD')";
                p.Add(":p11", string.Format("{0}", p11));
            }

            //停用狀況
            if (!string.IsNullOrWhiteSpace(p12))
            {
                sql += @" AND A.ORDERDCFLAG = :p12 ";
                p.Add(":p12", string.Format("{0}", p12));
            }

            //查詢類別 (第1碼 D: 1_藥品，其他: 0_非藥品非衛材)
            if (!string.IsNullOrWhiteSpace(p13))
            {
                if (p13 == "1")
                {
                    sql += @" AND SUBSTR(A.ORDERTYPE, 1, 1) = 'D' ";
                }
                else if (p13 == "0")
                {
                    sql += @" AND SUBSTR(A.ORDERTYPE, 1, 1) != 'D' ";
                }
                p.Add(":p13", string.Format("{0}", p13));
            }

            //有無健保碼
            if (!string.IsNullOrWhiteSpace(p14))
            {
                if (p14 == "Y")
                {
                    sql += @" AND (B.INSUORDERCODE IS NOT NULL AND TRIM(B.INSUORDERCODE) IS NOT NULL AND TRIM(REPLACE(B.INSUORDERCODE, '　', ' ')) IS NOT NULL) ";
                }
                else if (p14 == "N")
                {
                    sql += @" AND (B.INSUORDERCODE IS NULL OR TRIM(B.INSUORDERCODE) IS NULL OR TRIM(REPLACE(B.INSUORDERCODE, '　', ' ')) IS NULL) ";
                }
                p.Add(":p14", string.Format("{0}", p14));
            }

            //標案來源
            if (!string.IsNullOrWhiteSpace(p15))
            {
                sql += @" AND B.CASEFROM = :p15 ";
                p.Add(":p15", string.Format("{0}", p15));
            }

            //健保給付狀況(住院)
            if (!string.IsNullOrWhiteSpace(p16))
            {
                sql += @" AND B.INSUSIGNI = :p16 ";
                p.Add(":p16", string.Format("{0}", p16));
            }

            //健保給付狀況(門診)
            if (!string.IsNullOrWhiteSpace(p17))
            {
                sql += @" AND B.INSUSIGNO = :p17 ";
                p.Add(":p17", string.Format("{0}", p17));
            }

            sql += @" ORDER BY B.ENDDATE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboItemModel> GetOrderDCFlag()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT DATA_VALUE AS VALUE, DATA_VALUE || ' ' || DATA_DESC TEXT
                            FROM PARAM_D
                            WHERE GRP_CODE = 'HIS_BASORDM' AND DATA_NAME = 'ORDERDCFLAG'
                            ORDER BY VALUE DESC";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetCaseFrom()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT DATA_VALUE AS VALUE, DATA_VALUE || ' ' || DATA_DESC TEXT
                            FROM PARAM_D
                            WHERE GRP_CODE = 'HIS_BASORDD' AND DATA_NAME = 'CASEFROM'
                            ORDER BY VALUE ";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetInsuSignI()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT DATA_VALUE AS VALUE, DATA_VALUE || ' ' || DATA_DESC TEXT
                            FROM PARAM_D
                            WHERE GRP_CODE = 'HIS_BASORDD' AND DATA_NAME = 'INSUSIGNI'
                            ORDER BY VALUE ";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetInsuSignO()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT DATA_VALUE AS VALUE, DATA_VALUE || ' ' || DATA_DESC TEXT
                            FROM PARAM_D
                            WHERE GRP_CODE = 'HIS_BASORDD' AND DATA_NAME = 'INSUSIGNO'
                            ORDER BY VALUE ";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        //查詢Master
        //public IEnumerable<AA0057M> GetAllM(string MAT_CLASS, string MMCODE, string FRWH, bool clsALL ,string FLOWID, string PR_TIME_B, string PR_TIME_E, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT 
        //                    A.DOCNO,
        //                    A.FLOWID,
        //                    (SELECT MAT_CLASS || ' ' || MAT_CLSNAME TEXT FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MAT_CLASS,
        //                    (SELECT WH_NO || ' ' || WH_NAME TEXT FROM MI_WHMAST WHERE WH_NO = A.FRWH) AS FRWH,
        //                    A.APPTIME,
        //                    A.APPLY_NOTE,
        //                    B.MMCODE,
        //                    (SELECT MMNAME_C TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS MMNAME_C,
        //                    (SELECT MMNAME_E TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS MMNAME_E,
        //                    B.APPQTY,
        //                    (SELECT BASE_UNIT TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS BASE_UNIT,
        //                    (SELECT M_CONTPRICE TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS M_CONTPRICE
        //                FROM
        //                    ME_DOCM A, ME_DOCD B
        //                WHERE
        //                    1=1 AND A.DOCNO = B.DOCNO AND (A.DOCTYPE = 'MR1' OR A.DOCTYPE = 'MR2') ";

        //    if (!string.IsNullOrWhiteSpace(MAT_CLASS))
        //    {
        //        if (clsALL == true)
        //        {
        //            sql += @" AND A.MAT_CLASS IN ("+MAT_CLASS+") ";
        //        }
        //        else
        //        {
        //            sql += @" AND A.MAT_CLASS = :p0 ";
        //            p.Add(":p0", string.Format("{0}", MAT_CLASS));
        //        }
        //    }

        //    if (!string.IsNullOrWhiteSpace(MMCODE))
        //    {
        //        sql += @" AND B.MMCODE LIKE :p1 ";
        //        p.Add(":p1", string.Format("{0}", MMCODE));
        //    }

        //    if (!string.IsNullOrWhiteSpace(FRWH))
        //    {
        //        sql += @" AND A.FRWH LIKE :p2 ";
        //        p.Add(":p2", string.Format("{0}", FRWH));
        //    }

        //    if (!string.IsNullOrWhiteSpace(FLOWID))
        //    {
        //        sql += @" AND A.FLOWID LIKE :p4 ";
        //        p.Add(":p4", string.Format("{0}", FLOWID));
        //    }

        //    if (!string.IsNullOrWhiteSpace(PR_TIME_B))
        //    {
        //        sql += " AND APPTIME >= (TO_DATE(:PR_TIME_B,'YYYY/mm/dd')) ";
        //        p.Add(":PR_TIME_B", string.Format("{0}", DateTime.Parse(PR_TIME_B).ToString("yyyy/MM/dd")));
        //    }

        //    if (!string.IsNullOrWhiteSpace(PR_TIME_E))
        //    {
        //        sql += " AND APPTIME <= (TO_DATE(:PR_TIME_E,'YYYY/mm/dd')) ";
        //        p.Add(":PR_TIME_E", string.Format("{0}", DateTime.Parse(PR_TIME_E).ToString("yyyy/MM/dd")));
        //    }

        //    sql += @" ORDER BY A.APPTIME, A.DOCNO, B.SEQ ";


        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<AA0057M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        //public IEnumerable<AA0057M> Print(string MAT_CLASS, string MMCODE, string FRWH, bool clsALL, string FLOWID, string PR_TIME_B, string PR_TIME_E)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT 
        //                    A.DOCNO,
        //                    A.FLOWID,
        //                    (SELECT MAT_CLASS || ' ' || MAT_CLSNAME TEXT FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MAT_CLASS,
        //                    (SELECT WH_NO || ' ' || WH_NAME TEXT FROM MI_WHMAST WHERE WH_NO = A.FRWH) AS FRWH,
        //                    A.APPTIME,
        //                    A.APPLY_NOTE,
        //                    B.MMCODE,
        //                    (SELECT MMNAME_C TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS MMNAME_C,
        //                    (SELECT MMNAME_E TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS MMNAME_E,
        //                    B.APPQTY,
        //                    (SELECT BASE_UNIT TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS BASE_UNIT,
        //                    (SELECT M_CONTPRICE TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS M_CONTPRICE
        //                FROM
        //                    ME_DOCM A, ME_DOCD B
        //                WHERE
        //                    1=1 AND A.DOCNO = B.DOCNO AND (A.DOCTYPE = 'MR1' OR A.DOCTYPE = 'MR2') ";

        //    if (!string.IsNullOrWhiteSpace(MAT_CLASS))
        //    {
        //        if (clsALL == true)
        //        {
        //            sql += @" AND A.MAT_CLASS IN (" + MAT_CLASS + ") ";
        //        }
        //        else
        //        {
        //            sql += @" AND A.MAT_CLASS = :p0 ";
        //            p.Add(":p0", string.Format("{0}", MAT_CLASS));
        //        }
        //    }

        //    if (!string.IsNullOrWhiteSpace(MMCODE))
        //    {
        //        sql += @" AND B.MMCODE LIKE :p1 ";
        //        p.Add(":p1", string.Format("{0}", MMCODE));
        //    }

        //    if (!string.IsNullOrWhiteSpace(FRWH))
        //    {
        //        sql += @" AND A.FRWH LIKE :p2 ";
        //        p.Add(":p2", string.Format("{0}", FRWH));
        //    }

        //    if (!string.IsNullOrWhiteSpace(FLOWID))
        //    {
        //        sql += @" AND A.FLOWID LIKE :p4 ";
        //        p.Add(":p4", string.Format("{0}", FLOWID));
        //    }

        //    if (!string.IsNullOrWhiteSpace(PR_TIME_B))
        //    {
        //        sql += " AND APPTIME >= (TO_DATE(:PR_TIME_B,'YYYY/mm/dd')) ";
        //        p.Add(":PR_TIME_B", string.Format("{0}", DateTime.Parse(PR_TIME_B).ToString("yyyy/MM/dd")));
        //    }

        //    if (!string.IsNullOrWhiteSpace(PR_TIME_E))
        //    {
        //        sql += " AND APPTIME <= (TO_DATE(:PR_TIME_E,'YYYY/mm/dd')) ";
        //        p.Add(":PR_TIME_E", string.Format("{0}", DateTime.Parse(PR_TIME_E).ToString("yyyy/MM/dd")));
        //    }

        //    sql += @" ORDER BY A.APPTIME, A.DOCNO, B.SEQ ";

        //    return DBWork.Connection.Query<AA0057M>(sql, p, DBWork.Transaction);
        //}

        //public IEnumerable<ComboItemModel> GetCLSNAME(string userid)
        //{
        //    var p = new DynamicParameters();

        //    string sql = @"  SELECT MAT_CLASS VALUE,
        //                            MAT_CLASS || ' ' || MAT_CLSNAME TEXT
        //                     FROM MI_MATCLASS
        //                     WHERE MAT_CLSID = WHM1_TASK(:p0)
        //                     ORDER BY MAT_CLASS";

        //    p.Add(":p0", string.Format("{0}", userid));

        //    return DBWork.Connection.Query<ComboItemModel>(sql, p , DBWork.Transaction);
        //}

        //public IEnumerable<ComboItemModel> GetFLOWID()
        //{
        //    string sql = @"  SELECT FLOWID
        //                     FROM ME_FLOW
        //                     ORDER BY FLOWID";
        //    return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        //}




        //public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT {0} 
        //                    A.MMCODE, 
        //                    A.MMNAME_C, 
        //                    A.MMNAME_E, 
        //                    A.MAT_CLASS, 
        //                    A.BASE_UNIT 
        //                FROM 
        //                    MI_MAST A 
        //                WHERE 1=1 
        //                    AND (SELECT COUNT(*) FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = '560000' ) > 0 ";

        //    if (p1 != "")
        //    {
        //        sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
        //        p.Add(":MMCODE_I", p1);
        //        p.Add(":MMNAME_E_I", p1);
        //        p.Add(":MMNAME_C_I", p1);

        //        sql += " AND (A.MMCODE LIKE :MMCODE ";
        //        p.Add(":MMCODE", string.Format("%{0}%", p1));

        //        sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
        //        p.Add(":MMNAME_E", string.Format("%{0}%", p1));

        //        sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
        //        p.Add(":MMNAME_C", string.Format("%{0}%", p1));

        //        if (p0 != "")
        //        {
        //            sql += " AND A.MAT_CLASS = :MAT_CLASS ";
        //            p.Add(":MAT_CLASS", string.Format("{0}", p0));
        //        }

        //        sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
        //    }
        //    else
        //    {
        //        sql = string.Format(sql, "");

        //        if (p0 != "")
        //        {
        //            sql += " AND A.MAT_CLASS = :MAT_CLASS ";
        //            p.Add(":MAT_CLASS", string.Format("{0}", p0));
        //        }

        //        sql += " ORDER BY A.MMCODE ";
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}


        //public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();
        //    string sql = @"SELECT 
        //                        A.MMCODE, 
        //                        A.MMNAME_C, 
        //                        A.MMNAME_E, 
        //                        A.M_CONTPRICE, 
        //                        A.BASE_UNIT,
        //                        A.MAT_CLASS
        //                    FROM 
        //                        MI_MAST A 
        //                    WHERE 1=1 
        //                        AND (SELECT COUNT(*) FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = '560000' ) > 0 ";

        //    if (query.MMCODE != "")
        //    {
        //        sql += " AND A.MMCODE LIKE :MMCODE ";
        //        p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
        //    }

        //    if (query.MMNAME_C != "")
        //    {
        //        sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
        //        p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
        //    }

        //    if (query.MMNAME_E != "")
        //    {
        //        sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
        //        p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
        //    }

        //    if (query.MAT_CLASS != "")
        //    {
        //        sql += " AND A.MAT_CLASS = :MAT_CLASS ";
        //        p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        //public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE 
        //                FROM MI_WHMAST A 
        //                WHERE 1=1 AND (SELECT COUNT(*) FROM ME_DOCM WHERE FRWH = A.WH_NO) > 0 ";

        //    if (p0 != "")
        //    {
        //        sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
        //        p.Add(":WH_NO_I", p0);
        //        p.Add(":WH_NAME_I", p0);

        //        sql += " AND (A.WH_NO LIKE :WH_NO ";
        //        p.Add(":WH_NO", string.Format("%{0}%", p0));

        //        sql += " OR A.WH_NAME LIKE :WH_NAME) ";
        //        p.Add(":WH_NAME", string.Format("%{0}%", p0));

        //        sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
        //    }
        //    else
        //    {
        //        sql = string.Format(sql, "");
        //        sql += " ORDER BY A.WH_NO ";
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        //}


        //public IEnumerable<MI_WHMAST> GetWh_no(MI_WHMAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();
        //    string sql = @"SELECT A.WH_NO,
        //                          A.WH_NAME,
        //                          (SELECT data_value || ' ' || data_desc 
        //                           FROM param_d
        //                           WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_KIND' and data_value = A.wh_kind) AS WH_KIND, 
        //                          (SELECT data_value || ' ' || data_desc
        //                           FROM param_d
        //                           WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_GRADE' and data_value = A.wh_grade) AS WH_GRADE
        //                    FROM MI_WHMAST A 
        //                    WHERE 1=1 AND (SELECT COUNT(*) FROM ME_DOCM WHERE FRWH = A.WH_NO) > 0 ";

        //    if (query.WH_NO != "")
        //    {
        //        sql += " AND A.WH_NO LIKE :WH_NO ";
        //        p.Add(":WH_NO", string.Format("%{0}%", query.WH_NO));
        //    }

        //    if (query.WH_NAME != "")
        //    {
        //        sql += " AND A.WH_NAME LIKE :WH_NAME ";
        //        p.Add(":WH_NAME", string.Format("%{0}%", query.WH_NAME));
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}


        //public class MI_MAST_QUERY_PARAMS
        //{
        //    public string MMCODE;
        //    public string MMNAME_C;
        //    public string MMNAME_E;
        //    public string MAT_CLASS;

        //    public string WH_NO;
        //    public string IS_INV;  // 需判斷庫存量>0
        //    public string E_IFPUBLIC;  // 是否公藥
        //}

        //public class MI_WHMAST_QUERY_PARAMS
        //{
        //    public string MMCODE;
        //    public string MMNAME_C;
        //    public string MMNAME_E;
        //    public string MAT_CLASS;

        //    public string WH_NO;
        //    public string WH_NAME;
        //    public string IS_INV;  // 需判斷庫存量>0
        //    public string E_IFPUBLIC;  // 是否公藥
        //}

    }
}
