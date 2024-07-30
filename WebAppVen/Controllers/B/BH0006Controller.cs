using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.B;
using WebAppVen.Models;
using System.Text;
using System.Security.Cryptography;
using System;
using System.Globalization;

namespace WebAppVen.Controllers.B
{
    public class BH0006Controller : SiteBase.BaseApiController
    {
        //新增Detail
        [HttpPost]
        [AllowAnonymous]//可匿名存取
        public ApiResponse Create(FormDataCollection data)
        {
            using (WorkSession session = new WorkSession()) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0006Repository(DBWork);
                    var WB_MAILBACK = new WB_MAILBACK();

                    WB_MAILBACK.PO_NO = data.Get("po_no");
                    WB_MAILBACK.AGEN_NO = data.Get("agen_no");
                    WB_MAILBACK.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Create(WB_MAILBACK);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        [AllowAnonymous]//可匿名存取
        public string Do_DeCode(FormDataCollection form)
        {
            var data = form.Get("data");

            return JCLib.Security.DES.Decode(data); ;
        }

    }
}