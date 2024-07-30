using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WebApp.Controllers.AA
{
    public class AA0006Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0006Repository repo = new AA0006Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(p0, p1, p2, p5, p6);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form)
        {
            var PO_NO = form.Get("PO_NO");
            var MMCODE = form.Get("MMCODE");
            var SEQ = form.Get("SEQ");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0006Repository repo = new AA0006Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(PO_NO, MMCODE, SEQ);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateDetail(FormDataCollection form)
        {
            var BW_SQTY = form.Get("BW_SQTY");
            var DIST_QTY = form.Get("DIST_QTY");
            var DIST_MEMO = form.Get("DIST_MEMO");
            var PO_NO = form.Get("PO_NO");
            var MMCODE = form.Get("MMCODE");
            var SEQ = form.Get("SEQ");
            var PR_DEPT = form.Get("PR_DEPT");

            bool IsReqDataNotEmpty = (BW_SQTY != "") && (DIST_QTY != "") && (DIST_MEMO != "")
                && (PO_NO != "") && (MMCODE != "") && (SEQ != "") && (PR_DEPT != "");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0006Repository repo = new AA0006Repository(DBWork);

                    if (IsReqDataNotEmpty)
                    {
                        // 去除前端傳進來最後一個逗號
                        BW_SQTY = BW_SQTY.Substring(0, BW_SQTY.Length - 1);
                        DIST_QTY = DIST_QTY.Substring(0, DIST_QTY.Length - 1);
                        DIST_MEMO = DIST_MEMO.Substring(0, DIST_MEMO.Length - 1);
                        PO_NO = PO_NO.Substring(0, PO_NO.Length - 1);
                        MMCODE = MMCODE.Substring(0, MMCODE.Length - 1);
                        SEQ = SEQ.Substring(0, SEQ.Length - 1);
                        PR_DEPT = PR_DEPT.Substring(0, PR_DEPT.Length - 1);

                        string[] tmp_BW_SQTY = BW_SQTY.Split(',');
                        string[] tmp_DIST_QTY = DIST_QTY.Split(',');
                        string[] tmp_DIST_MEMO = DIST_MEMO.Split(',');
                        string[] tmp_PO_NO = PO_NO.Split(',');
                        string[] tmp_MMCODE = MMCODE.Split(',');
                        string[] tmp_SEQ = SEQ.Split(',');
                        string[] tmp_PR_DEPT = PR_DEPT.Split(',');

                        //判斷所有分割完的資料總數是否一樣
                        int DataLength = tmp_BW_SQTY.Length;
                        bool IsDataLengthSame = (tmp_DIST_QTY.Length == DataLength) &&
                            (tmp_DIST_MEMO.Length == DataLength) && (tmp_PO_NO.Length == DataLength) &&
                            (tmp_MMCODE.Length == DataLength) && (tmp_SEQ.Length == DataLength)
                            && (tmp_PR_DEPT.Length == DataLength);

                        if (IsDataLengthSame)
                        {
                            for (int DataIndex = 0; DataIndex < DataLength; DataIndex++)
                            {
                                AA0006_Detail tmp_Detail = new AA0006_Detail();
                                tmp_Detail.BW_SQTY = tmp_BW_SQTY[DataIndex];
                                tmp_Detail.DIST_QTY = tmp_DIST_QTY[DataIndex];
                                tmp_Detail.DIST_MEMO = tmp_DIST_MEMO[DataIndex];
                                tmp_Detail.PO_NO = tmp_PO_NO[DataIndex];
                                tmp_Detail.MMCODE = tmp_MMCODE[DataIndex];
                                tmp_Detail.SEQ = tmp_SEQ[DataIndex];
                                tmp_Detail.DIST_USER = User.Identity.Name;
                                tmp_Detail.PR_DEPT = tmp_PR_DEPT[DataIndex];
                                session.Result.afrs = repo.UpdateDetail(tmp_Detail);
                            }
                        }
                        //DBWork.Commit();

                        //DBWork.BeginTransaction();
                        //Update成功後呼叫SP
                        string SP_RetMsg = "";
                        string IsCallSPSuccess = repo.CallSP_NumberCheck(tmp_PO_NO[0], tmp_MMCODE[0],
                            DBWork.UserInfo.UserId, DBWork.ProcIP, out SP_RetMsg);

                        //SP呼叫成功→Y(結束)  SP呼叫失敗→N(將Detail狀態改回L)
                        if (IsCallSPSuccess == "Y")
                        {
                            DBWork.Commit();
                        }
                        else if (IsCallSPSuccess == "N")
                        {
                            DBWork.Rollback();
                            //for (int DataIndex = 0; DataIndex < DataLength; DataIndex++)
                            //{
                            //    AA0006_Detail tmp_Detail = new AA0006_Detail();
                            //    tmp_Detail.BW_SQTY = tmp_BW_SQTY[DataIndex];
                            //    tmp_Detail.DIST_QTY = tmp_DIST_QTY[DataIndex];
                            //    tmp_Detail.DIST_MEMO = tmp_DIST_MEMO[DataIndex];
                            //    tmp_Detail.PO_NO = tmp_PO_NO[DataIndex];
                            //    tmp_Detail.MMCODE = tmp_MMCODE[DataIndex];
                            //    tmp_Detail.SEQ = tmp_SEQ[DataIndex];
                            //    tmp_Detail.DIST_USER = User.Identity.Name;
                            //    tmp_Detail.PR_DEPT = tmp_PR_DEPT[DataIndex];
                            //    session.Result.afrs = repo.RestoreDetail(tmp_Detail);
                            //}
                            session.Result.msg = SP_RetMsg;
                            session.Result.success = false;
                        }
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    session.Result.msg = "資料儲存失敗，請通知系統管理人員。(PO_NO)";
                    session.Result.success = false;
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0006Repository repo = new AA0006Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAGEN_NO()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0006Repository repo = new AA0006Repository(DBWork);
                    session.Result.etts = repo.GetAGEN_NO();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPO_NO(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0006Repository repo = new AA0006Repository(DBWork);
                    session.Result.etts = repo.GetPO_NO(p0, p1, p2, p3);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 數量檢核
        /// </summary>
        /// <returns></returns>
        public ApiResponse NumberCheck(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                //UnitOfWork DBWork = session.UnitOfWork;
                //try
                //{
                //    AA0006Repository repo = new AA0006Repository(DBWork);

                //    var INID = DBWork.UserInfo.Inid;
                //    var WHNO = p4;
                //    var USER_ID = DBWork.UserInfo.UserId;
                //    var USER_IP = DBWork.ProcIP;

                //    var RetCode = repo.CallSP_NumberCheck(p0, INID, WHNO, USER_ID, USER_IP);
                //    if (RetCode == "ERR")
                //    {
                //        session.Result.msg += "數量檢核失敗<br>";
                //    }
                //}
                //catch
                //{
                //    throw;
                //}
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var mat_class = form.Get("mat_class");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0006Repository repo = new AA0006Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, mat_class, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAckDetails(FormDataCollection form) {
            string docno = form.Get("docno");
            string mmcode = form.Get("mmcode");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0006Repository(DBWork);
                    session.Result.etts = repo.GetAckDetails(docno, mmcode);
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
    }
}