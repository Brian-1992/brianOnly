using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository;

namespace WebApp.Controllers
{
    [RoutePrefix("api/MMSMS")]
    public class WebApiController:SiteBase.BaseApiController
    {
        [HttpPost]
        [Route("GetInvQty")]
        public ApiReturnItem GetInvqty(MI_WINVCTL item ) {

            if (string.IsNullOrEmpty(item.WH_NO) || string.IsNullOrEmpty(item.MMCODE)) {
                return new ApiReturnItem()
                {
                    Message = "請填入庫房代碼(WH_NO)與院內碼(MMCODE)"
                };
            }

            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                try
                {
                    WebApiRepository repo = new WebApiRepository(DBWork);
                    WhMmInvqty temp = repo.GetInvqty(item);
                    temp.LOT_EXP_INV = repo.GetWexpInv(temp);

                    return new ApiReturnItem()
                    {
                        Datas = new List<WhMmInvqty>() { temp }
                    };
                }
                catch (Exception e) {

                    throw;
                }
            }
        }

        [HttpPost]
        [Route("GetMmcodeInvqty")]
        public ApiReturnItem GetMmcodeInvqty(WhMmInvqty item) {
            if (string.IsNullOrEmpty(item.MMCODE)) {
                return new ApiReturnItem()
                {
                    Message= "請填入院內碼(MMCODE)"
                };
            }

            using (WorkSession session = new WorkSession()) {
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

    }
}