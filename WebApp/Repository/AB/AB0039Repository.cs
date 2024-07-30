using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.AB
{
    public class AB0039Repository : JCLib.Mvc.BaseRepository
    {
        public AB0039Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢
        public IEnumerable<AB0039> GetAll(string mmcode1, string mmcode2, //p0,p5
                                          string insuorder1, string insuorder2, //p2,p3
                                          string agenno1, string agenno2, //p4,p5
                                          string casefrom, string sourcecode, //p6,p7
                                          string procdate1, string procdate2, //p8,p9
                                          int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT               
                        HIS_BASORDM.ORDERCODE,  --院內代碼
                        HIS_BASORDD.INSUORDERCODE,  --健保碼
                        HIS_BASORDM.ORDERENGNAME,  --英文名稱
                        HIS_BASORDM.SCIENTIFICNAME,  --成份名稱
                        HIS_BASORDM.ORDERHOSPNAME,  --院內名稱
                        HIS_BASORDM.ORDERCHINNAME,  --中文名稱
                        HIS_BASORDM.ORDEREASYNAME,  --簡稱(hp專用)
                        HIS_STKDMIT.COMMITTEECODE,  --藥委會品項
                        HIS_STKDMIT.DRUGCLASSIFY,  --藥品性質欄位(僅做藥品之分類，線上並無作用)
                        HIS_BASORDM.ORDERUSETYPE,  --醫令使用狀態
                        HIS_BASORDM.ORDERCONDCODE,  --給付條文代碼
                        HIS_BASORDM.DOHLICENSENO,  --衛生署核准字號
                        HIS_STKDMIT.MULTIPRESCRIPTIONCODE,  --藥品單複方
                        HIS_BASORDM.DRUGELEMCODE1,  --藥品成份1
                        HIS_STKDMIT.COMPONENTNUNIT,  --成份量及單位
                        HIS_BASORDM.DRUGELEMCODE2,  --藥品成份2
                        HIS_STKDMIT.COMPONENTNUNIT2,  --成份量及單位2
                        HIS_BASORDM.DRUGELEMCODE3,  --藥品成份3
                        HIS_STKDMIT.COMPONENTNUNIT3,  --成份量及單位3
                        HIS_BASORDM.DRUGELEMCODE4,  --藥品成份4
                        HIS_STKDMIT.COMPONENTNUNIT4,  --成份量及單位4
                        HIS_BASORDM.ORDERUNIT,  --醫囑單位
                        HIS_BASORDM.ORDERCHINUNIT,  --中文單位
                        HIS_BASORDD.SUPPLYNO,  --廠商代碼(供應商代碼)
                        HIS_BASORDD.CONTRACNO,  --合約碼
                        HIS_BASORDD.CASEFROM,  --標案來源
                        HIS_BASORDD.INSUAMOUNT1,  --健保價
                        HIS_BASORDD.PAYAMOUNT1,  --自費價
                        HIS_BASORDM.BUYORDERFLAG,  --買斷藥
                        HIS_BASORDM.CARRYKINDI,  --住院消耗歸整
                        HIS_BASORDM.CARRYKINDO,  --門急消耗歸整
                        HIS_BASORDM.AGGREGATECODE,  --累計用藥
                        HIS_BASORDD.ORIGINALPRODUCER,  --製造廠名稱
                        HIS_BASORDD.AGENTNAME,  --申請商名稱
                        HIS_STKDMIT.SPECNUNIT,  --規格量及單位
                        HIS_BASORDM.ATTACHUNIT,  --申報(計價)單位
                        HIS_BASORDM.STOCKUNIT,  --扣庫單位
                        HIS_BASORDM.UDSERVICEFLAG,  --UD服務
                        HIS_BASORDM.LIMITEDQTYO,  --門診倍數核發
                        HIS_BASORDM.LIMITEDQTYI,  --住院倍數核發
                        HIS_BASORDM.PATHNO,  --預設給藥途徑
                        HIS_STKDMIT.PUBLICDRUGCODE,  --公藥分類(三總藥學專用欄位)
                        HIS_STKDMIT.DRUGCLASS,  --藥品類別
                        HIS_BASORDM.RESEARCHDRUGFLAG,  --研究用藥
                        HIS_BASORDM.LIMITFLAG,  --開立限制
                        HIS_BASORDM.HOSPEXAMINEQTYFLAG,  --內審限制用量
                        HIS_BASORDM.RESTRICTTYPE,  --限制狀態
                        HIS_BASORDM.MAXTAKETIMES,  --限制次數
                        HIS_BASORDM.MAXQTYO,  --門診限制開立數量
                        HIS_BASORDM.MAXDAYSO,  --門診限制開立日數
                        HIS_BASORDM.VALIDDAYSO,  --門診效期日數
                        HIS_BASORDM.MAXQTYI,  --住院限制開立數量
                        HIS_BASORDM.MAXDAYSI,  --住院限制開立日數
                        HIS_BASORDM.VALIDDAYSI,  --住院效期日數
                        HIS_BASORDM.FIXPATHNOFLAG,  --限制途徑
                        HIS_BASORDM.TAKEKIND,  --服用藥別
                        HIS_BASORDM.ANTIBIOTICSCODE,  --抗生素等級
                        HIS_BASORDM.RESTRICTCODE,  --管制用藥
                        HIS_BASORDM.FREQNOI,  --住院給藥頻率
                        HIS_BASORDM.FREQNOO,  --門診給藥頻率
                        HIS_BASORDM.DOSE,  --預設劑量
                        HIS_BASORDM.DOSE DOSE1,  --預設劑量
                        HIS_BASORDM.ORDERABLEDRUGFORM,  --藥品劑型
                        HIS_BASORDM.RAREDISORDERFLAG,  --罕見疾病用藥
                        CASE WHEN HIS_BASORDM.SPECIALORDERKIND = 'P' THEN '是' ELSE '否' END SPECIALORDERKIND,  --外審(健保專案)用藥
                        HIS_BASORDM.HOSPEXAMINEFLAG,  --內審用藥
                        HIS_BASORDM.MAXQTYPERTIME,  --一次極量
                        HIS_BASORDM.MAXQTYPERDAY,  --一日極量
                        HIS_BASORDM.ONLYROUNDFLAG,  --不可剝半
                        HIS_BASORDM.UNABLEPOWDERFLAG,  --不可磨粉
                        HIS_BASORDM.COLDSTORAGEFLAG,  --冷藏存放
                        HIS_BASORDM.LIGHTAVOIDFLAG,  --避光存放
                        HIS_BASORDM.WEIGHTTYPE,  --體重及安全量：計算別
                        HIS_BASORDM.WEIGHTTYPE WEIGHTTYPE1,  --體重及安全量：限制數量
                        HIS_BASORDM.DANGERDRUGFLAG,  --高警訊藥品
                        HIS_BASORDM.DANGERDRUGMEMO,  --高警訊藥品提示
                        HIS_STKDMIT.DRUGEXTERIOR,  --藥品外觀
                        HIS_STKDMIT.DRUGENGEXTERIOR,  --藥品外觀(英文)
                        HIS_BASORDM.SYMPTOMCHIN,  --適應症(中文)
                        HIS_BASORDM.SYMPTOMENG,  --適應症(英文)
                        HIS_STKDMIT.CHINSIDEEFFECT,  --主要副作用(中文)
                        HIS_STKDMIT.ENGSIDEEFFECT,  --主要副作用(英文)
                        HIS_STKDMIT.CHINATTENTION,  --注意事項(中文)
                        HIS_STKDMIT.ENGATTENTION,  --注意事項(英文)
                        HIS_BASORDM.DOHLICENSENO DOHLICENSENO1,  --衛生署核准適應症
                        HIS_STKDMIT.FDASYMPTOM,  --FDA核准適應症
                        HIS_STKDMIT.DRUGMEMO,  --處方集
                        HIS_STKDMIT.SUCKLESECURITY,  --授乳安全性
                        HIS_STKDMIT.PREGNANTGRADE,  --懷孕分級
                        HIS_STKDMIT.DRUGPICTURELINK,  --藥品圖片檔名
                        HIS_STKDMIT.DRUGLEAFLETLINK,  --藥品仿單檔名
                        HIS_BASORDM.TDMFLAG,  --TDM 藥品
                        HIS_BASORDM.TDMFLAG TDMFLAG1,  --TDM 合理治療濃度上限
                        HIS_BASORDM.TDMFLAG TDMFLAG2,  --TDM 合理治療濃度下限
                        HIS_BASORDM.TDMFLAG TDMFLAG3,  --TDM 合理PEAK起
                        HIS_BASORDM.TDMFLAG TDMFLAG4,  --TDM 合理PEAK迄
                        HIS_BASORDM.TDMFLAG TDMFLAG5,  --TDM 合理 Trough 起
                        HIS_BASORDM.TDMFLAG TDMFLAG6,  --TDM 合理 Trough 迄
                        HIS_BASORDM.TDMFLAG TDMFLAG7,  --TDM 危急值 起
                        HIS_BASORDM.TDMFLAG TDMFLAG8,  --TDM 危急值 迄
                        HIS_BASORDM.TDMFLAG TDMFLAG9,  --TDM 備註1
                        HIS_BASORDM.TDMFLAG TDMFLAG10,  --TDM 備註2
                        HIS_BASORDM.TDMFLAG TDMFLAG11,  --TDM備 註3
                        HIS_BASORDM.UDPOWDERFLAG,  --UD磨粉
                        HIS_BASORDM.MACHINEFLAG,  --藥包機品項
                        HIS_BASORDM.DRUGPARENTCODE1,  --成份母層代碼1
                        HIS_BASORDM.DRUGPARENTCODE2,  --成份母層代碼2
                        HIS_BASORDM.DRUGPARENTCODE3,  --成份母層代碼3
                        HIS_BASORDM.DRUGPARENTCODE4,  --成份母層代碼4
                        HIS_STKDMIT.DRUGPACKAGE,  --藥品包裝
                        HIS_BASORDM.ATCCODE1,  --藥理分類ATC1
                        HIS_BASORDM.ATCCODE1 ATCCODE2,  --藥理分類AHFS1
                        HIS_BASORDM.ATCCODE1 ATCCODE3,  --藥理分類ATC2
                        HIS_BASORDM.ATCCODE1 ATCCODE4,  --藥理分類AHFS2
                        HIS_BASORDM.ATCCODE1 ATCCODE5,  --藥理分類ATC3
                        HIS_BASORDM.ATCCODE1 ATCCODE6,  --藥理分類AHFS3
                        HIS_BASORDM.ATCCODE1 ATCCODE7,  --藥理分類ATC4
                        HIS_BASORDM.ATCCODE1 ATCCODE8,  --藥理分類AHFS4
                        HIS_BASORDM.GERIATRIC,  --老年人劑量調整
                        HIS_BASORDM.LIVERLIMITED,  --肝功能不良需調整劑量
                        HIS_BASORDM.RENALLIMITED,  --腎功能不良需調整劑量
                        HIS_BASORDM.BIOLOGICALAGENT,  --生物製劑
                        HIS_BASORDM.BLOODPRODUCT,  --血液製劑
                        HIS_BASORDM.FREEZING,  --是否需冷凍
                        CASE WHEN HIS_BASORDM.RETURNDRUGFLAG= 'Y' THEN '是' ELSE '否' END RETURNDRUGFLAG --CDC藥品
                        FROM HIS_BASORDM, HIS_BASORDD, HIS_STKDMIT
                        WHERE HIS_BASORDD.ORDERCODE = HIS_BASORDM.ORDERCODE
                        AND   HIS_STKDMIT.SKORDERCODE = HIS_BASORDM.ORDERCODE
                        AND   TWN_SYSDATE <= HIS_BASORDD.ENDDATE
                        AND   TWN_SYSDATE >= HIS_BASORDD.BEGINDATE

                        ";
            //p0,p5
            if (mmcode1 != "" & mmcode2 != "")
            {
                sql += " AND HIS_BASORDM.ORDERCODE IS NOT NULL AND HIS_BASORDM.ORDERCODE BETWEEN :p0 AND :p5 ";
                p.Add(":p0", string.Format("{0}", mmcode1));
                p.Add(":p5", string.Format("{0}", mmcode2));
            }
            if (mmcode1 != "" & mmcode2 == "")
            {
                sql += " AND HIS_BASORDM.ORDERCODE = :p0 ";
                p.Add(":p0", string.Format("{0}", mmcode1));
            }
            if (mmcode1 == "" & mmcode2 != "")
            {
                sql += " AND HIS_BASORDM.ORDERCODE= :p5 ";
                p.Add(":p5", string.Format("{0}", mmcode2));
            }
            //p2,p3
            if (insuorder1 != "" & insuorder2 != "")
            {
                sql += " AND HIS_BASORDD.INSUORDERCODE IS NOT NULL AND HIS_BASORDD.INSUORDERCODE BETWEEN :p2 AND :p3 ";
                p.Add(":p2", string.Format("{0}", insuorder1));
                p.Add(":p3", string.Format("{0}", insuorder2));
            }
            if (insuorder1 != "" & insuorder2 == "")
            {
                sql += " AND HIS_BASORDD.INSUORDERCODE = :p2 ";
                p.Add(":p2", string.Format("{0}", insuorder1));
            }
            if (insuorder1 == "" & insuorder2 != "")
            {
                sql += " AND HIS_BASORDD.INSUORDERCODE= :p3 ";
                p.Add(":p3", string.Format("{0}", insuorder2));
            }
            //p4,p5
            if (agenno1 != "" & agenno2 != "")
            {
                sql += " AND HIS_BASORDD.SUPPLYNO IS NOT NULL AND HIS_BASORDD.SUPPLYNO BETWEEN :p4 AND :p5 ";
                p.Add(":p4", string.Format("{0}", agenno1));
                p.Add(":p5", string.Format("{0}", agenno2));
            }
            if (agenno1 != "" & agenno2 == "")
            {
                sql += " AND HIS_BASORDD.SUPPLYNO = :p4 ";
                p.Add(":p4", string.Format("{0}", agenno1));
            }
            if (agenno1 == "" & agenno2 != "")
            {
                sql += " AND HIS_BASORDD.SUPPLYNO= :p5 ";
                p.Add(":p5", string.Format("{0}", agenno2));
            }
            //p6
            if (casefrom != "")
            {
                sql += " AND HIS_BASORDD.CASEFROM = :p6 ";
                p.Add(":p6", string.Format("{0}", casefrom));
            }
            //p7
            if (sourcecode != "")
            {
                sql += " AND HIS_STKDMIT.StockSourceCode = :p7 ";
                p.Add(":p7", string.Format("{0}", sourcecode));
            }
            //p8,p9
            if (procdate1 != "" & procdate2 != "")
            {
                sql += " AND TWN_DATE(HIS_BASORDM.ProcDateTime) BETWEEN :p8 AND :p9 ";
                p.Add(":p8", string.Format("{0}", procdate1));
                p.Add(":p9", string.Format("{0}", procdate2));
            }
            if (procdate1 != "" & procdate2 == "")
            {
                sql += " AND TWN_DATE(A.procdate) >= :p8 ";
                p.Add(":p8", string.Format("{0}", procdate1));
            }
            if (procdate1 == "" & procdate2 != "")
            {
                sql += " AND TWN_DATE(A.procdate) <= :p9 ";
                p.Add(":p9", string.Format("{0}", procdate2));
            }
            
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0039>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        

        //匯出
        public DataTable GetExcel(string mmcode1, string mmcode2, //p0,p5
                                          string insuorder1, string insuorder2, //p2,p3
                                          string agenno1, string agenno2, //p4,p5
                                          string casefrom, string sourcecode, //p6,p7
                                          string procdate1, string procdate2)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT               
                        HIS_BASORDM.ORDERCODE AS 院內代碼,
                        HIS_BASORDD.INSUORDERCODE AS 健保碼,
                        HIS_BASORDM.ORDERENGNAME AS 英文名稱,
                        HIS_BASORDM.SCIENTIFICNAME AS 成份名稱,
                        HIS_BASORDM.ORDERHOSPNAME AS 院內名稱,
                        HIS_BASORDM.ORDERCHINNAME AS 中文名稱,
                        HIS_BASORDM.ORDEREASYNAME AS 簡稱_hp專用,
                        HIS_STKDMIT.COMMITTEECODE AS 藥委會品項,
                        HIS_STKDMIT.DRUGCLASSIFY AS 藥品性質欄位_僅做藥品之分類_線上並無作用,
                        HIS_BASORDM.ORDERUSETYPE AS 醫令使用狀態,
                        HIS_BASORDM.ORDERCONDCODE AS 給付條文代碼,
                        HIS_BASORDM.DOHLICENSENO AS 衛生署核准字號,
                        HIS_STKDMIT.MULTIPRESCRIPTIONCODE AS 藥品單複方,
                        HIS_BASORDM.DRUGELEMCODE1 AS 藥品成份1,
                        HIS_STKDMIT.COMPONENTNUNIT AS 成份量及單位,
                        HIS_BASORDM.DRUGELEMCODE2 AS 藥品成份2,
                        HIS_STKDMIT.COMPONENTNUNIT2 AS 成份量及單位2,
                        HIS_BASORDM.DRUGELEMCODE3 AS 藥品成份3,
                        HIS_STKDMIT.COMPONENTNUNIT3 AS 成份量及單位3,
                        HIS_BASORDM.DRUGELEMCODE4 AS 藥品成份4,
                        HIS_STKDMIT.COMPONENTNUNIT4 AS 成份量及單位4,
                        HIS_BASORDM.ORDERUNIT AS 醫囑單位,
                        HIS_BASORDM.ORDERCHINUNIT AS 中文單位,
                        HIS_BASORDD.SUPPLYNO AS 廠商代碼_供應商代碼,
                        HIS_BASORDD.CONTRACNO AS 合約碼,
                        HIS_BASORDD.CASEFROM AS 標案來源,
                        HIS_BASORDD.INSUAMOUNT1 AS 健保價,
                        HIS_BASORDD.PAYAMOUNT1 AS 自費價,
                        HIS_BASORDM.BUYORDERFLAG AS 買斷藥,
                        HIS_BASORDM.CARRYKINDI AS 住院消耗歸整,
                        HIS_BASORDM.CARRYKINDO AS 門急消耗歸整,
                        HIS_BASORDM.AGGREGATECODE AS 累計用藥,
                        HIS_BASORDD.ORIGINALPRODUCER AS 製造廠名稱,
                        HIS_BASORDD.AGENTNAME AS 申請商名稱,
                        HIS_STKDMIT.SPECNUNIT AS 規格量及單位,
                        HIS_BASORDM.ATTACHUNIT AS 申報_計價單位,
                        HIS_BASORDM.STOCKUNIT AS 扣庫單位,
                        HIS_BASORDM.UDSERVICEFLAG AS UD服務,
                        HIS_BASORDM.LIMITEDQTYO AS 門診倍數核發,
                        HIS_BASORDM.LIMITEDQTYI AS 住院倍數核發,
                        HIS_BASORDM.PATHNO AS 預設給藥途徑,
                        HIS_STKDMIT.PUBLICDRUGCODE AS 公藥分類_三總藥學專用欄位,
                        HIS_STKDMIT.DRUGCLASS AS 藥品類別,
                        HIS_BASORDM.RESEARCHDRUGFLAG AS 研究用藥,
                        HIS_BASORDM.LIMITFLAG AS 開立限制,
                        HIS_BASORDM.HOSPEXAMINEQTYFLAG AS 內審限制用量,
                        HIS_BASORDM.RESTRICTTYPE AS 限制狀態,
                        HIS_BASORDM.MAXTAKETIMES AS 限制次數,
                        HIS_BASORDM.MAXQTYO AS 門診限制開立數量,
                        HIS_BASORDM.MAXDAYSO AS 門診限制開立日數,
                        HIS_BASORDM.VALIDDAYSO AS 門診效期日數,
                        HIS_BASORDM.MAXQTYI AS 住院限制開立數量,
                        HIS_BASORDM.MAXDAYSI AS 住院限制開立日數,
                        HIS_BASORDM.VALIDDAYSI AS 住院效期日數,
                        HIS_BASORDM.FIXPATHNOFLAG AS 限制途徑,
                        HIS_BASORDM.TAKEKIND AS 服用藥別,
                        HIS_BASORDM.ANTIBIOTICSCODE AS 抗生素等級,
                        HIS_BASORDM.RESTRICTCODE AS 管制用藥,
                        HIS_BASORDM.FREQNOI AS 住院給藥頻率,
                        HIS_BASORDM.FREQNOO AS 門診給藥頻率,
                        HIS_BASORDM.DOSE AS 預設劑量,
                        HIS_BASORDM.DOSE AS 預設劑量,
                        HIS_BASORDM.ORDERABLEDRUGFORM AS 藥品劑型,
                        HIS_BASORDM.RAREDISORDERFLAG AS 罕見疾病用藥,
                        CASE WHEN HIS_BASORDM.SPECIALORDERKIND = 'P' THEN '是' ELSE '否' END AS 外審_健保專案用藥,
                        HIS_BASORDM.HOSPEXAMINEFLAG AS 內審用藥,
                        HIS_BASORDM.MAXQTYPERTIME AS 一次極量,
                        HIS_BASORDM.MAXQTYPERDAY AS 一日極量,
                        HIS_BASORDM.ONLYROUNDFLAG AS 不可剝半,
                        HIS_BASORDM.UNABLEPOWDERFLAG AS 不可磨粉,
                        HIS_BASORDM.COLDSTORAGEFLAG AS 冷藏存放,
                        HIS_BASORDM.LIGHTAVOIDFLAG AS 避光存放,
                        HIS_BASORDM.WEIGHTTYPE AS 體重及安全量_計算別,
                        HIS_BASORDM.WEIGHTTYPE AS 體重及安全量_限制數量,
                        HIS_BASORDM.DANGERDRUGFLAG AS 高警訊藥品,
                        HIS_BASORDM.DANGERDRUGMEMO AS 高警訊藥品提示,
                        HIS_STKDMIT.DRUGEXTERIOR AS 藥品外觀,
                        HIS_STKDMIT.DRUGENGEXTERIOR AS 藥品外觀_英文,
                        HIS_BASORDM.SYMPTOMCHIN AS 適應症_中文,
                        HIS_BASORDM.SYMPTOMENG AS 適應症_英文,
                        HIS_STKDMIT.CHINSIDEEFFECT AS 主要副作用_中文,
                        HIS_STKDMIT.ENGSIDEEFFECT AS 主要副作用_英文,
                        HIS_STKDMIT.CHINATTENTION AS 注意事項_中文,
                        HIS_STKDMIT.ENGATTENTION AS 注意事項_英文,
                        HIS_BASORDM.DOHLICENSENO AS 衛生署核准適應症,
                        HIS_STKDMIT.FDASYMPTOM AS FDA核准適應症,
                        HIS_STKDMIT.DRUGMEMO AS 處方集,
                        HIS_STKDMIT.SUCKLESECURITY AS 授乳安全性,
                        HIS_STKDMIT.PREGNANTGRADE AS 懷孕分級,
                        HIS_STKDMIT.DRUGPICTURELINK AS 藥品圖片檔名,
                        HIS_STKDMIT.DRUGLEAFLETLINK AS 藥品仿單檔名,
                        HIS_BASORDM.TDMFLAG AS TDM藥品,
                        HIS_BASORDM.TDMFLAG AS TDM合理治療濃度上限,
                        HIS_BASORDM.TDMFLAG AS TDM合理治療濃度下限,
                        HIS_BASORDM.TDMFLAG AS TDM合理PEAK起,
                        HIS_BASORDM.TDMFLAG AS TDM合理PEAK迄,
                        HIS_BASORDM.TDMFLAG AS TDM合理Trough起,
                        HIS_BASORDM.TDMFLAG AS TDM合理Trough迄,
                        HIS_BASORDM.TDMFLAG AS TDM危急值起,
                        HIS_BASORDM.TDMFLAG AS TDM危急值迄,
                        HIS_BASORDM.TDMFLAG AS TDM備註1,
                        HIS_BASORDM.TDMFLAG AS TDM備註2,
                        HIS_BASORDM.TDMFLAG AS TDM備註3,
                        HIS_BASORDM.UDPOWDERFLAG AS UD磨粉,
                        HIS_BASORDM.MACHINEFLAG AS 藥包機品項,
                        HIS_BASORDM.DRUGPARENTCODE1 AS 成份母層代碼1,
                        HIS_BASORDM.DRUGPARENTCODE2 AS 成份母層代碼2,
                        HIS_BASORDM.DRUGPARENTCODE3 AS 成份母層代碼3,
                        HIS_BASORDM.DRUGPARENTCODE4 AS 成份母層代碼4,
                        HIS_STKDMIT.DRUGPACKAGE AS 藥品包裝,
                        HIS_BASORDM.ATCCODE1 AS 藥理分類ATC1,
                        HIS_BASORDM.ATCCODE1 AS 藥理分類AHFS1,
                        HIS_BASORDM.ATCCODE1 AS 藥理分類ATC2,
                        HIS_BASORDM.ATCCODE1 AS 藥理分類AHFS2,
                        HIS_BASORDM.ATCCODE1 AS 藥理分類ATC3,
                        HIS_BASORDM.ATCCODE1 AS 藥理分類AHFS3,
                        HIS_BASORDM.ATCCODE1 AS 藥理分類ATC4,
                        HIS_BASORDM.ATCCODE1 AS 藥理分類AHFS4,
                        HIS_BASORDM.GERIATRIC AS 老年人劑量調整,
                        HIS_BASORDM.LIVERLIMITED AS 肝功能不良需調整劑量,
                        HIS_BASORDM.RENALLIMITED AS 腎功能不良需調整劑量,
                        HIS_BASORDM.BIOLOGICALAGENT AS 生物製劑,
                        HIS_BASORDM.BLOODPRODUCT AS 血液製劑,
                        HIS_BASORDM.FREEZING AS 是否需冷凍,
                        CASE WHEN HIS_BASORDM.RETURNDRUGFLAG= 'Y' THEN '是' ELSE '否' END AS CDC藥品
                        FROM HIS_BASORDM, HIS_BASORDD, HIS_STKDMIT
                        WHERE HIS_BASORDD.ORDERCODE = HIS_BASORDM.ORDERCODE
                        AND   HIS_STKDMIT.SKORDERCODE = HIS_BASORDM.ORDERCODE
                        AND   TWN_SYSDATE <= HIS_BASORDD.ENDDATE
                        AND   TWN_SYSDATE >= HIS_BASORDD.BEGINDATE

                        ";
            //p0,p5
            if (mmcode1 != "" & mmcode2 != "")
            {
                sql += " AND HIS_BASORDM.ORDERCODE BETWEEN :p0 AND :p5 ";
                p.Add(":p0", string.Format("{0}", mmcode1));
                p.Add(":p5", string.Format("{0}", mmcode2));
            }
            if (mmcode1 != "" & mmcode2 == "")
            {
                sql += " AND HIS_BASORDM.ORDERCODE = :p0 ";
                p.Add(":p0", string.Format("{0}", mmcode1));
            }
            if (mmcode1 == "" & mmcode2 != "")
            {
                sql += " AND HIS_BASORDM.ORDERCODE= :p5 ";
                p.Add(":p5", string.Format("{0}", mmcode2));
            }
            //p2,p3
            if (insuorder1 != "" & insuorder2 != "")
            {
                sql += " AND HIS_BASORDD.INSUORDERCODE BETWEEN :p0 AND :p5 ";
                p.Add(":p0", string.Format("{0}", insuorder1));
                p.Add(":p5", string.Format("{0}", insuorder2));
            }
            if (insuorder1 != "" & insuorder2 == "")
            {
                sql += " AND HIS_BASORDD.INSUORDERCODE = :p0 ";
                p.Add(":p0", string.Format("{0}", insuorder1));
            }
            if (insuorder1 == "" & insuorder2 != "")
            {
                sql += " AND HIS_BASORDD.INSUORDERCODE= :p5 ";
                p.Add(":p5", string.Format("{0}", insuorder2));
            }
            //p4,p5
            if (agenno1 != "" & agenno2 != "")
            {
                sql += " AND HIS_BASORDD.SUPPLYNO BETWEEN :p0 AND :p5 ";
                p.Add(":p0", string.Format("{0}", agenno1));
                p.Add(":p5", string.Format("{0}", agenno2));
            }
            if (agenno1 != "" & agenno2 == "")
            {
                sql += " AND HIS_BASORDD.SUPPLYNO = :p0 ";
                p.Add(":p0", string.Format("{0}", agenno1));
            }
            if (agenno1 == "" & agenno2 != "")
            {
                sql += " AND HIS_BASORDD.SUPPLYNO= :p5 ";
                p.Add(":p5", string.Format("{0}", agenno2));
            }
            //p6
            if (casefrom != "")
            {
                sql += " AND HIS_BASORDD.CASEFROM = :p6 ";
                p.Add(":p6", string.Format("{0}", casefrom));
            }
            //p7
            if (sourcecode != "")
            {
                sql += " AND HIS_STKDMIT.StockSourceCode = :p7 ";
                p.Add(":p7", string.Format("{0}", sourcecode));
            }
            //p8,p9
            if (procdate1 != "" & procdate2 != "")
            {
                sql += " AND TWN_DATE(HIS_BASORDM.ProcDateTime) BETWEEN :p8 AND :p9 ";
                p.Add(":p8", string.Format("{0}", procdate1));
                p.Add(":p9", string.Format("{0}", procdate2));
            }
            if (procdate1 != "" & procdate2 == "")
            {
                sql += " AND TWN_DATE(A.procdate) >= :p8 ";
                p.Add(":p8", string.Format("{0}", procdate1));
            }
            if (procdate1 == "" & procdate2 != "")
            {
                sql += " AND TWN_DATE(A.procdate) <= :p9 ";
                p.Add(":p9", string.Format("{0}", procdate2));
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //院內碼下拉式選單
        public IEnumerable<COMBO_MODEL> GetOrdercodeCombo()
        {
            string sql = @"SELECT ORDERCODE AS VALUE, ORDERCODE AS TEXT, ORDERCODE AS COMBITEM 
                            FROM HIS_BASORDM 
                            WHERE ORDERTYPE LIKE 'D%'
                            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        //廠商代碼下拉式選單
        public IEnumerable<COMBO_MODEL> GetAgennoCombo()
        {
            string sql = @"SELECT AGEN_NO AS VALUE,AGEN_NAMEC AS TEXT, 
                            AGEN_NO || ' ' || AGEN_NAMEC AS COMBITEM
                            FROM PH_VENDER
                            ORDER BY AGEN_NO
                            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        //健保碼下拉式選單
        public IEnumerable<COMBO_MODEL> GetInsuOrderCombo()
        {
            string sql = @"SELECT DISTINCT INSUORDERCODE AS VALUE,INSUORDERCODE AS TEXT, 
                            INSUORDERCODE AS COMBITEM
                            FROM HIS_BASORDD
                            WHERE 1 = 1
                            ORDER BY INSUORDERCODE
                            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        //標案來源下拉式選單
        public IEnumerable<COMBO_MODEL> GetCaseFromCombo()
        {
            string sql = @"SELECT DATA_VALUE AS VALUE,DATA_DESC AS TEXT, 
                            DATA_VALUE || ' ' || DATA_DESC AS COMBITEM
                            FROM  PARAM_D 
                            WHERE GRP_CODE = 'HIS_BASORDD'
                            AND DATA_NAME = 'CASEFROM'
                            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        //來源代碼下拉式選單
        public IEnumerable<COMBO_MODEL> GetSourceCodeCombo()
        {
            string sql = @"SELECT DATA_VALUE AS VALUE,DATA_DESC AS TEXT, 
                            DATA_VALUE || ' ' || DATA_DESC AS COMBITEM
                            FROM  PARAM_D 
                            WHERE GRP_CODE = 'MI_MAST'
                            AND DATA_NAME = 'E_SOURCECODE'
                            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
    }
}