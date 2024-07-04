using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.B;
using WebAppVen.Models;

namespace WebAppVen.Controllers.B
{
    public class BH0009Controller : SiteBase.BaseApiController
    {
        //更新狀態及日期
        [HttpPost]
        [AllowAnonymous]//可匿名存取
        public ApiResponse Update(FormDataCollection data)
        {
            using (WorkSession session = new WorkSession()) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0009Repository(DBWork);

                    string[] Batnos = data.Get("invoice_batno").Split(',');
                    
                    foreach (var invoice_batno in Batnos)
                    {                        
                        session.Result.afrs = repo.Update(invoice_batno);
                    }                  

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