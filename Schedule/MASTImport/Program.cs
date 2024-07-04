using System;
using MMSMSBAS;
using MMSMSBAS.Models;
using JCLib.DB;
using MASTImport.Repository;
using MMSMSREPORT.MMReport;
using MMSMSREPORT.Models;

using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using JCLib.DB.Tool;
using System.Diagnostics;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Types;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MASTImport
{
    class Program
    {
        static void Main(string[] args)
        {
            callMMSMSBAS();
        }

        static L l = new L("MASTImport.Program");

        static void callMMSMSBAS()
        {
            /**
             * 呼叫林江開發的dll取得資料
             */
            TsghHisBAS db = new TsghHisBAS();
            var basstkaList = db.GetBASSTKA<BASSTKAModels>();
            var basordmList = db.GetBASORDM<BASORDMModels>();
            var basorddList = db.GetBASORDD<BASORDDModels>();
            var stkctdmList = db.GetSTKCTDM<STKCTDMModels>();
            var medlocationList = db.GetMEDLOCATION<MEDLOCATIONModels>();
            var basespcList = db.GetBASESPC<BASESPCModels>();
            var phrdcmgList = db.GetPHRDCMG<PHRDCMGModels>();

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                // 做寫入資料的部分
                DBWork.BeginTransaction();
                try
                {
                    //STKDMIT
                    Console.WriteLine("開始匯入BASSTKA...");
                    var repoS = new BASSTKARepository(DBWork);
                    int delAfrsS = repoS.DeleteAll(); //基本檔才需刪除
                    Console.WriteLine(string.Format("刪除BASSTKA共{0}筆", delAfrsS));

                    int ImpAfrsS = repoS.Import(basstkaList);
                    Console.WriteLine(string.Format("匯入BASSTKA共{0}筆", ImpAfrsS));

                    //BASORDM
                    Console.WriteLine("開始匯入BASORDM...");
                    var repoM = new BASORDMRepository(DBWork);
                    int delAfrsM = repoM.DeleteAll(); //基本檔才需刪除
                    Console.WriteLine(string.Format("刪除BASORDM共{0}筆", delAfrsM));

                    int ImpAfrsM = repoM.Import(basordmList);
                    Console.WriteLine(string.Format("匯入BASORDM共{0}筆", ImpAfrsM));

                    //BASORDD
                    Console.WriteLine("開始匯入BASORDD...");
                    var repoD = new BASORDDRepository(DBWork);
                    int delAfrsD = repoD.DeleteAll(); //基本檔才需刪除
                    Console.WriteLine(string.Format("刪除BASORDD共{0}筆", delAfrsD));

                    int ImpAfrsD = repoD.Import(basorddList);
                    Console.WriteLine(string.Format("匯入BASORDD共{0}筆", ImpAfrsD));

                    //STKCTDM
                    Console.WriteLine("開始匯入STKCTDM...");
                    var repoC = new STKCTDMRepository(DBWork);
                    int delAfrsC = repoC.DeleteAll(); //基本檔才需刪除
                    Console.WriteLine(string.Format("刪除STKCTDM共{0}筆", delAfrsC));

                    int ImpAfrsC = repoC.Import(stkctdmList);
                    Console.WriteLine(string.Format("匯入STKCTDM共{0}筆", ImpAfrsC));

                    //MEDLOCATION
                    Console.WriteLine("開始匯入MEDLOCATION...");
                    var repoMED = new MEDLOCATIONRepository(DBWork);
                    int delAfrsMED = repoMED.DeleteAll(); //基本檔才需刪除
                    Console.WriteLine(string.Format("刪除MEDLOCATION共{0}筆", delAfrsMED));

                    int ImpAfrsMED = repoMED.Import(medlocationList);
                    Console.WriteLine(string.Format("匯入MEDLOCATION共{0}筆", ImpAfrsMED));

                    //BASESPC
                    Console.WriteLine("開始匯入BASESPC...");
                    var repoBSPC = new BASESPCRepository(DBWork);
                    int delAfrsBSPC = repoBSPC.DeleteAll(); //基本檔才需刪除
                    Console.WriteLine(string.Format("刪除BASESPC共{0}筆", delAfrsBSPC));

                    int ImpAfrsBSPC = repoBSPC.Import(basespcList);
                    Console.WriteLine(string.Format("匯入BASESPC共{0}筆", ImpAfrsBSPC));

                    //PHRDCMG
                    Console.WriteLine("開始匯入PHRDCMG...");
                    var repoPCMG = new PHRDCMGRepository(DBWork);
                    int delAfrsPCMG = repoPCMG.DeleteAll(); //基本檔才需刪除
                    Console.WriteLine(string.Format("刪除PHRDCMG共{0}筆", delAfrsPCMG));

                    int ImpAfrsPCMG = repoPCMG.Import(phrdcmgList);
                    Console.WriteLine(string.Format("匯入PHRDCMG共{0}筆", ImpAfrsPCMG));

                    DBWork.Commit();
                }
                catch(Exception ex)
                {
                    l.clg("資料匯入失敗:" + ex.Message);
                    DBWork.Rollback();
                    throw;
                }

                // 做排程的部分
                DBWork.BeginTransaction();
                try
                {
                    Console.WriteLine("執行排程HIS_MAST...");
                    procMast();

                    DBWork.Commit();
                    l.clg("排程執行完畢");
                }
                catch (Exception ex)
                {
                    l.clg("排程匯入失敗:" + ex.Message);
                    DBWork.Rollback();
                    throw;
                }
            }

            Console.WriteLine("匯入完成");
        }

        static public string procMast()
        {
            CallDBtools calldbtools = new CallDBtools();
            String s_conn_oracle = calldbtools.SelectDB("oracle");

            OracleConnection conn = new OracleConnection(s_conn_oracle);
            conn.Open();
            OracleCommand cmd = new OracleCommand("INF_SET.HIS_MAST", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            OracleParameter o_retid = new OracleParameter("O_RETID", OracleDbType.Varchar2, 1);
            o_retid.Direction = ParameterDirection.Output;

            OracleParameter o_retmsg = new OracleParameter("O_RETMSG", OracleDbType.Varchar2, 200);
            o_retmsg.Direction = ParameterDirection.Output;

            cmd.Parameters.Add(o_retid);
            cmd.Parameters.Add(o_retmsg);

            cmd.ExecuteNonQuery();

            return o_retmsg.ToString();
        }
    }
}
