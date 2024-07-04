using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.B;

namespace WebAppVen.Controllers.B
{
    public class BH0010Controller : SiteBase.BaseApiController
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
                    var repo = new BH0010Repository(DBWork);

                    var crdocno = data.Get("crdocno");
           
                    session.Result.afrs = repo.Update(crdocno);                    

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