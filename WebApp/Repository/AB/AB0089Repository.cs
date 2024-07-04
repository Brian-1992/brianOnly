using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0089Repository : JCLib.Mvc.BaseRepository
    {
        public AB0089Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0089_1> GetAll_1(string p0, string p1, string p2, string p3, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select 
                OrderCode, ProcDateTime, ProcOpID, OrderEngName, OrderChinName, 
                ScientificName, OrderUnit, OrderChinUnit, AttachUnit, StockUnit, 
                SkOrderCode, UDServiceFlag,TakeKind, LimitedQtyO, LimitedQtyI,
                BuyOrderFlag,  OpenDate, PublicDrugFlag, StartDate, OrderHospName, 
                OrderEasyName, OrderDCFlag, MaxQtyPerTime, MaxQtyPerDay, MaxTakeTimes, 
                DOHLicenseNo, RFIDCode, PathNo, AggregateCode, LimitFlag, 
                RestrictCode, AntibioticsCode,CarryKindI, CarryKindO, UDPowderFlag, 
                ReturnDrugFlag, ResearchDrugFlag, MachineFlag, FixPathNoFlag,  SymptomChin, 
                SymptomEng, OnlyRoundFlag, UnablePowderFlag,DangerDrugFlag, DangerDrugMemo, 
                ColdStorageFlag, LightAvoidFlag, ChangeStatus, FreqNoO, FreqNoI, 
                OrderDays, Dose, HospChargeId1, OrderType, OrderKind,
                HighPriceFlag,InpDisplayFlag, Substitute1, Substitute2, Substitute3, 
                Substitute4, Substitute5, WeightType, WeightUnitLimit, RestrictType,
                MaxQtyO,MaxQtyI,MaxDaysO, MaxDaysI, ValidDaysO, ValidDaysI, 
                OrderCodeSort, DrugElemCode1, DrugElemCode2, DrugElemCode3, DrugElemCode4,
                TDMFlag, SpecialOrderKind,NeedRegionFlag, OrderUseType, FixDoseFlag, 
                RareDisorderFlag, HospExamineFlag, OrderCondCode, HospExamineQtyFlag, CreateDateTime, 
                CreateOpID
                from HIS_BASORDM
                where 1 = 1 ";
            var p01 = "";
            var p23 = "";
            if (p2 != "" )
            {
                p01 = p0 + p1.Replace(":", "") + "00";
            }
            else
            {
                p01 = p0 + "000000";
            }
            if (p3 != "")
            {
                p23 = p2 + p3.Replace(":", "") + "00";
            }
            else
            {
                p23 = p1 + "000000";
            }
            if (p0 != "" & p1 != "")
            {
                sql += " AND ProcDateTime BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p01));
                p.Add(":p1", string.Format("{0}", p23));
            }
            if (p0 != "" & p1 == "")
            {
                sql += " AND ProcDateTime >= :p0 ";
                p.Add(":p0", string.Format("{0}", p01));
            }
            if (p0 == "" & p1 != "")
            {
                sql += " AND ProcDateTime <= :p1 ";
                p.Add(":p1", string.Format("{0}", p23));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0089_1>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public IEnumerable<AB0089_2> GetAll_2(string p0, string p1, string p2, string p3, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select 
                OrderCode, ProcDateTime, ProcOpID, BeginDate, EndDate,
                 StockTransQtyO, StockTransQtyI,0 ChangeQtyI, 0 ChangeQtyO, 0 Price1,
                0 Price2, CostAmount, ''IsDisc, 0 DiscPer,InsuOrderCode, 
                InsuSignI, InsuSignO, ContractPrice, ContracNo, 
                SupplyNo, CaseFrom, OriginalProducer, AgentName, 
                CreateDateTime, CreateOpID
                from HIS_BASORDD where 1 = 1 ";
            var p01 = "";
            var p23 = "";
            if (p2 != "")
            {
                p01 = p0 + p1.Replace(":", "") + "00";
            }
            else
            {
                p01 = p0 + "000000";
            }
            if (p3 != "")
            {
                p23 = p2 + p3.Replace(":", "") + "00";
            }
            else
            {
                p23 = p1 + "000000";
            }
            if (p0 != "" & p1 != "")
            {
                sql += " AND ProcDateTime BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p01));
                p.Add(":p1", string.Format("{0}", p23));
            }
            if (p0 != "" & p1 == "")
            {
                sql += " AND ProcDateTime >= :p0 ";
                p.Add(":p0", string.Format("{0}", p01));
            }
            if (p0 == "" & p1 != "")
            {
                sql += " AND ProcDateTime <= :p1 ";
                p.Add(":p1", string.Format("{0}", p23));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0089_2>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0089_3> GetAll_3(string p0, string p1, string p2, string p3, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select
                SkOrderCode, ProcDateTime, ProcOpID, DCMassageCode, DMITDCCode, 
                Manufacturer, WCostAmount, PublicDrugCode, StockUseCode, SpecNUnit,
                ComponentNUnit, YearArmyNo, ItemArmyNo, GroupArmyNo, ClassifiedArmyNo, 
                ContractEffectiveDate, MultiPrescriptionCode,DrugClass, DrugClassify, DrugForm, 
                CommitteeMemo, CommitteeCode, InventoryFlag, ApplyUnit, PurchaseCaseType, 
                MaxCureConsistency, MinCureConsistency, PearBegin, PearEnd, TroughBegin, 
                TroughEnd, DangerBegin, DangerEnd, TDMMemo1, TDMMemo2, 
                TDMMemo3, ChinAttention, EngAttention, DrugMemo, ChinSideEffect, 
                EngSideEffect, Warn, DOHSymptom, FDASymptom, SuckleSecurity, 
                PregnantGrade, DrugExterior, PurchaseUnit, DCMassageMemo, FirstPurchaseDate, 
                DrugLeafletLink, DrugPictureLink, ComponentNUnit2, ComponentNUnit3, ComponentNUnit4, 
                DrugEngExterior, ArmyOrderCode, DrugApplyType,ParentCode, ParentOrderCode, 
                SonTransQty, CreateDateTime, CreateOpID
                from HIS_STKDMIT
                where 1=1 ";

            var p01 = "";
            var p23 = "";
            if (p2 != "")
            {
                p01 = p0 + p1.Replace(":", "") + "00";
            }
            else
            {
                p01 = p0 + "000000";
            }
            if (p3 != "")
            {
                p23 = p2 + p3.Replace(":", "") + "00";
            }
            else
            {
                p23 = p1 + "000000";
            }
            if (p0 != "" & p1 != "")
            {
                sql += " AND ProcDateTime BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p01));
                p.Add(":p1", string.Format("{0}", p23));
            }
            if (p0 != "" & p1 == "")
            {
                sql += " AND ProcDateTime >= :p0 ";
                p.Add(":p0", string.Format("{0}", p01));
            }
            if (p0 == "" & p1 != "")
            {
                sql += " AND ProcDateTime <= :p1 ";
                p.Add(":p1", string.Format("{0}", p23));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0089_3>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        
        public DataTable GetExcel_T1(string p0, string p1, string p2, string p3)
        {
            DynamicParameters p = new DynamicParameters();


            var sql = @"select 
                OrderCode 院內代碼, ProcDateTime 記錄處理日期時間, ProcOpID 記錄處理人員, OrderEngName 英文名稱, OrderChinName 中文名稱, 
                ScientificName 成份名稱, OrderUnit 醫囑單位, OrderChinUnit 中文單位, AttachUnit 申報單位, StockUnit 扣庫單位,  
                SkOrderCode 藥品藥材代碼, UDServiceFlag UD服務,TakeKind 服用藥別, LimitedQtyO 門診倍數核發, LimitedQtyI 住院倍數核發,
                BuyOrderFlag 買斷藥,  OpenDate 系統啟用日期, PublicDrugFlag 公藥否, StartDate 開始日期, OrderHospName 院內名稱,  
                OrderEasyName 簡稱, OrderDCFlag 停用碼, MaxQtyPerTime 一次極量, MaxQtyPerDay 一日極量, MaxTakeTimes 限制次數, 
                DOHLicenseNo 衛生署核准字號, RFIDCode RFID條碼, PathNo 預設給藥途徑, AggregateCode 累計用藥, LimitFlag 開立限制, 
                RestrictCode 管制用藥, AntibioticsCode 抗生素等級,CarryKindI 住院消耗歸整, CarryKindO 門急消耗歸整, UDPowderFlag UD磨粉, 
                ReturnDrugFlag 合理回流藥, ResearchDrugFlag 研究用藥, MachineFlag 藥包機品項, FixPathNoFlag 限制途徑,  SymptomChin 適應症_中文, 
                SymptomEng 適應症_英文, OnlyRoundFlag 不可剝半, UnablePowderFlag 不可磨粉,DangerDrugFlag 高警訊藥品, DangerDrugMemo 高警訊藥品提示, 
                ColdStorageFlag 冷藏存放, LightAvoidFlag 避光存放, ChangeStatus 異動狀態, FreqNoO 門診給藥頻率, FreqNoI 住院給藥頻率, 
                OrderDays 預設開立天數, Dose 預設劑量, HospChargeId1 院內費用類別, OrderType 醫令類別, OrderKind 醫令類別_申報定義,
                HighPriceFlag 高價用藥,InpDisplayFlag 住院醫囑顯示, Substitute1 替代院內代碼1, Substitute2 替代院內代碼2, Substitute3 替代院內代碼3, 
                Substitute4 替代院內代碼4, Substitute5 替代院內代碼5, WeightType 體重及安全量_計算別, WeightUnitLimit 體重及安全量_限制數量, RestrictType 限制狀態,
                MaxQtyO 門診限制開立數量,MaxQtyI 住院限制開立數量,MaxDaysO 門診限制開立日數, MaxDaysI 住院限制開立日數, ValidDaysO 門診效期日數, ValidDaysI 住院效期日數, 
                OrderCodeSort 醫令排序, DrugElemCode1 藥品成份1, DrugElemCode2 藥品成份2, DrugElemCode3 藥品成份3, DrugElemCode4 藥品成份4,
                TDMFlag TDM藥品, SpecialOrderKind 外審_健保專案_用藥,NeedRegionFlag 處置需報部位, OrderUseType 醫令使用狀態, FixDoseFlag 預設劑量, 
                RareDisorderFlag 罕見疾病用藥, HospExamineFlag 內審用藥, OrderCondCode 給付條文代碼, HospExamineQtyFlag 內審限制用量, CreateDateTime 記錄建立日期時間, 
                CreateOpID 記錄建立人員
                from HIS_BASORDM
                where 1 = 1 ";

            var p01 = "";
            var p23 = "";
            if (p2 != "")
            {
                p01 = p0 + p1.Replace(":", "") + "00";
            }
            else
            {
                p01 = p0 + "000000";
            }
            if (p3 != "")
            {
                p23 = p2 + p3.Replace(":", "") + "00";
            }
            else
            {
                p23 = p1 + "000000";
            }
            if (p0 != "" & p1 != "")
            {
                sql += " AND ProcDateTime BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p01));
                p.Add(":p1", string.Format("{0}", p23));
            }
            if (p0 != "" & p1 == "")
            {
                sql += " AND ProcDateTime >= :p0 ";
                p.Add(":p0", string.Format("{0}", p01));
            }
            if (p0 == "" & p1 != "")
            {
                sql += " AND ProcDateTime <= :p1 ";
                p.Add(":p1", string.Format("{0}", p23));
            }

            sql += @"  ORDER BY ProcDateTime";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel_T2(string p0, string p1, string p2, string p3)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"select 
                    OrderCode 院內代碼, ProcDateTime 記錄處理日期時間, ProcOpID 記錄處理人員, BeginDate 生效起日_開始日期, EndDate 生效迄日_結束日期,
                    StockTransQtyO 門診扣庫轉換量, StockTransQtyI 住院扣庫轉換量,0 門診申報轉換量, 0 住院申報轉換量, 0 健保價,
                    0 自費價, CostAmount 進價, '' 是否優惠, 0 優惠百分比,InsuOrderCode 健保碼, 
                    InsuSignI 健保負擔碼_住院, InsuSignO 健保負擔碼_門診, ContractPrice 合約單價, ContracNo 合約碼, 
                    SupplyNo 廠商代碼_供應商代碼, CaseFrom 標案來源, OriginalProducer 製造廠名稱, AgentName 申請商名稱, 
                    CreateDateTime 記錄建立日期時間, CreateOpID 記錄建立人員 
                    from HIS_BASORDD where 1 = 1 ";

            var p01 = "";
            var p23 = "";
            if (p2 != "")
            {
                p01 = p0 + p1.Replace(":", "") + "00";
            }
            else
            {
                p01 = p0 + "000000";
            }
            if (p3 != "")
            {
                p23 = p2 + p3.Replace(":", "") + "00";
            }
            else
            {
                p23 = p1 + "000000";
            }
            if (p0 != "" & p1 != "")
            {
                sql += " AND ProcDateTime BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p01));
                p.Add(":p1", string.Format("{0}", p23));
            }
            if (p0 != "" & p1 == "")
            {
                sql += " AND ProcDateTime >= :p0 ";
                p.Add(":p0", string.Format("{0}", p01));
            }
            if (p0 == "" & p1 != "")
            {
                sql += " AND ProcDateTime <= :p1 ";
                p.Add(":p1", string.Format("{0}", p23));
            }

            sql += @" ORDER BY ProcDateTime";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel_T3(string p0, string p1, string p2, string p3)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @"select
                SkOrderCode 藥品藥材代碼, ProcDateTime 記錄處理日期時間, ProcOpID 記錄處理人員, DCMassageCode 藥品停用通知, DMITDCCode 藥品進貨異動, 
                Manufacturer 廠牌, WCostAmount 移動平均加權價weighted_CostAmount, PublicDrugCode 公藥分類_三總藥學專用欄位, StockUseCode 扣庫規則分類_三總藥學專用欄位, SpecNUnit 規格量及單位,
                ComponentNUnit 成份量及單位, YearArmyNo 軍聯項次年號, ItemArmyNo 軍聯項次號, GroupArmyNo 軍聯項次分類, ClassifiedArmyNo 軍聯項次組別, 
                ContractEffectiveDate 合約效期, MultiPrescriptionCode 藥品單複方,DrugClass 藥品類別, DrugClassify 藥品性質欄位_僅做藥品之分類_線上並無作用, DrugForm 藥品劑型, 
                CommitteeMemo 藥委會註記, CommitteeCode 藥委會品項, InventoryFlag 盤點品項YN, ApplyUnit 院內單位, PurchaseCaseType 藥品採購案別, 
                MaxCureConsistency TDM合理治療濃度上限, MinCureConsistency TDM合理治療濃度下限, PearBegin TDM合理PEAK起, PearEnd TDM合理PEAK迄, TroughBegin TDM合理Trough起, 
                TroughEnd TDM合理Trough迄, DangerBegin TDM危急值起, DangerEnd TDM危急值迄, TDMMemo1 TDM備註1, TDMMemo2 TDM備註2, 
                TDMMemo3 TDM備註3, ChinAttention 注意事項_中文, EngAttention 注意事項_英文, DrugMemo 處方集, ChinSideEffect 主要副作用_中文, 
                EngSideEffect 主要副作用_英文, Warn 警語, DOHSymptom 衛生署核准適應症, FDASymptom FDA核准適應症, SuckleSecurity 授乳安全性, 
                PregnantGrade 懷孕分級, DrugExterior 藥品外觀, PurchaseUnit 採購單位, DCMassageMemo 藥品異動備註, FirstPurchaseDate 第一次進貨日期, 
                DrugLeafletLink 藥品仿單檔名, DrugPictureLink 藥品圖片檔名, ComponentNUnit2 成份量及單位2, ComponentNUnit3 成份量及單位3, ComponentNUnit4 成份量及單位4, 
                DrugEngExterior 藥品外觀_英文, ArmyOrderCode 軍品院內碼, DrugApplyType 藥品請領類別,ParentCode 母藥註記, ParentOrderCode 母藥院內碼, 
                SonTransQty 子藥轉換量, CreateDateTime 記錄建立日期時間, CreateOpID 記錄建立人員 
                from HIS_STKDMIT
                where 1=1 ";

            var p01 = "";
            var p23 = "";
            if (p2 != "")
            {
                p01 = p0 + p1.Replace(":", "") + "00";
            }
            else
            {
                p01 = p0 + "000000";
            }
            if (p3 != "")
            {
                p23 = p2 + p3.Replace(":", "") + "00";
            }
            else
            {
                p23 = p1 + "000000";
            }
            if (p0 != "" & p1 != "")
            {
                sql += " AND ProcDateTime BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p01));
                p.Add(":p1", string.Format("{0}", p23));
            }
            if (p0 != "" & p1 == "")
            {
                sql += " AND ProcDateTime >= :p0 ";
                p.Add(":p0", string.Format("{0}", p01));
            }
            if (p0 == "" & p1 != "")
            {
                sql += " AND ProcDateTime <= :p1 ";
                p.Add(":p1", string.Format("{0}", p23));
            }

            sql += @" ORDER BY ProcDateTime";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        

    }
}
