using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0004Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0004Repository(DBWork);
                    CE0004Repository.CHK_MAST_QUERY_PARAMS query = new CE0004Repository.CHK_MAST_QUERY_PARAMS();
                    //if (form.Get("p1").Trim() != "")
                    //{
                    //    string tmp = form.Get("p1").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    //    string[] tmp2 = tmp.Split('-');
                    //    string ym = Convert.ToString(Convert.ToInt32(tmp2[0]) - 1911) + tmp2[1];
                    //    query.DATA_YM = ym;
                    //}
                    //else
                    //    query.DATA_YM = "";

                    query.CHK_NO = form.Get("CHK_NO") == null ? "" : form.Get("CHK_NO");
                    query.CHK_WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO");
                    query.CHK_YM = form.Get("CHK_YM") == null ? "" : form.Get("CHK_YM");
                    query.CHK_KEEPER = User.Identity.Name;

                    session.Result.etts = repo.GetAll(query);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢
        [HttpPost]
        public ApiResponse AllDetail(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0004Repository(DBWork);
                    CE0004Repository.CHK_MAST_QUERY_PARAMS query = new CE0004Repository.CHK_MAST_QUERY_PARAMS();
                    //if (form.Get("p1").Trim() != "")
                    //{
                    //    string tmp = form.Get("p1").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    //    string[] tmp2 = tmp.Split('-');
                    //    string ym = Convert.ToString(Convert.ToInt32(tmp2[0]) - 1911) + tmp2[1];
                    //    query.DATA_YM = ym;
                    //}
                    //else
                    //    query.DATA_YM = "";

                    query.CHK_NO = form.Get("CHK_NO") == null ? "" : form.Get("CHK_NO");

                    session.Result.etts = repo.GetAllDetail(query);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailUpdate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0004Repository repo = new CE0004Repository(DBWork);

                    CHK_MAST mast = repo.GetChkMast(form.Get("CHK_NO"));
                    if (mast.CHK_STATUS != "2")
                    {
                        session.Result.success = false;
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        return session.Result;
                    }

                    CHK_DETAIL chk_detail = new CHK_DETAIL();
                    chk_detail.CHK_NO = form.Get("CHK_NO");
                    chk_detail.MMCODE = form.Get("MMCODE");
                    chk_detail.STORE_LOC = form.Get("STORE_LOC");
                    chk_detail.CHK_QTY = form.Get("CHK_QTY");
                    chk_detail.CHK_REMARK = form.Get("CHK_REMARK");
                    chk_detail.UPDATE_USER = User.Identity.Name;
                    chk_detail.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.DetailUpdate(chk_detail);

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
        public ApiResponse Finish(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                CE0004Repository repo = new CE0004Repository(DBWork);
                DBWork.BeginTransaction();
                try
                {
                    string spec_wh_no = repo.GetSpecWhNo(form.Get("CHK_NO"));
                    CE0004Repository.CHK_MAST_QUERY_PARAMS query = new CE0004Repository.CHK_MAST_QUERY_PARAMS();
                    query.CHK_NO = form.Get("CHK_NO") == null ? "" : form.Get("CHK_NO");
                    IEnumerable<CHK_DETAIL> myEnum = repo.GetGroupDetail(query);
                    CHK_MAST mast = repo.GetChkMast(form.Get("CHK_NO"));
                    if (mast.CHK_STATUS != "2") {
                        session.Result.success = false;
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        return session.Result;
                    }

                    var count = repo.GetUndoneDetailCount(query.CHK_NO);
                    if (count != "0") //沒盤完
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("盤點單號：{0} 尚有{1}品項未盤點", query.CHK_NO, count);
                        return session.Result;
                    }

                    // 藥品庫 管制藥 日盤單 檢查是否有填盤差原因
                    if (mast.CHK_WH_KIND == "0" && mast.CHK_PERIOD == "D" &&
                        (mast.CHK_TYPE == "3" || mast.CHK_TYPE == "4")) {
                        if (repo.CheckChkRemark(mast.CHK_NO)) {
                            session.Result.success = false;
                            session.Result.msg = "有盤差項目未填備註，請填完後再完成盤點";
                            return session.Result;
                        }
                    }

                    myEnum.GetEnumerator();
                    foreach (var item in myEnum)
                    {
                        CE0004Repository.INSERT_CHK_DETAIL_PARAMS myParams = new CE0004Repository.INSERT_CHK_DETAIL_PARAMS();

                        if (mast.CHK_WH_GRADE != "1")
                        {
                            myParams = GetInserParamGrade23(DBWork, mast, item);
                        }
                        else {
                            myParams = GetInserParamGrade1(DBWork, mast, item);
                        }

                        myParams.USER = User.Identity.Name;
                        myParams.UPDATE_IP = DBWork.ProcIP;
                        repo.InsertDetailTot(myParams);
                    }

                    session.Result.afrs = repo.UpdateStatus(form.Get("CHK_NO"), User.Identity.Name, DBWork.ProcIP);
                    session.Result.etts = new List<CHK_MAST>() { repo.GetChkMast(form.Get("CHK_NO")) };

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
        public CE0004Repository.INSERT_CHK_DETAIL_PARAMS GetInserParamGrade1(IUnitOfWork DBWork, CHK_MAST mast, CHK_DETAIL detail) {
            CE0004Repository repo = new CE0004Repository(DBWork);
            CE0004Repository.INSERT_CHK_DETAIL_PARAMS myParams = new CE0004Repository.INSERT_CHK_DETAIL_PARAMS();

            string spec_wh_no = repo.GetSpecWhNo(mast.CHK_NO);
            string tempCHK_YM = mast.CHK_YM.Length > 5 ? mast.CHK_YM.Substring(0, 5) : mast.CHK_YM;

            MI_MAST mi_mast = repo.GetMiMast(detail.MMCODE);

            myParams.CHK_NO = mast.CHK_NO;
            myParams.MMCODE = detail.MMCODE;
            if (mi_mast.M_CONTPRICE == null || mi_mast.M_CONTPRICE == string.Empty)
            {
                mi_mast.M_CONTPRICE = "0";
            }
            //myParams.STORE_LOC = item.STORE_LOC;
            if (mast.CHK_WH_KIND == "0" || mast.CHK_WH_KIND == "1")   // 0-藥品 & 1-衛材
            {
                if (mast.CHK_WH_KIND == "0")
                {
                    myParams.STORE_QTYC = repo.GetSTORE_QTYC_PH1S(myParams.CHK_NO, detail.MMCODE, "004" + detail.MMCODE.Substring(3, detail.MMCODE.Length - 3), spec_wh_no, mast.CHK_WH_GRADE);
                    myParams.STORE_QTYM = repo.GetSTORE_QTYM(mast.CHK_NO, detail.MMCODE, "004" + detail.MMCODE.Substring(3, detail.MMCODE.Length - 3), spec_wh_no);
                    myParams.LAST_QTYM = repo.GetLAST_QTYM(tempCHK_YM, "004" + detail.MMCODE.Substring(3, detail.MMCODE.Length - 3), spec_wh_no);

                    myParams.STORE_QTY = myParams.STORE_QTYC;
                }
                else
                {
                    myParams.STORE_QTYC = repo.GetSTORE_QTYC_GRADE1(mast.CHK_NO, detail.MMCODE, detail.MMCODE, spec_wh_no, mast.CHK_WH_GRADE);
                    myParams.STORE_QTYM = repo.GetSTORE_QTYM(mast.CHK_NO, detail.MMCODE, detail.MMCODE, spec_wh_no);
                    myParams.LAST_QTYM = repo.GetLAST_QTYM(tempCHK_YM, detail.MMCODE, spec_wh_no);

                    myParams.STORE_QTY = Convert.ToString(Convert.ToDouble(myParams.STORE_QTYC) + Convert.ToDouble(myParams.STORE_QTYM));
                }
                myParams.STORE_QTYS = detail.STORE_QTYS;
                myParams.LAST_QTYS = "0";
            }
            else   // E-能設 & C-通訊
            {
                myParams.STORE_QTYC = repo.GetSTORE_QTYC_1(mast.CHK_NO, detail.MMCODE);
                myParams.STORE_QTYM = repo.GetSTORE_QTYM_1(mast.CHK_NO, detail.MMCODE);
                myParams.STORE_QTYS = repo.GetSTORE_QTYS_1(mast.CHK_NO, detail.MMCODE);
                myParams.LAST_QTYM = repo.GetLAST_QTYM_1(tempCHK_YM, detail.MMCODE);
                myParams.LAST_QTYS = repo.GetLAST_QTYS_1(tempCHK_YM, detail.MMCODE);
                myParams.STORE_QTY = Convert.ToString(Convert.ToDouble(myParams.STORE_QTYC) + Convert.ToDouble(myParams.STORE_QTYM) + Convert.ToDouble(myParams.STORE_QTYS));
            }
            myParams.STORE_QTY_N = myParams.STORE_QTY;

            myParams.CHK_QTY = repo.GetCHK_QTY(mast.CHK_NO, detail.MMCODE);    // 盤點量
            if (mast.CHK_WH_KIND == "E" || mast.CHK_WH_KIND == "C")
            {
                myParams.GAP_T = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - Convert.ToDouble(myParams.STORE_QTYC) - Convert.ToDouble(myParams.STORE_QTYM) - Convert.ToDouble(myParams.STORE_QTYS));
                myParams.GAP_C = myParams.GAP_T;
            }
            else if (mast.CHK_WH_KIND == "0")
            {
                myParams.GAP_T = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - Convert.ToDouble(myParams.STORE_QTYC));
                myParams.GAP_C = myParams.GAP_T;
            }
            else {
                myParams.GAP_T = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - Convert.ToDouble(myParams.STORE_QTYC) - Convert.ToDouble(myParams.STORE_QTYM));
                myParams.GAP_C = myParams.GAP_T;
            }
            
            myParams.LAST_QTYC = repo.GetLAST_QTYC(tempCHK_YM, detail.MMCODE, detail.WH_NO);
            myParams.LAST_QTY = Convert.ToString(Convert.ToDouble(myParams.LAST_QTYC) + Convert.ToDouble(myParams.LAST_QTYM) + Convert.ToDouble(myParams.LAST_QTYS));
            myParams.APL_OUTQTY = repo.GetAPL_OUTQTY(detail.MMCODE, detail.WH_NO);
            myParams.PRO_LOS_QTY = Convert.ToString(Convert.ToDouble(myParams.GAP_T));
            myParams.PRO_LOS_AMOUNT = Convert.ToString(Convert.ToDouble(myParams.PRO_LOS_QTY) * Convert.ToDouble(detail.M_CONTPRICE));
            if (myParams.APL_OUTQTY != "0" && myParams.APL_OUTQTY != null)
            {
                myParams.MISS_PER = Convert.ToString(Convert.ToDouble(myParams.PRO_LOS_QTY) / Convert.ToDouble(myParams.APL_OUTQTY));
                myParams.MISS_PERC = Convert.ToString((Convert.ToDouble(myParams.GAP_C)) / Convert.ToDouble(myParams.APL_OUTQTY));
            }
            else
            {
                myParams.MISS_PER = myParams.PRO_LOS_QTY;
                myParams.MISS_PERC = myParams.GAP_C;
            }

            return myParams;

        }
        public CE0004Repository.INSERT_CHK_DETAIL_PARAMS GetInserParamGrade23(IUnitOfWork DBWork, CHK_MAST mast, CHK_DETAIL detail)
        {
            CE0004Repository repo = new CE0004Repository(DBWork);
            CE0004Repository.INSERT_CHK_DETAIL_PARAMS myParams = new CE0004Repository.INSERT_CHK_DETAIL_PARAMS();

            string spec_wh_no = repo.GetSpecWhNo(mast.CHK_NO);
            string chkym = mast.CHK_YM.Length > 5 ? mast.CHK_YM.Substring(0, 5) : mast.CHK_YM;

            MI_MAST mi_mast = repo.GetMiMast(detail.MMCODE);

            myParams.CHK_NO = mast.CHK_NO;
            myParams.MMCODE = detail.MMCODE;
            if (detail.M_CONTPRICE == null || detail.M_CONTPRICE == string.Empty)
            {
                detail.M_CONTPRICE = "0";
            }
            myParams.STORE_QTY = repo.GetSTORE_QTY(mast.CHK_NO, detail.MMCODE, mast.CHK_WH_GRADE);
            myParams.STORE_QTY_N = myParams.STORE_QTY;
            myParams.STORE_QTYC = myParams.STORE_QTY;
            myParams.STORE_QTYM = "0";
            myParams.STORE_QTYS = "0";
            myParams.LAST_QTY = repo.GetLAST_QTYC(chkym, detail.MMCODE, detail.WH_NO);
            myParams.LAST_QTYC = myParams.LAST_QTY;
            myParams.LAST_QTYM = "0";
            myParams.LAST_QTYS = "0";
            myParams.APL_OUTQTY = repo.GetAPL_OUTQTY(detail.MMCODE, detail.WH_NO);
            myParams.HIS_CONSUME_QTY_T = repo.GetHisConsumeQtyT(mast.CHK_NO, detail.MMCODE);
            myParams.HIS_CONSUME_DATATIME = repo.GetHisConsumeDatatime(mast.CHK_NO, detail.MMCODE);
            // 電腦量 = 原始電腦量-醫令扣庫數量
            myParams.STORE_QTY = Convert.ToString(Convert.ToDouble(myParams.STORE_QTY) - Convert.ToDouble(myParams.HIS_CONSUME_QTY_T));
            myParams.STORE_QTYC = myParams.STORE_QTY;

            myParams.CHK_QTY = repo.GetCHK_QTY(mast.CHK_NO, detail.MMCODE);    // 盤點量
            myParams.GAP_T = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - (Convert.ToDouble(myParams.STORE_QTY)));
            myParams.GAP_C = myParams.GAP_T;

            // 扣庫，算盤盈虧
            if (mi_mast.M_TRNID == "1")
            {
                myParams.PRO_LOS_QTY = myParams.GAP_T;
                myParams.PRO_LOS_AMOUNT = Convert.ToString(Convert.ToDouble(myParams.PRO_LOS_QTY) * Convert.ToDouble(detail.M_CONTPRICE));
                if (myParams.APL_OUTQTY != "0" && myParams.APL_OUTQTY != null)
                {
                    myParams.MISS_PER = Convert.ToString(Convert.ToDouble(myParams.PRO_LOS_QTY) / Convert.ToDouble(myParams.APL_OUTQTY));
                    myParams.MISS_PERC = Convert.ToString((Convert.ToDouble(myParams.GAP_C)) / Convert.ToDouble(myParams.APL_OUTQTY));
                }
                else
                {
                    myParams.MISS_PER = myParams.PRO_LOS_QTY;
                    myParams.MISS_PERC = myParams.GAP_C;
                }
            }
            // 不扣庫，算消耗
            if (mi_mast.M_TRNID == "2")
            {
                myParams.CONSUME_QTY = Convert.ToDouble(myParams.GAP_T).ToString();
                myParams.CONSUME_AMOUNT = Convert.ToString(Convert.ToDouble(myParams.CONSUME_QTY) * Convert.ToDouble(detail.M_CONTPRICE));
                myParams.MISS_PER = "0";
                myParams.MISS_PERC = "0";
            }


            return myParams;

        }

        [HttpPost]
        public ApiResponse Close(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    CE0004Repository repo = new CE0004Repository(DBWork);

                    CHK_MAST mast = repo.GetChkMast(form.Get("CHK_NO1"));
                    if (mast.CHK_STATUS != "3" && mast.CHK_STATUS != "C")
                    {
                        session.Result.success = false;
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        return session.Result;
                    }

                    SP_MODEL sp = repo.PostInvt(form.Get("CHK_NO1"),User.Identity.Name, DBWork.ProcIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = sp.O_ERRMSG;
                        return session.Result;
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CurrentStatus(FormDataCollection form) {
            string chk_no = form.Get("chk_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    CE0004Repository repo = new CE0004Repository(DBWork);
                    IEnumerable<CHK_DETAIL> temps  = repo.GetCurrentStatus(chk_no);
                    //foreach (CHK_DETAIL temp in temps) {
                    //    if (int.Parse(temp.UNDONE_NUM) == 0)
                    //    {
                    //        temp.DONE_STATUS = "已完成";
                    //    }
                    //    if (int.Parse(temp.UNDONE_NUM) > 0) {
                    //        temp.DONE_STATUS = "未完成";
                    //    }
                    //}

                    session.Result.etts = temps;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckCurrentYm(FormDataCollection form) {
            string chk_no = form.Get("chk_no");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    CE0004Repository repo = new CE0004Repository(DBWork);
                    string result = repo.CheckCurrentYm(chk_no);
                    session.Result.msg = result == "Y" ? "Y" : "N";
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}
