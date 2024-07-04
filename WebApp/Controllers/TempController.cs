using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;

namespace WebApp.Controllers
{
    [RoutePrefix("api/SPTS")]
    public class TempController : SiteBase.BaseApiController
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("GetPatientInfo")]
        public ApiReturnItem GetPatientInfo(FormDataCollection form)
        {
            string chartNo = form.Get("ChartNo");
            PatientInfo info = new PatientInfo();
            if (chartNo == "456789")
            {
                info.ChartNo = chartNo;
                info.NrCode = "32";
                info.PatientName = "病人89";
                info.BedNo = "01";

                IEnumerable<PatientInfo> infos = new List<PatientInfo>() { info };
                return new ApiReturnItem()
                {
                    Datas = infos
                };
            }

            if (chartNo == "123456")
            {
                info.ChartNo = chartNo;
                info.NrCode = "52";
                info.PatientName = "病人56";
                info.BedNo = "03";

                IEnumerable<PatientInfo> infos = new List<PatientInfo>() { info };
                return new ApiReturnItem()
                {
                    Datas = infos
                };

            }

            return new ApiReturnItem()
            {
                Message = "查無病人資料"
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetEmpInfo")]
        public ApiReturnItem GetEmpInfo(FormDataCollection form)
        {
            string id = form.Get("Id");
            EmpInfo info = new EmpInfo();
            if (id == "N123456789")
            {
                info.IdNo = id;
                info.EmpName = "護理人員測試";
                info.DeptNo = "332300";

                IEnumerable<EmpInfo> infos = new List<EmpInfo>() { info };
                return new ApiReturnItem()
                {
                    Datas = infos
                };
            }

            if (id == "OR12345678")
            {
                info.IdNo = id;
                info.EmpName = "手術室";
                info.DeptNo = "333300";

                IEnumerable<EmpInfo> infos = new List<EmpInfo>() { info };
                return new ApiReturnItem()
                {
                    Datas = infos
                };
            }
            return new ApiReturnItem()
            {
                Message = "查無員工資料"
            };
        }
    }

    public class EmpInfo
    {
        public string IdNo { get; set; }
        public string DeptNo { get; set; }
        public string EmpName { get; set; }
    }

    public class PatientInfo
    {
        public string PatientName { get; set; }
        public string ChartNo { get; set; }
        public string NrCode { get; set; }
        public string BedNo { get; set; }
        public string MedNo { get; set; }
    }
}