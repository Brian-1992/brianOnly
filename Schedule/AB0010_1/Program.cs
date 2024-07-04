using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCLib.DB;
using JCLib.DB.Tool;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Dapper;
using AB0010_1.Repository;

namespace AB0010_1
{
    class Program
    {
        static void Main(string[] args)
        {
            updateMEDOCM();
        }

        static L l = new L("AB0010_1.Program");

        static void updateMEDOCM()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new ME_DOCMRepository(DBWork);
                    int updateAfrs = -1;
                    IEnumerable<string> docnos = repo.GetDocnos();
                    Console.WriteLine(string.Format("docno數量: {0}", docnos.Count()));
                    foreach (string docno in docnos) {
                        updateAfrs = repo.UpdateDocdApl_contime(docno);
                        Console.WriteLine(string.Format("docno:{0} 更新ME_DOCD共{1}筆", docno, updateAfrs));
                    }

                    updateAfrs = repo.UpdateME_DOCM(); 
                    Console.WriteLine(string.Format("更新ME_DOCM共{0}筆", updateAfrs));

                    DBWork.Commit();
                    l.clg(string.Format("排程執行完畢,更新ME_DOCM共{0}筆", updateAfrs));
                }
                catch
                {
                    l.clg("排程執行失敗");
                    DBWork.Rollback();
                    throw;
                }
            }
        }
    }
}
