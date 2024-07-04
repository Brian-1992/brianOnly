using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCLib.DB;
using JCLib.DB.Tool;

namespace BES003
{
    public class Service
    {
        public void Run() {
            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try {
                    var repo = new BES003Reposiory(DBWork);

                    repo.UpdateStatus();

                    DBWork.Commit();

                }catch(Exception e){
                    DBWork.Rollback();
                }
            }
        }
    }
}
