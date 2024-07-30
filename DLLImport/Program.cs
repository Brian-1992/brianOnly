using System;
using MMSMSBAS;
using MMSMSBAS.Models;
using JCLib.DB;
using DLLImport.Repository;
using MMSMSREPORT.MMReport;
using MMSMSREPORT.Models;

using System.Reflection;
using DLLImport.Repository;
using DLLImport.Models;
using System.Collections.Generic;
using System.Linq;
using JCLib.DB.Tool;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DLLImport
{
    class Program
    {

        #region " 共用函式 "
        static String getSDT(
            ME_DLLCTLRepository r,
            String dllCode
        )
        {
            //01.以DLLCODE=AB0083D     查ME_DDLCTL.ENDDATE時間為scdt
            ME_DLLCTLModels scdt = new ME_DLLCTLModels();
            scdt.DLLCODE = dllCode; //  "AB0083D";
            List<ME_DLLCTLModels> lst = r.Query(scdt).ToList(); // insert into ME_DLLCTL(DLLCODE, WH_NO, ENDDATE) values('AB0083D', '*', sysdate)
            if (lst.Count == 0)
            {
                scdt.WH_NO = "*";
                r.ins(scdt);
                scdt = r.QueryFirst(scdt);
            }
            else
            {
                scdt = lst[0];
            }
            l.clg("讀取 select TWN_TIME(ENDDATE) from ME_DLLCTL where DLLCODE='" + dllCode + "' 取得'" + scdt.ENDDATE + "'時間 ");
            return scdt.ENDDATE;
        }
        static String getSDT(
            ME_DLLCTLRepository r,
            String wh_no,
            String dllCode
        )
        {
            //01.以DLLCODE=AB0083D     查ME_DDLCTL.ENDDATE時間為scdt
            ME_DLLCTLModels scdt = new ME_DLLCTLModels();
            scdt.WH_NO = wh_no;
            scdt.DLLCODE = dllCode;
            List<ME_DLLCTLModels> lst = r.Query(scdt).ToList(); // insert into ME_DLLCTL(DLLCODE, WH_NO, ENDDATE) values('AB0083D', '*', sysdate)
            if (lst.Count == 0)
            {
                r.ins(scdt);
                scdt = r.QueryFirst(scdt);
            }
            else
            {
                scdt = lst[0];
            }
            l.clg("讀取 select TWN_TIME(ENDDATE) from ME_DLLCTL where WH_NO='" + wh_no + "' and DLLCODE='" + dllCode + "' 取得'" + scdt.ENDDATE + "'時間 ");
            return scdt.ENDDATE;
        }
        static String getEDT(ME_DLLCTLRepository r)
        {
            ME_DLLCTLModels ecdt = r.QueryEndDate();
            l.clg("讀取 select TWN_TIME(SYSDATE) from dual 取得'" + ecdt.ENDDATE + "'時間 ");
            return ecdt.ENDDATE;
        } // 
        static void updEndDate(
            ME_DLLCTLRepository r,
            String dllcode,
            String endDate
        )
        {

            ME_DLLCTLModels upd_me_dllctl = new ME_DLLCTLModels();
            upd_me_dllctl.DLLCODE = dllcode;
            upd_me_dllctl.ENDDATE = endDate;
            int iEffect = r.UpdateEndDate(upd_me_dllctl);
            l.clg(string.Format("回押ME_DLLCTL.ENDDATE=結束時間 update ME_DLLCTL set ENDDATE='" + endDate + "' where DLLCODE='" + dllcode + "', 共{0}筆", iEffect));
        } // 
        static void updEndDate(
            ME_DLLCTLRepository r,
            String dllcode,
            String wh_no,
            String endDate
        )
        {

            ME_DLLCTLModels upd_me_dllctl = new ME_DLLCTLModels();
            upd_me_dllctl.DLLCODE = dllcode;
            upd_me_dllctl.WH_NO = wh_no;
            upd_me_dllctl.ENDDATE = endDate;
            int iEffect = r.UpdateEndDate(upd_me_dllctl);
            l.clg(string.Format("回押ME_DLLCTL.ENDDATE=結束時間 update ME_DLLCTL set ENDDATE='" + endDate + "' where DLLCODE='" + dllcode + "' and WH_NO='" + wh_no + "', 共{0}筆", iEffect));
        } // 

        static void logLstData<T>(IList<T> getHisBackList)
        {
            if (getHisBackList.Count > 0)
            {
                hisbackFileName = exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_PhrP021_table_log.txt";
                l.writeFile(
                    l.getLogDirPath(), // C:\TsghmmLog\Schedule
                    hisbackFileName,   // HISBACK.Program_19-08-16_113233_MMRep_PhrP021_table_log.txt";
                    JsonConvert.SerializeObject(getHisBackList)
                );
            }
        }



        #endregion


        // select* from ME_DLLCTL where DLLCODE like 'HISBACK%'  -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'HISBACK%'  -- 測試刪除
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'HISBACK%'  -- 測試程式時使用
        // select count(*) from HIS_BACK where ordercode='005TRA07'  -- 檢查是否有從Sybase轉到Oracle
        // select* from HIS_BACK where ordercode='005TRA07'  -- 檢查是否有從Sybase轉到Oracle
        static void HISBACK_處理病房退藥扣庫(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "HISBACK";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0079D     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式MMRep_PhrP021(scdt, ecdt)取得資料
            MMRep_PhrP021 db2 = new MMRep_PhrP021();
            var getHisBackList = db2.GetWardBackDrug<HISBACKModles>(sDt, eDt);
            logLstData<HISBACKModles>(getHisBackList);
            l.clg(string.Format("查詢三總Sybase.INACARM, HISMEDD, HISPROF, BASORDM, HISEXND, BASORDD, HISBACK 等Table組合資料，共{0}筆", getHisBackList.Count));

            // -- oracle端處理 -- 
            var repoB = new RepHISBACKRepository(DBWork);
            // 04.Ins Oracle HIS_BACK
            int ImpAfrsB = repoB.Import(getHisBackList);
            l.clg(string.Format("匯入Oracle.HIS_BACK共{0}筆", ImpAfrsB));

            // 05.Ins Oracle ME_BACK
            HISBACKModles v = new HISBACKModles() { SCDT = sDt, ECDT = eDt };
            int ImpAfrsC = repoB.Ins_to_ME_BACK(v);
            l.clg(string.Format("匯入Oracle.ME_BACK共{0}筆", ImpAfrsC));

            // 06.執行Stored Procedure CREATE_RN_DOC
            var iCreate_Rn_Doc = repoB.exec_CREATE_RN_DOC();
            l.clg(string.Format("執行Oracle.StredProcedure CREATE_RN_DOC 結果:{0},訊息:{1}", iCreate_Rn_Doc.Split('^')[0], iCreate_Rn_Doc.Split('^')[1]));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 


        // 介接報表 AB0071買退藥清單 介接至 ME_AB0071 (Oracle DB)
        // select * from ME_DLLCTL where DLLCODE like 'AB0071%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0071%' -- 測試刪除
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0071%'  -- 測試程式時使用
        // select count(*) from ME_AB0071 where ordercode='005TRA01'  -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0071 where ordercode='005TRA01'  -- 檢查是否有從Sybase轉到Oracle
        static void AB0071_買退藥清單(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0071";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0079D     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0071 mmrep_ab0071 = new MMRep_AB0071();
            var lstAb0071 = mmrep_ab0071.GetBuyBackDrug<ME_AB0071Modles>(sDt, eDt);
            logLstData<ME_AB0071Modles>(lstAb0071);
            l.clg(string.Format("查詢三總Sybase.HISEXND, BASORDM, HISBACK, XMYORDO, XMYOPDM, BASUSRM, HISMEDD, HISPROF 等Table組合資料，共{0}筆", lstAb0071.Count));


            //04.Ins Oracle ME_AB0071
            var repoB = new RepAB0071Repository(DBWork);
            int ImpAfrsB = repoB.Import(lstAb0071);
            l.clg(string.Format("匯入Oracle.ME_AB0071共{0}筆", ImpAfrsB));

            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } //


        // select* from ME_DLLCTL where DLLCODE like 'AB0079D%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0079D%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0079D%'  -- 測試程式時使用
        // select count(*) from ME_AB0079 where ORDERCODE='005TRA01' and DSM='D' -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0079 where ORDERCODE='005TRA01' and DSM='D'
        static void AB0079D_每月醫生醫令統計(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0079D";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0079D     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0079D mmrep_ab0079d = new MMRep_AB0079D();
            var lstAb0079d = mmrep_ab0079d.GetDocOrdercode<ME_AB0079DModles>(sDt, eDt);
            logLstData<ME_AB0079DModles>(lstAb0079d);
            l.clg(string.Format("查詢三總Sybase.HISPROF, HISEXND, BASORDM, INACARM, BASUSRM, XMYORDO 等Table組合資料，共{0}筆", lstAb0079d.Count));

            //04.Ins Oracle ME_AB0079D
            var repoB = new RepAB0079DRepository(DBWork);
            //int delAfrsS = repoB.DeleteAll(); //基本檔才需刪除
            //l.clg(string.Format("刪除Oracle.AB0079D共{0}筆", delAfrsS));
            int ImpAfrsB = repoB.Import(lstAb0079d);
            l.clg(string.Format("匯入Oracle.ME_AB0079D共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 

        // select* from ME_DLLCTL where DLLCODE like 'AB0079S%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0079S%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0079S%'  -- 測試程式時使用
        // select count(*) from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='S'  -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='S'  -- 檢查是否有從Sybase轉到Oracle
        static void AB0079S_每月藥品醫令統計(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0079S";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0079S     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0079S mmrep_ab0079s = new MMRep_AB0079S();
            var lstAb0079s = mmrep_ab0079s.GetDrugOrdercode<ME_AB0079SModles>(sDt, eDt);
            logLstData<ME_AB0079SModles>(lstAb0079s);
            l.clg(string.Format("查詢三總Sybase.HISPROF, HISEXND, BASORDM, INACARM, XMYORDO 等Table組合資料，共{0}筆", lstAb0079s.Count));


            //04.Ins Oracle ME_AB0079S
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAb0079s);
            l.clg(string.Format("匯入Oracle.ME_AB0079S共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 


        // select* from ME_DLLCTL where DLLCODE like 'AB0079M%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0079M%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0079M%'  -- 測試程式時使用
        // select count(*) from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        static void AB0079M_每月科室醫令統計(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0079M";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0079M     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt

            
            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0079M mmrep_ab0079M = new MMRep_AB0079M();
            var lstAb0079M = mmrep_ab0079M.GetDeptOrdercode<ME_AB0079MModles>(sDt, eDt);
            logLstData<ME_AB0079MModles>(lstAb0079M);
            l.clg(string.Format("查詢三總Sybase.HISPROF, HISEXND, BASORDM, INACARM, BASUSRM, XMYORDO 等Table組合資料，共{0}筆", lstAb0079M.Count));

            //04.Ins Oracle ME_AB0079M
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAb0079M);
            l.clg(string.Format("匯入Oracle.ME_AB0079M共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 


        // select* from ME_DLLCTL where DLLCODE like 'AB0083A%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0083A%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0083A%'  -- 測試程式時使用
        // select count(*) from ME_AB0083A where 1=1 and ORDERCODE='005TRA07' -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0083A where 1=1 and ORDERCODE='005TRA07' -- 檢查是否有從Sybase轉到Oracle
        static void AB0083A_退藥異常統計表(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0083A";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0083D     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt
            

            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0083A mmrep_ab0083A = new MMRep_AB0083A();
            var lstAb0083A = mmrep_ab0083A.GetDrugBackA<ME_AB0083AModles>(sDt, eDt);
            logLstData<ME_AB0083AModles>(lstAb0083A);
            l.clg(string.Format("查詢三總Sybase.HISBACK, HISEXND, BASNRSM, BASCODE, BASORDM 等Table組合資料，共{0}筆", lstAb0083A.Count));
            
            //04.Ins Oracle ME_AB0083A
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAb0083A);
            l.clg(string.Format("匯入Oracle.ME_AB0083A共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 


        // select* from ME_DLLCTL where DLLCODE like 'AB0083B%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0083B%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0083B%'  -- 測試程式時使用
        // select count(*) from ME_AB0083B where 1=1 and ORDERCODE='005TRA07' -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0083B where 1=1 and ORDERCODE='005TRA07' -- 檢查是否有從Sybase轉到Oracle
        static void AB0083B_退藥短少金額統計表(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0083B";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0083D     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0083B mmrep_ab0083B = new MMRep_AB0083B();
            var lstAb0083B = mmrep_ab0083B.GetDrugBackB<ME_AB0083BModles>(sDt, eDt);
            logLstData<ME_AB0083BModles>(lstAb0083B);
            l.clg(string.Format("查詢三總Sybase.HISBACK, HISEXND, BASNRSM, BASCODE, BASORDM 等Table組合資料，共{0}筆", lstAb0083B.Count));
            
            //04.Ins Oracle ME_AB0083B
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAb0083B);
            l.clg(string.Format("匯入Oracle.ME_AB0083B共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 


        // select* from ME_DLLCTL where DLLCODE like 'AB0083C%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0083C%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0083C%'  -- 測試程式時使用
        // select count(*) from ME_AB0083C where 1=1 -- ORDERCODE='005TRA01' -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0083C where 1=1 and ORDERCODE='005TRA07' -- 檢查是否有從Sybase轉到Oracle
        static void AB0083C_退藥異常次數及金額統計表(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0083C";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_AB0083C_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0083D     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0083C mmrep_ab0083C = new MMRep_AB0083C();
            var lstAb0083C = mmrep_ab0083C.GetDrugBackC<ME_AB0083CModles>(sDt, eDt);
            logLstData<ME_AB0083CModles>(lstAb0083C);
            l.clg(string.Format("查詢三總Sybase.HISBACK, HISEXND, BASNRSM, BASORDM, STKDMIT 等Table組合資料，共{0}筆", lstAb0083C.Count));

            //04.Ins Oracle ME_AB0083C
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAb0083C);
            l.clg(string.Format("匯入Oracle.ME_AB0083C共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 



        // select * from ME_DLLCTL where DLLCODE like 'AB0083D%'                    -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0083D%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0083D%' -- 測試程式時使用
        // select count(*) from ME_AB0083D where 1=1 and ORDERCODE='005TRA07'       -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0083D where 1=1 and ORDERCODE='005TRA07'              -- 檢查是否有從Sybase轉到Oracle
        static void AB0083D_退藥異常總表(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0083D";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0083D     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0083D mmrep_ab0083D = new MMRep_AB0083D();
            var lstAb0083D = mmrep_ab0083D.GetDrugBackD<ME_AB0083DModles>(sDt, eDt);
            logLstData<ME_AB0083DModles>(lstAb0083D);
            l.clg(string.Format("查詢三總Sybase.HISBACK, HISEXND, BASNRSM, BASORDM, STKDMIT, BASCODE 等Table組合資料，共{0}筆", lstAb0083D.Count));
            
            //04.Ins Oracle ME_AB0083D
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAb0083D);
            l.clg(string.Format("匯入Oracle.ME_AB0083D共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 


        // select* from ME_DLLCTL where DLLCODE like 'AB0075A%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0075A%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0075A%'  -- 測試程式時使用
        // select count(*) from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        static void AB0075A_化療工作量依醫師確認日期(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0075A";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0075A     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0075A mmrep_AB0075A = new MMRep_AB0075A();
            var lstAB0075A = mmrep_AB0075A.GetCHEMOWork<ME_AB0075AModles>(sDt, eDt);
            logLstData<ME_AB0075AModles>(lstAB0075A);
            l.clg(string.Format("查詢三總Sybase.PHRCHMC, HISPROF, PHRCHEM 等Table組合資料，共{0}筆", lstAB0075A.Count));

            //04.Ins Oracle ME_AB0075A
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAB0075A);
            l.clg(string.Format("匯入Oracle.ME_AB0075A共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 

        // select* from ME_DLLCTL where DLLCODE like 'AB0075B%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0075B%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0075B%'  -- 測試程式時使用
        // select count(*) from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        static void AB0075B_PCA每月工作量及項目統計(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0075B";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0075B     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0075B mmrep_AB0075B = new MMRep_AB0075B();
            var lstAB0075B = mmrep_AB0075B.GetPCAWork<ME_AB0075BModles>(sDt, eDt);
            logLstData<ME_AB0075BModles>(lstAB0075B);
            l.clg(string.Format("查詢三總Sybase.HISPROF 等Table組合資料，共{0}筆", lstAB0075B.Count));

            //04.Ins Oracle ME_AB0075B
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAB0075B);
            l.clg(string.Format("匯入Oracle.ME_AB0075B共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 

        //【在漢翔要跑測試程式前要執行這段】
        //UPDATE TPNEUCR set
        //VISITDATE = LTRIM(RTRIM(
        //    STR(convert(int, substring(convert(char(10), getdate(), 111), 1, 4)) - 1911) ||
        //    substring(convert(char(10), getdate(), 111), 6, 2) ||
        //    substring(convert(char(10), getdate(), 111), 9, 2)
        //))
        //where MAKEUPTYPE = '3'
        //and MEDNO || LTRIM(RTRIM(STR(VISITSEQ))) || LTRIM(RTRIM(STR(ORDERNO))) in (
        //    select MEDNO || LTRIM(RTRIM(STR(VISITSEQ))) || LTRIM(RTRIM(STR(PARENTORDERNO))) from HISPROF
        //)
        // ---------
        // select* from ME_DLLCTL where DLLCODE like 'AB0075C%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0075C%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0075C%'  -- 測試程式時使用
        // select count(*) from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        static void AB0075C_TPN藥局(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0075C";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0075C     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt
            eDt = eDt.Substring(0, 7);          // 取前7碼(1081009)


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0075C mmrep_AB0075C = new MMRep_AB0075C();
            var lstAB0075C = mmrep_AB0075C.GetTPNWork<ME_AB0075CModles>(eDt);
            logLstData<ME_AB0075CModles>(lstAB0075C);
            l.clg(string.Format("查詢三總Sybase.TPNEUCR, HISPROF 等Table組合資料，共{0}筆", lstAB0075C.Count));

            //04.Ins Oracle ME_AB0075C
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAB0075C);
            l.clg(string.Format("匯入Oracle.ME_AB0075C共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 

        // select* from ME_DLLCTL where DLLCODE like 'AB0075D%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0075D%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0075D%'  -- 測試程式時使用
        // select count(*) from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        static void AB0075D_內湖門急汀洲住藥局工作日(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0075D";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0075D     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0075D mmrep_AB0075D = new MMRep_AB0075D();
            var lstAB0075D = mmrep_AB0075D.GetPharmacyWork<ME_AB0075DModles>(sDt, eDt);
            logLstData<ME_AB0075DModles>(lstAB0075D);
            l.clg(string.Format("查詢三總Sybase.HISDRUG 等Table組合資料，共{0}筆", lstAB0075D.Count));

            //04.Ins Oracle ME_AB0075D
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAB0075D);
            l.clg(string.Format("匯入Oracle.ME_AB0075D共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 


        // 【在漢翔跑測試程式前要執行這段才有測試資料可以測試】
        // update HISEXND set 
        // CREATEDATETIME = LTRIM(RTRIM(  -- 把資料改成測試當天的資料才有資料可以測試
        //     STR(convert(int,  substring(convert(char(10),getdate(),111),1,4)  )-1911) || 
        //     substring(convert(char(10),getdate(),111), 6,2) || 
        //     substring(convert(char(10),getdate(),111), 9,2)
        // )) ||  substring(CREATEDATETIME,8,13) 
        // WHERE PHARMOUTCODE = 'U' 
        // AND CREATEDATETIME LIKE '1081018%' 
        // AND ORDERCODE >= '005AA' 
        // AND ORDERCODE <= '00999' 
        // AND STOCKFLAG = 'Y' 
        // -------------
        // select* from ME_DLLCTL where DLLCODE like 'AB0075E%' -- 查詢是否有值
        // delete from ME_DLLCTL where DLLCODE like 'AB0075E%'                      -- 測試無資料的情況
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0075E%'  -- 測試程式時使用
        // select count(*) from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        // select * from ME_AB0079 where 1=1 and ORDERCODE='005TRA01' and DSM='M'  -- 檢查是否有從Sybase轉到Oracle
        static void AB0075E_琩高(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0075E";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, dllcode);    //01.以DLLCODE=AB0075E     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);             //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt
            eDt = eDt.Substring(0, 7);

            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0075E mmrep_AB0075E = new MMRep_AB0075E();
            var lstAB0075E = mmrep_AB0075E.GetUDWork<ME_AB0075EModles>(eDt);
            logLstData<ME_AB0075EModles>(lstAB0075E);
            l.clg(string.Format("查詢三總Sybase.HISEXND 等Table組合資料，共{0}筆", lstAB0075E.Count));

            //04.Ins Oracle ME_AB0075E
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAB0075E);
            l.clg(string.Format("匯入Oracle.ME_AB0075E共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + frame.GetMethod().Name + "...");
        } // 

        // select* from ME_DLLCTL where DLLCODE like 'AB0013%' -- 查詢是否有值
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0013%'  -- 測試程式時使用
        // select count(*) from ME_AB0013 where 1=1 -- and ORDERCODE='005TRA01' and DSM = 'M'-- 2631 檢查是否有從Sybase轉到Oracle
        static void AB0013_申請藥品補發(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0013";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_AB0013_table_log.txt";
            l.clg("開始匯入 AB0013_退藥短少金額統計表 ...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = getSDT(r, "AB0013"); //01.以DLLCODE=AB0013     查ME_DDLCTL.ENDDATE時間為scdt
            String eDt = getEDT(r);            //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt


            //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
            MMRep_AB0013 mmrep_AB0013 = new MMRep_AB0013();
            var lstAB0013 = mmrep_AB0013.GetDrugReissue<ME_AB0013Modles>(sDt, eDt);
            logLstData<ME_AB0013Modles>(lstAB0013);
            l.clg(string.Format("查詢三總Sybase.HISMEDD, INACARM, HISPROF 等Table組合資料，共{0}筆", lstAB0013.Count));


            //04.Ins Oracle ME_AB0013
            var repoB = new RepAB0079DRepository(DBWork);
            int ImpAfrsB = repoB.Import(lstAB0013);
            l.clg(string.Format("匯入Oracle.ME_AB0013共{0}筆", ImpAfrsB));


            updEndDate(r, dllcode, eDt); //05.Upd ME_DLLCTL.ENDDATE
            l.clg("結束匯入" + dllcode + "...");
        } // 

        // select* from ME_DLLCTL where DLLCODE like 'AB0012%' -- 查詢是否有值
        // update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0012%'  -- 測試程式時使用
        // select count(*) from ME_AB0012 where 1=1 -- and ORDERCODE='005TRA01' and DSM = 'M'-- 2631 檢查是否有從Sybase轉到Oracle

        //select* from ME_DLLCTL where DLLCODE like 'AB0012%' and WH_NO='41' -- 查詢是否有值
        //delete from ME_DLLCTL where DLLCODE like 'AB0012%' and WH_NO='41' -- 測試刪除
        //update ME_DLLCTL set ENDDATE=sysdate - 365 where DLLCODE like 'AB0012%' and WH_NO='41'  -- 測試程式時使用
        //select count(*) from ME_AB0012 where ordercode = '005TRA01'-- 檢查是否有從Sybase轉到Oracle
        //select* from ME_AB0012 where ordercode='005TRA01'   -- 檢查是否有從Sybase轉到Oracle
        static void AB0012_病房管制藥品給藥(UnitOfWork DBWork)
        {
            l = new L(exeName + ".Program");
            fl = new FL(exeName + ".Program");
            dllcode = "AB0012";
            hisbackFileName = "" + exeName + ".Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_" + dllcode + "_table_log.txt";
            StackTrace trace = new StackTrace(); StackFrame frame = trace.GetFrame(0);
            l.clg("開始匯入 " + frame.GetMethod().Name + "...");
            var r = new ME_DLLCTLRepository(DBWork);
            String sDt = "";
            String eDt = getEDT(r);            //02.取得TWN_DATE(SYSDATE)資料庫時間為ecdt

            // 03.讀取WH_NO(庫房) 
            var w = new MI_WHMASTRepository(DBWork);
            List<MI_WHMASTModels> lst_wh_no =  w.GetDist_WH_NO().ToList();
            l.clg("讀取 select distinct wh_no from MI_WHMAST where wh_kind = '0' and wh_grade >= '2' and wh_grade <> '5' 取得WH_NO共 " + lst_wh_no.Count +" 筆 ");
            foreach (MI_WHMASTModels v in lst_wh_no)
            {
                if (v.WH_NO == "41") // 在漢翔驗證資料使用
                {
                    String s = "";
                    s += "41"; // 供除錯時用
                }
                    if (true || v.WH_NO == "41") // 在漢翔驗證資料使用
                {
                    DBWork.BeginTransaction();

                    sDt = getSDT(r, v.WH_NO, "AB0012"); //01.以DLLCODE=AB0012     查ME_DDLCTL.ENDDATE時間為scdt

                    //03.呼叫三總介接函式GetBuyBackDrug(scdt, ecdt)取得資料
                    MMRep_AB0012 mmrep_AB0012 = new MMRep_AB0012();
                    var lstAB0012 = mmrep_AB0012.GetControlDrug<ME_AB0012Modles>(v.WH_NO, sDt, eDt);
                    logLstData<ME_AB0012Modles>(lstAB0012);
                    foreach (ME_AB0012Modles l in lstAB0012)
                    {
                        if (String.IsNullOrEmpty(l.RESTQTY))
                        {
                            l.RESTQTY = "0";
                        }
                    }
                    l.clg(string.Format("查詢三總Sybase.HISMEDD, INACARM, HISPROF 等Table組合資料，共{0}筆", lstAB0012.Count));


                    //04.Ins Oracle ME_AB0012
                    var repoB = new RepAB0079DRepository(DBWork);
                    int ImpAfrsB = repoB.Import(lstAB0012);
                    l.clg(string.Format("匯入Oracle.ME_" + dllcode + "共{0}筆", ImpAfrsB));


                    updEndDate(r, dllcode, v.WH_NO, eDt); //05.Upd ME_DLLCTL.ENDDATE

                    DBWork.Commit();
                }
            }
            l.clg("結束匯入" + dllcode + "...");
        } //

        // 原林江與吉威介接程式
        static void callMMSMSBAS()
        {
            /**
             * 呼叫林江開發的dll取得資料
             */
            TsghHisBAS db = new TsghHisBAS();
            var basstkaList = db.GetBASSTKA<BASSTKAModels>();
            var basordmList = db.GetBASORDM<BASORDMModels>();
            var basorddList = db.GetBASORDD<BASORDDModels>();

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
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



                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
            }

            Console.WriteLine("匯入完成");
        }


        static String exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        static L l = new L("HISBACK.Program");
        static FL fl = new FL("HISBACK.Program");
        static String hisbackFileName = "HISBACK.Program_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_MMRep_PhrP021_table_log.txt";
        static String dllcode; // 排程代碼

        // 中文亂碼問題 https://www.cjavapy.com/article/126/

        static void Main(string[] args)
        {
            // callMMSMSBAS(); // 原林江與吉威介接程式

            /**
             * 呼叫林江開發的dll取得資料
             */
            // BAS基本檔介接
            // TsghHisBAS db = new TsghHisBAS();
            //AidcBAS db = new AidcBAS();
            //var basstkaList = db.GetBASSTKA<BASSTKAModels>();
            //var basordmList = db.GetBASORDM<BASORDMModels>();
            //var basorddList = db.GetBASORDD<BASORDDModels>();

            //// -- 以下為林江(02-8792-3311轉19672)提供的程式 -- 
            ////報表AB0012介接至ME_AB0012(Oracle DB)
            //MMRep_AB0012 db1 = new MMRep_AB0012();
            ////var getPubDrugList = db1.GetControlDrug1<ME_AB0012Modles>("21", "1080611143100", "1080613151616");
            //var getPubDrugList = db1.GetControlDrug<ME_AB0012Modles>("31", "1080610000000", "1080610235959");
            
            ////報表AB0071買退藥清單介接至ME_AB0071(Oracle DB)
            //MMRep_AB0071 db3 = new MMRep_AB0071();
            //var getBuyBackDrugList = db3.GetBuyBackDrug_TEST<ME_AB0071Modles>();
            // -- 以上為林江提供的程式 -- 



            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction(); // 當是AB0012_病房管制藥品給藥時，要關掉
                try
                {
                    // -- AB0012系列 -- 
                    // AB0012_病房管制藥品給藥(DBWork);                      // AB0012.exe (資料庫資料仍無法備齊，整理中)

                    // -- HISBACK系列 -- 
                    HISBACK_處理病房退藥扣庫(DBWork);                     // HISBACK.exe wait

                    // -- AB0071系列 -- 
                    // AB0071_買退藥清單(DBWork);                            // ME_AB0071.exe wait

                    // -- AB0079系列 -- 
                    // AB0079D_每月醫生醫令統計(DBWork);                     // AB0079D.exe wait
                    // AB0079S_每月藥品醫令統計(DBWork);                     // AB0079S.exe wait
                    // AB0079M_每月科室醫令統計(DBWork);                     // AB0079M.exe wait

                    // -- AB0083系列 -- 
                    // AB0083A_退藥異常統計表(DBWork);                        // AB0083A.exe ok
                    // AB0083B_退藥短少金額統計表(DBWork);                    // AB0083B.exe ok
                    // AB0083C_退藥異常次數及金額統計表(DBWork);              // AB0083C.exe ok
                    // AB0083D_退藥異常總表(DBWork);                          // AB0083D.exe ok

                    // -- AB0075系列 -- 
                    // AB0075A_化療工作量依醫師確認日期(DBWork);                // AB0075A.exe  
                    // AB0075B_PCA每月工作量及項目統計(DBWork);              // AB0075B.exe
                    // AB0075C_TPN藥局(DBWork);                              // AB0075C.exe
                    // AB0075D_內湖門急汀洲住藥局工作日(DBWork);             // AB0075D.exe
                    // AB0075E_琩高(DBWork);                                 // AB0075E.exe

                    // -- AB0013系列 -- 
                    // AB0013_申請藥品補發(DBWork);                          // AB0083A.exe  (少如大哥確認不做)



                    // -- 以下為林江提供的程式 -- 
                    ////介接AB0012
                    //Console.WriteLine("開始匯入AB0012...");
                    //var repoA = new RepAB0012Repository(DBWork);
                    ////int delAfrsS = repoA.DeleteAll(); //基本檔才需刪除
                    ////Console.WriteLine(string.Format("刪除BASSTKA共{0}筆", delAfrsS));
                    //int ImpAfrsA = repoA.Import(getPubDrugList);
                    //Console.WriteLine(string.Format("匯入BASSTKA共{0}筆", ImpAfrsA));

                    ////介接AB0071
                    //Console.WriteLine("開始匯入AB0071買退藥清單...");
                    //var repoC = new RepAB0071Repository(DBWork);
                    ////int delAfrsS = repoA.DeleteAll(); //基本檔才需刪除
                    ////Console.WriteLine(string.Format("刪除BASSTKA共{0}筆", delAfrsS));
                    //int ImpAfrsC = repoC.Import(getBuyBackDrugList);
                    //Console.WriteLine(string.Format("匯入BASSTKA共{0}筆", ImpAfrsC));

                    // -- 以上為林江提供的程式
                    //// -- 以下為原來的程式
                    ////STKDMIT
                    //Console.WriteLine("開始匯入BASSTKA...");
                    //var repoS = new BASSTKARepository(DBWork);
                    //int delAfrsS = repoS.DeleteAll(); //基本檔才需刪除
                    //Console.WriteLine(string.Format("刪除BASSTKA共{0}筆", delAfrsS));

                    //int ImpAfrsS = repoS.Import(basstkaList);
                    //Console.WriteLine(string.Format("匯入BASSTKA共{0}筆", ImpAfrsS));

                    ////BASORDM
                    //Console.WriteLine("開始匯入BASORDM...");
                    //var repoM = new BASORDMRepository(DBWork);
                    //int delAfrsM = repoM.DeleteAll(); //基本檔才需刪除
                    //Console.WriteLine(string.Format("刪除BASORDM共{0}筆", delAfrsM));

                    //int ImpAfrsM = repoM.Import(basordmList);
                    //Console.WriteLine(string.Format("匯入BASORDM共{0}筆", ImpAfrsM));

                    ////BASORDD
                    //Console.WriteLine("開始匯入BASORDD...");
                    //var repoD = new BASORDDRepository(DBWork);
                    //int delAfrsD = repoD.DeleteAll(); //基本檔才需刪除
                    //Console.WriteLine(string.Format("刪除BASORDD共{0}筆", delAfrsD));

                    //int ImpAfrsD = repoD.Import(basorddList);
                    //Console.WriteLine(string.Format("匯入BASORDD共{0}筆", ImpAfrsD));

                    DBWork.Commit(); // 當是AB0012_病房管制藥品給藥時，要關掉
                    l.clg("排程執行完畢");

                    // 寄送mail通知admin有執行程式 
                    fl.sendMailToAdmin("三總排程-" + exeName +"執行完畢", "程式正常執行完畢", l.getLogDirPath() + "\\" + hisbackFileName);
                }
                catch (Exception ex)
                {
                    l.clg("排程執行錯誤:" + ex.Message);
                    l.le("Main()", ex.Message);
                    DBWork.Rollback();
                    // 寄送mail通知admin有執行程式 
                    fl.sendMailToAdmin("三總排程-" + exeName + "執行異常", "Exception=" + ex.Message, l.getLogDirPath() + "\\" + hisbackFileName);
                    throw;
                }
            }

            Console.WriteLine("匯入完成");
        } // 

        

    } // ec
} // en
