using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    
    public class AB0089_3 : JCLib.Mvc.BaseModel
    {
        public string SkOrderCode { get; set; }
        public string ProcDateTime { get; set; }
        public string ProcOpID { get; set; }
        public string DCMassageCode { get; set; }
        public string DMITDCCode { get; set; }

        public string Manufacturer { get; set; }
        public string WCostAmount { get; set; }
        public string PublicDrugCode { get; set; }
        public string StockUseCode { get; set; }
        public string SpecNUnit { get; set; }

        public string ComponentNUnit { get; set; }
        public string YearArmyNo { get; set; }
        public string ItemArmyNo { get; set; }
        public string GroupArmyNo { get; set; }
        public string ClassifiedArmyNo { get; set; }

        public string ContractEffectiveDate { get; set; }
        public string MultiPrescriptionCode { get; set; }
        public string DrugClass { get; set; }
        public string DrugClassify { get; set; }
        public string DrugForm { get; set; }

        public string CommitteeMemo { get; set; }
        public string CommitteeCode { get; set; }
        public string InventoryFlag { get; set; }
        public string ApplyUnit { get; set; }
        public string PurchaseCaseType { get; set; }

        public string MaxCureConsistency { get; set; }
        public string MinCureConsistency { get; set; }
        public string PearBegin { get; set; }
        public string PearEnd { get; set; }
        public string PearTroughBeginEnd { get; set; }

        public string TroughEnd { get; set; }
        public string DangerBegin { get; set; }
        public string DangerEnd { get; set; }
        public string TDMMemo1 { get; set; }
        public string TDMMemo2 { get; set; }

        public string TDMMemo3 { get; set; }
        public string ChinAttention { get; set; }
        public string EngAttention { get; set; }
        public string DrugMemo { get; set; }
        public string ChinSideEffect { get; set; }

        public string EngSideEffect { get; set; }
        public string Warn { get; set; }
        public string DOHSymptom { get; set; }
        public string FDASymptom { get; set; }
        public string SuckleSecurity { get; set; }

        public string PregnantGrade { get; set; }
        public string DrugExterior { get; set; }
        public string PurchaseUnit { get; set; }
        public string DCMassageMemo { get; set; }
        public string FirstPurchaseDate { get; set; }

        public string DrugLeafletLink { get; set; }
        public string DrugPictureLink { get; set; }
        public string ComponentNUnit2 { get; set; }
        public string ComponentNUnit3 { get; set; }
        public string ComponentNUnit4 { get; set; }

        public string DrugEngExterior { get; set; }
        public string ArmyOrderCode { get; set; }
        public string DrugApplyType { get; set; }
        public string ParentCode { get; set; }
        public string ParentOrderCode { get; set; }

        public string SonTransQty { get; set; }
        public string CreateDateTime { get; set; }
        public string CreateOpID { get; set; }
    }
}