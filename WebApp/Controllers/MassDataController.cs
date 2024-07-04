using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JCLib.DB;
using Dapper;
using System.Data;
using Newtonsoft.Json;
using WebApp.Repository.UR;

namespace WebApp.Controllers
{
    public class MassDataController : SiteBase.BaseApiController
    {
        [AllowAnonymous]
        // GET api/<controller>
        public string Get()
        {
            DataSet result = new DataSet();

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    MassDataRepository repo = new MassDataRepository(DBWork);
                    result.Tables.Add(repo.GetAll());
                }
                catch
                {
                    throw;
                }
            }

            //把DataSet物件轉成JSON字串
            return JsonConvert.SerializeObject(result);
        }
    }
}