using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class TempSpecimen
    {
        public string ChartNo { get; set; }
        public string ChinName { get; set; }
        public string MedNo { get; set; }
        public string VisitSeq { get; set; }
        public string NrCode { get; set; }
        public string Rid { get; set; }
        public string BedNo { get; set; }
        public string RxNo { get; set; }
        public string SendUnit { get; set; }
        public string SendName { get; set; }
        public string SpecimenName { get; set; }
        public string DefaultVolumn { get; set; }
        public string ContainerName { get; set; }
        public string OrderDr { get; set; }
        public string UseDateTime { get; set; }
        public string CreateOpId { get; set; }
        public string CreateDateTime { get; set; }
        public string ExecReceiveLocation { get; set;}
        public string HisDept { get; set; }
        public string HisLocation { get; set; }
        public string Gender { get; set;}
        public string Lis_SampCode { get; set; }
        public string Urgent { get; set; }
        public string AttentionComment { get; set; }
        public string Lis_ToDoCode { get; set; }
        public string Lis_SpecialItem { get; set; }
        public string Out_Check_Item { get; set; }
        public string DataSource { get; set; }
        public string CancelDateTime { get; set; }
        public string CancelOpId { get; set; }
        public string OrderNo { get; set; }
        public string IsCancel { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateIp { get; set; }
        public string HisLocation_SPTS { get; set; }
        public string DEPT { get; set; }
        public string ERGLEVEL { get; set; }
        public string LAB_BARCODE { get; set; }
        public string LAB_STNNAME { get; set; }
        public string FCLASS_DEPT { get; set; }
    }

    public class TempPath {
        public string DataSource { get; set; }
        public string ArvlDeptNo { get; set; }
        public string RxNo { get; set; }
        public string ChartNo { get; set; }
        public string MedNo { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public string AdmNo { get; set; }
        public string NrCode { get; set; }
        public string BedNo { get; set; }
        public string Rid { get; set; }
        public string TrtDept { get; set; }
        public string ExamCode { get; set; }
        public string ExamName { get; set; }
        public string IsUrgent { get; set; }
        public string OrderDrName { get; set; }
        public string OrderTime { get; set; }
        public string OrderMemo { get; set; }
        public string OrderNo { get; set; }
        public string IsCancel { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateIp { get; set; }
        public string HisLocation { get; set; }

    }
}