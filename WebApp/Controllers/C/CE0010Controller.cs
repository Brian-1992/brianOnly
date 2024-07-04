using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using Newtonsoft.Json;


namespace WebApp.Controllers.C
{
    public class CE0010Controller : SiteBase.BaseApiController
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
                    var repo = new CE0010Repository(DBWork);
                    CE0010Repository.CHK_MAST_QUERY_PARAMS query = new CE0010Repository.CHK_MAST_QUERY_PARAMS();
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
                    var repo = new CE0010Repository(DBWork);
                    CE0010Repository.CHK_MAST_QUERY_PARAMS query = new CE0010Repository.CHK_MAST_QUERY_PARAMS();
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
                    CE0010Repository repo = new CE0010Repository(DBWork);

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
                try
                {
                    CE0010Repository repo = new CE0010Repository(DBWork);
                    CE0004Repository repoCE04 = new CE0004Repository(DBWork);
                    string spec_wh_no = repo.GetSpecWhNo(form.Get("CHK_NO"));
                    CHK_MAST mast = repo.GetChkMast(form.Get("CHK_NO"));

                    if (mast.CHK_STATUS != "2")
                    {
                        session.Result.success = false;
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        return session.Result;
                    }

                    var count = repoCE04.GetUndoneDetailCount(mast.CHK_NO);
                    if (count != "0") //沒盤完
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("盤點單號：{0} 尚有{1}品項未盤點", mast.CHK_NO, count);
                        return session.Result;
                    }

                    // 藥品庫 管制藥 日盤單 檢查是否有填盤差原因
                    if (mast.CHK_WH_KIND == "0" && mast.CHK_PERIOD == "D" &&
                        (mast.CHK_TYPE == "3" || mast.CHK_TYPE == "4"))
                    {
                        if (repoCE04.CheckChkRemark(mast.CHK_NO))
                        {
                            session.Result.success = false;
                            session.Result.msg = "有盤差項目未填備註，請填完後再完成盤點";
                            return session.Result;
                        }
                    }

                    CE0010Repository.CHK_MAST_QUERY_PARAMS query = new CE0010Repository.CHK_MAST_QUERY_PARAMS();
                    query.CHK_NO = form.Get("CHK_NO") == null ? "" : form.Get("CHK_NO");    // 三盤點單號
                    IEnumerable<CHK_DETAIL> myEnum = repo.GetGroupDetail(query);
                    myEnum.GetEnumerator();
                    foreach (var item in myEnum)
                    {
                        if (repo.IsExists(mast.CHK_NO1, item.MMCODE) == false)
                        {
                            CE0004Repository.INSERT_CHK_DETAIL_PARAMS CE0004Params = GetInsertParamGrade23(DBWork, mast, item);
                            CE0004Params.USER = DBWork.UserInfo.UserId;
                            CE0004Params.UPDATE_IP = DBWork.ProcIP;
                            CE0004Params.CHK_NO1 = mast.CHK_NO1;
                            repo.InsertDetailTot(CE0004Params);
                            continue;
                        }


                        CE0010Repository.UPDATE_CHK_DETAIL_PARAMS myParams = new CE0010Repository.UPDATE_CHK_DETAIL_PARAMS();
                        if (mast.CHK_WH_GRADE != "1")
                        {
                            myParams = GetUpdateParamsGrade23(DBWork, mast, item);
                        }
                        else
                        {
                            myParams = GetUpdateParamsGrade1(DBWork, mast, item);
                        }

                        myParams.USER = User.Identity.Name;
                        myParams.UPDATE_IP = DBWork.ProcIP;
                        repo.UpdateDetailTot(myParams);
                    }
                    session.Result.afrs = repo.UpdateStatus(form.Get("CHK_NO"), User.Identity.Name, DBWork.ProcIP);
                    session.Result.etts = new List<CHK_MAST>() { repoCE04.GetChkMast(form.Get("CHK_NO")) };
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public CE0010Repository.UPDATE_CHK_DETAIL_PARAMS GetUpdateParamsGrade1(IUnitOfWork DBWork, CHK_MAST mast, CHK_DETAIL detail)
        {
            CE0010Repository.UPDATE_CHK_DETAIL_PARAMS myParams = new CE0010Repository.UPDATE_CHK_DETAIL_PARAMS();
            CE0010Repository repo = new CE0010Repository(DBWork);

            string spec_wh_no = repo.GetSpecWhNo(mast.CHK_NO);
            string chk_wh_kind = repo.GetChkWhKind(mast.CHK_NO);

            string tempCHK_YM = mast.CHK_YM.Length > 5 ? mast.CHK_YM.Substring(0, 5) : mast.CHK_YM;

            myParams.CHK_NO = mast.CHK_NO; // 複盤點單號
            myParams.CHK_NO1 = mast.CHK_NO1; // 初盤點單號
            myParams.MMCODE = detail.MMCODE;
            //myParams.STORE_LOC = item.STORE_LOC;
            if (mast.CHK_WH_GRADE == "1")
            {
                if (mast.CHK_WH_KIND == "0" || mast.CHK_WH_KIND == "1")   // 0-藥品 & 1-衛材
                {
                    if (mast.CHK_WH_KIND == "0")
                    {
                        myParams.STORE_QTYC = repo.GetSTORE_QTYC_PH1S(myParams.CHK_NO, detail.MMCODE, "004" + detail.MMCODE.Substring(3, detail.MMCODE.Length - 3), spec_wh_no, mast.CHK_WH_GRADE);
                        myParams.STORE_QTYM = repo.GetSTORE_QTYM(mast.CHK_NO, detail.MMCODE, "004" + detail.MMCODE.Substring(3, detail.MMCODE.Length - 3), spec_wh_no);
                        myParams.STORE_QTYS = detail.STORE_QTYS;

                        myParams.STORE_QTY = myParams.STORE_QTYC;
                    }
                    else
                    {
                        myParams.STORE_QTYC = repo.GetSTORE_QTYC_GRADE1(mast.CHK_NO, detail.MMCODE, detail.MMCODE, spec_wh_no, mast.CHK_WH_GRADE);
                        myParams.STORE_QTYM = repo.GetSTORE_QTYM(mast.CHK_NO, detail.MMCODE, detail.MMCODE, spec_wh_no);
                        myParams.STORE_QTYS = detail.STORE_QTYS;

                        myParams.STORE_QTY = Convert.ToString(Convert.ToDouble(myParams.STORE_QTYC) + Convert.ToDouble(myParams.STORE_QTYM) + Convert.ToDouble(myParams.STORE_QTYS));
                    }
                }
                else   // E-能設 & C-通訊
                {
                    myParams.STORE_QTYC = repo.GetSTORE_QTYC_1(mast.CHK_NO, detail.MMCODE);
                    myParams.STORE_QTYM = repo.GetSTORE_QTYM_1(mast.CHK_NO, detail.MMCODE);
                    myParams.STORE_QTYS = repo.GetSTORE_QTYS_1(mast.CHK_NO, detail.MMCODE);

                    myParams.STORE_QTY = Convert.ToString(Convert.ToDouble(myParams.STORE_QTYC) + Convert.ToDouble(myParams.STORE_QTYM) + Convert.ToDouble(myParams.STORE_QTYS));
                }
            }
            myParams.STORE_QTY_N = myParams.STORE_QTY;
            myParams.CHK_QTY = repo.GetCHK_QTY(mast.CHK_NO, detail.MMCODE);
            myParams.APL_OUTQTY = repo.GetAPL_OUTQTY(detail.MMCODE, detail.WH_NO);
            if (mast.CHK_WH_KIND == "E" || mast.CHK_WH_KIND == "C")     // 能設通信盤點盤自己+戰備+學院
            {
                myParams.GAP_T = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - Convert.ToDouble(myParams.STORE_QTYC) - Convert.ToDouble(myParams.STORE_QTYM) - Convert.ToDouble(myParams.STORE_QTYS));
                myParams.GAP_C = myParams.GAP_T;
            }
            else if (mast.CHK_WH_KIND == "0")   // 藥庫盤點盤自己庫
            {
                myParams.GAP_T = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - Convert.ToDouble(myParams.STORE_QTYC));
                myParams.GAP_C = myParams.GAP_T;
            }
            else                                // 中央庫房盤點盤自己+戰備
            {
                myParams.GAP_T = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - Convert.ToDouble(myParams.STORE_QTYC) - Convert.ToDouble(myParams.STORE_QTYM));
                myParams.GAP_C = myParams.GAP_T;
            }
            myParams.PRO_LOS_QTY = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - Convert.ToDouble(myParams.STORE_QTY));
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
        public CE0010Repository.UPDATE_CHK_DETAIL_PARAMS GetUpdateParamsGrade23(IUnitOfWork DBWork, CHK_MAST mast, CHK_DETAIL detail)
        {
            CE0010Repository.UPDATE_CHK_DETAIL_PARAMS myParams = new CE0010Repository.UPDATE_CHK_DETAIL_PARAMS();
            CE0010Repository repo = new CE0010Repository(DBWork);
            CE0004Repository repo04 = new CE0004Repository(DBWork);

            string spec_wh_no = repo.GetSpecWhNo(mast.CHK_NO);
            MI_MAST mi_mast = repo.GetMiMast(detail.MMCODE);

            string tempCHK_YM = mast.CHK_YM.Length > 5 ? mast.CHK_YM.Substring(0, 5) : mast.CHK_YM;

            myParams.CHK_NO = mast.CHK_NO;   // 複盤點單號
            myParams.CHK_NO1 = mast.CHK_NO1; // 初盤點單號
            myParams.MMCODE = detail.MMCODE;
            //myParams.STORE_LOC = item.STORE_LOC;
            myParams.STORE_QTY = repo.GetSTORE_QTY(mast.CHK_NO, detail.MMCODE, mast.CHK_WH_GRADE);
            myParams.STORE_QTY_N = myParams.STORE_QTY;
            myParams.STORE_QTYC = myParams.STORE_QTY;
            myParams.STORE_QTYM = "0";
            myParams.STORE_QTYS = "0";
            myParams.APL_OUTQTY = repo.GetAPL_OUTQTY(detail.MMCODE, detail.WH_NO);
            myParams.CHK_QTY = repo.GetCHK_QTY(mast.CHK_NO, detail.MMCODE);    // 盤點量
            myParams.HIS_CONSUME_QTY_T = repo04.GetHisConsumeQtyT(mast.CHK_NO, detail.MMCODE);
            myParams.HIS_CONSUME_DATATIME = repo04.GetHisConsumeDatatime(mast.CHK_NO, detail.MMCODE);

            // 電腦量 = 原始電腦量-醫令扣庫數量
            myParams.STORE_QTY = Convert.ToString(Convert.ToDouble(myParams.STORE_QTY) - Convert.ToDouble(myParams.HIS_CONSUME_QTY_T));
            myParams.STORE_QTYC = myParams.STORE_QTY;

            myParams.GAP_T = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - (Convert.ToDouble(myParams.STORE_QTY)));
            myParams.GAP_C = myParams.GAP_T;

            // 扣庫，算盤盈虧
            if (mi_mast.M_TRNID == "1")
            {
                myParams.PRO_LOS_QTY = Convert.ToString(Convert.ToDouble(myParams.CHK_QTY) - Convert.ToDouble(myParams.STORE_QTY));
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
        public CE0004Repository.INSERT_CHK_DETAIL_PARAMS GetInsertParamGrade23(IUnitOfWork DBWork, CHK_MAST mast, CHK_DETAIL detail)
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
                    myParams.MISS_PERC = myParams.MISS_PER;
                }
                else
                {
                    myParams.MISS_PER = "0";
                    myParams.MISS_PERC = "0";
                }
            }
            // 不扣庫，算消耗
            if (mi_mast.M_TRNID == "2")
            {
                myParams.CONSUME_QTY = Math.Abs(Convert.ToDouble(myParams.GAP_T)).ToString();
                myParams.CONSUME_AMOUNT = Convert.ToString(Convert.ToDouble(myParams.CONSUME_QTY) * Convert.ToDouble(detail.M_CONTPRICE));
                myParams.MISS_PER = "0";
                myParams.MISS_PERC = "0";
            }

            return myParams;

        }
    }
}
