using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Controllers
{
    [RoutePrefix("api/MMSMS")]
    public class WebApiController : ApiController
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("GetInvQty")]
        public ApiReturnItem GetInvqty([FromBody]WhMmInvqty item)
        {

            if (string.IsNullOrEmpty(item.WH_NO) || item.MMCODES.Any() == false)
            {
                return new ApiReturnItem()
                {
                    Message = "請填入庫房代碼(WH_NO)與院內碼清單(MMCODES)"
                };
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {

                    WebApiRepository repo = new WebApiRepository(DBWork);

                    List<WhMmInvqty> list = new List<WhMmInvqty>();
                    foreach (string mmcode in item.MMCODES)
                    {
                        WhMmInvqty temp = repo.GetInvqty(item.WH_NO, mmcode);
                        if (temp == null)
                        {
                            temp = repo.GetWexpInvWhenCTLNULL(item.WH_NO, mmcode);
                            if (temp == null)
                            {
                                temp = repo.GetWexpInvWhenINVNULL(item.WH_NO, mmcode);
                            }
                        }
                        temp.LOT_EXP_INV = repo.GetWexpInv(item.WH_NO, mmcode);
                        List<LotExpInv> temp_list = new List<LotExpInv>();

                        list.Add(temp);
                    }

                    return new ApiReturnItem()
                    {
                        Datas = list
                    };
                }
                catch (Exception e)
                {

                    throw;
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetMmcodeInvqty")]
        public ApiReturnItem GetMmcodeInvqty([FromBody]WhMmInvqty item)
        {
            if (string.IsNullOrEmpty(item.MMCODE))
            {
                return new ApiReturnItem()
                {
                    Message = "請填入院內碼(MMCODE)"
                };
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    WebApiRepository repo = new WebApiRepository(DBWork);
                    IEnumerable<MmcodeInvqty> invqtys = repo.GetMmcodeInvqty(item.MMCODE);

                    return new ApiReturnItem()
                    {
                        Datas = invqtys
                    };
                }
                catch (Exception e)
                {

                    throw;
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetTrns")]
        public ApiReturnItem GetTrns([FromBody]WhMmInvqty item)
        {
            string checkResult = CheckTrnsInput(item);
            if (checkResult != string.Empty)
            {
                return new ApiReturnItem()
                {
                    Message = checkResult
                };
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    WebApiRepository repo = new WebApiRepository(DBWork);
                    string mmcodesString = GetMmcodeString(item.MMCODES);
                    IEnumerable<WHTRNS> trns = repo.GetWhtrns(item.WH_NO, mmcodesString, item.STARTDATE, item.ENDDATE);

                    return new ApiReturnItem()
                    {
                        Datas = trns
                    };
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        private string CheckTrnsInput(WhMmInvqty item)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(item.WH_NO))
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "庫房代碼未填(WH_NO)");
            }
            if (item.MMCODES.Any() == false)
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "院內碼陣列未填(MMCODES)");
            }
            if (string.IsNullOrEmpty(item.STARTDATE))
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "起始日期未填(STARTDATE)");
            }
            if (string.IsNullOrEmpty(item.STARTDATE))
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "起始日期未填(STARTDATE)");
            }
            return result;
        }
        private string GetMmcodeString(IEnumerable<string> mmcodes)
        {
            string result = string.Empty;
            foreach (string mmcode in mmcodes)
            {
                if (result != string.Empty)
                {
                    result += ",";
                }
                result += string.Format("'{0}'", mmcode);
            }
            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetDayPInvqty")]
        public ApiReturnItem GetDayPInvqty(PInvqty item)
        {
            #region 檢查需求欄位
            string result = string.Empty;
            if (string.IsNullOrEmpty(item.PDATE))
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "查詢日期未填(PDATE)");
            }
            if (string.IsNullOrEmpty(item.WH_NO))
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "庫房代碼未填(WH_NO)");
            }
            if (string.IsNullOrEmpty(item.MMCODE))
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "院內碼未填(MMCODE)");
            }
            #endregion

            if (string.IsNullOrEmpty(result) == false)
            {
                return new ApiReturnItem()
                {
                    Message = result
                };
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new WebApiRepository(DBWork);
                    IEnumerable<PInvqty> list = repo.GetPInvqty(item.PDATE, item.WH_NO, item.MMCODE);

                    return new ApiReturnItem()
                    {
                        Datas = list
                    };
                }
                catch (Exception e)
                {
                    throw;
                }
            }

        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetTempSpeci")]
        public IEnumerable<TempSpecimen> GetTempSpcei()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new WebApiRepository(DBWork);
                    IEnumerable<TempSpecimen> list = repo.GetTempSpecimens();

                    return list;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetTempPath")]
        public IEnumerable<TempPath> GetTempPath()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new WebApiRepository(DBWork);
                    IEnumerable<TempPath> list = repo.GetTempPath();

                    return list;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetWhMasts")]
        public ApiReturnItem GetWhMasts([FromBody]WhMast item)
        {
            string checkResult = CheckWhnosInput(item);
            if (checkResult != string.Empty)
            {
                if (string.IsNullOrEmpty(checkResult) == false)
                {
                    return new ApiReturnItem()
                    {
                        Message = checkResult
                    };
                }
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new WebApiRepository(DBWork);
                    IEnumerable<WhMast> list = repo.GetWhMasts(item.INID, item.WH_KIND);

                    return new ApiReturnItem()
                    {
                        Datas = list
                    };

                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetETagInfo")]
        public ApiReturnItem GetETagInfo([FromBody]WhMmInvqty item)
        {

            if (string.IsNullOrEmpty(item.WH_NO))
            {
                return new ApiReturnItem()
                {
                    Message = "請填入庫房代碼(WH_NO)"
                };
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    WebApiRepository repo = new WebApiRepository(DBWork);
                    List<ETagInfo> list = new List<ETagInfo>();
                    string mmcodeString = string.Join(",", item.MMCODES.Select(s => $"'{s}'"));
                    list = repo.GetETagInfo(item.WH_NO, mmcodeString);

                    return new ApiReturnItem()
                    {
                        Datas = list
                    };
                }
                catch (Exception e)
                {

                    throw;
                }
            }
        }

        private string CheckWhnosInput(WhMast item)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(item.INID))
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "責任中心未填(INID)");
            }
            if (string.IsNullOrEmpty(item.WH_KIND))
            {
                if (result != string.Empty)
                {
                    result += "、";
                }
                result = string.Format("{0}{1}", result, "責任中心未填(INID)");
            }
            return result;
        }
    }
}