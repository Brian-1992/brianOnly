using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0089_2 : JCLib.Mvc.BaseModel
    {
        public string OrderCode { get; set; }
        public string ProcDateTime { get; set; }
        public string ProcOpID { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string StockTransQtyO { get; set; }
        public string StockTransQtyI { get; set; }
        public string ChangeQtyI { get; set; }
        public string ChangeQtyO { get; set; }
        public string Price1 { get; set; }
        public string Price2 { get; set; }
        public string CostAmount { get; set; }
        public string IsDisc { get; set; }
        public string DiscPer { get; set; }
        public string InsuOrderCode { get; set; }
        public string InsuSignI { get; set; }
        public string InsuSignO { get; set; }
        public string ContractPrice { get; set; }
        public string ContracNo { get; set; }
        public string SupplyNo { get; set; }
        public string CaseFrom { get; set; }
        public string OriginalProducer { get; set; }
        public string AgentName { get; set; }
        public string CreateDateTime { get; set; }
        public string CreateOpID { get; set; }
    }
}