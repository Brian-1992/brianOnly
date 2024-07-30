using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0008Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {

            var wh_no = form.Get("p0");
            var chk_ym = form.Get("p1");
            var keeper = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0008Repository(DBWork);
                    string chk_status = "";
                    string chk_level = "3";
                    IEnumerable<CHK_MAST> masters = repo.GetChkMasts(wh_no, chk_ym, keeper, chk_status, chk_level, page, limit, sorters);
                    foreach (CHK_MAST master in masters)
                    {
                        master.CHK_TYPE_NAME = repo.GetChkWhkindName(master.CHK_WH_KIND, master.CHK_TYPE);
                    }
                    session.Result.etts = masters;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 新增

        [HttpPost]
        public ApiResponse GetMasterInsertList(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var chk_ym = form.Get("chk_ym");
            var keeper = form.Get("keeper");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0008Repository(DBWork);
                    IEnumerable<CHK_MAST> masters = repo.GetMasterInsertList(wh_no, chk_ym, keeper, page, limit, sorters);
                    foreach (CHK_MAST master in masters)
                    {
                        master.CHK_TYPE_NAME = repo.GetChkWhkindName(master.CHK_WH_KIND, master.CHK_TYPE);
                    }
                    session.Result.etts = masters;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse InsertMaster(CHK_MAST masterInput)
        {
            IEnumerable<CHK_MAST> masters = JsonConvert.DeserializeObject<IEnumerable<CHK_MAST>>(masterInput.ITEM_STRING);

            //CHK_MAST master = new CHK_MAST();
            //master.CHK_WH_NO = form.Get("CHK_CH_NO");
            //master.CHK_YM = form.Get("CHK_YM");
            //master.CHK_WH_GRADE = form.Get("CHK_WH_GRADE");
            //master.CHK_WH_KIND = form.Get("CHK_WH_KIND");
            //master.CHK_PERIOD = form.Get("CHK_PERIOD");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0008Repository(DBWork);
                    var repoCE0005 = new CE0005Repository(DBWork);
                    var repoCE0002 = new CE0002Repository(DBWork);
                    foreach (CHK_MAST master in masters)
                    {
                        string secondChkNo = master.CHK_NO;

                        if (master.CHK_PERIOD == "D")
                        {
                            master.CHK_YMD = master.CHK_YM;
                            master.CHK_YM = master.CHK_YM.Substring(0, 5);
                        }

                        master.CHK_NO1 = master.CHK_NO1;

                        string period = master.CHK_PERIOD == "D" ? "D" : "C";
                        string currentSeq = repoCE0002.GetCurrentSeq(master.CHK_WH_NO, master.CHK_YM);
                        master.CHK_NO = string.Format("{0}{1}{2}{3}{4}", master.CHK_WH_NO, master.CHK_YM, period, master.CHK_TYPE, currentSeq);

                        //master.CHK_NO = GetChkNo(master);
                        if (master.CHK_PERIOD == "D")
                        {
                            master.CHK_YM = master.CHK_YMD;
                        }
                        //master.CHK_KEEPER = DBWork.UserInfo.UserId;
                        master.CREATE_USER = DBWork.UserInfo.UserId;
                        master.UPDATE_USER = DBWork.UserInfo.UserId;
                        master.UPDATE_IP = DBWork.ProcIP;

                        //session.Result.afrs = repo.InsertChkMast(master);

                        if (repo.CheckExists(master.CHK_NO1)) {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = string.Format("已存在三盤資料，請重新選取。");
                            return session.Result;
                        }

                        session.Result.afrs = repo.InsertChkMast(master);
                        var preChkno = repoCE0005.GetPreChkno(master.CHK_NO1, "2");
                        session.Result.afrs = repoCE0005.UpdateChkno1(preChkno, master.UPDATE_USER, master.UPDATE_IP);


                        #region 中央庫房產生複盤單自動將數量不符之品項加入盤點 2020-01-30新增
                        // 若為中央庫房 wh_grade ='1' & wh_kind = '1'
                        if (master.CHK_WH_GRADE == "1" && master.CHK_WH_KIND == "1")
                        {
                            IEnumerable<CHK_DETAILTOT> tots = repo.GetNotMatchDetails(master.CHK_NO1);
                            foreach (CHK_DETAILTOT tot in tots)
                            {
                                CHK_DETAIL detail = new CHK_DETAIL();
                                detail.CHK_NO = master.CHK_NO;
                                detail.CHK_NO1 = master.CHK_NO1;
                                detail.MMCODE = tot.MMCODE;
                                detail.STATUS_INI = "1";
                                detail.CREATE_USER = DBWork.UserInfo.UserId;
                                detail.UPDATE_USER = detail.CREATE_USER;
                                detail.UPDATE_IP = DBWork.ProcIP;

                                session.Result.afrs = repo.InsertChkdetail(detail, master.CHK_WH_GRADE);

                            }

                            repoCE0002.DeleteDetailTempAll(master.CHK_NO);

                            if (tots.Count() > 0)
                            {
                                session.Result.afrs = repoCE0002.UpdateMaster(master.CHK_NO);
                            }
                        }
                        #endregion
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
        #endregion

        [HttpPost]
        public ApiResponse DeleteMaster(CHK_MAST masterInput)
        {
            IEnumerable<CHK_MAST> masters = JsonConvert.DeserializeObject<IEnumerable<CHK_MAST>>(masterInput.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    foreach (CHK_MAST master in masters)
                    {
                        // 先刪detail
                        session.Result.afrs = repo.DeleteChkDetail(master.CHK_NO);
                        // 後刪master
                        session.Result.afrs = repo.DeleteChkMast(master.CHK_NO);
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
        public ApiResponse GetDetailInclude(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0008Repository(DBWork);

                    IEnumerable<CHK_DETAIL> includes = repo.GetIncludeDetails(chk_no, page, limit, sorters);
                    includes = GetIncludeGroups(includes);
                    foreach (CHK_DETAIL include in includes)
                    {
                        string store_loc_name = string.Empty;
                        string store_loc = string.Empty;
                        int i = 0;
                        float inv_qty = 0;
                        foreach (CHK_DETAIL tempDetail in include.tempDetails)
                        {
                            i++;
                            if (tempDetail.STORE_QTYC == null || tempDetail.STORE_QTYC.Trim() == string.Empty)
                            {
                                tempDetail.STORE_QTYC = "0";
                            }
                            store_loc_name += (tempDetail.STORE_LOC + "-" + tempDetail.STORE_QTYC.ToString());
                            store_loc += (tempDetail.STORE_LOC);

                            inv_qty += float.Parse(tempDetail.STORE_QTYC);
                            if (i < include.tempDetails.Count())
                            {
                                store_loc_name += "<br>";
                                store_loc += ",";
                            }
                        }
                        include.STORE_LOC_NAME = store_loc_name;
                        include.STORE_LOC = store_loc;
                        include.STORE_QTYC = inv_qty.ToString();
                    }
                    session.Result.etts = includes;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public IEnumerable<CHK_DETAIL> GetIncludeGroups(IEnumerable<CHK_DETAIL> includes)
        {
            var o = from a in includes
                    group a by new
                    {
                        WH_NO = a.WH_NO,
                        MMCODE = a.MMCODE,
                        MMNAME_C = a.MMNAME_C,
                        MMNAME_E = a.MMNAME_E,
                        BASE_UNIT = a.BASE_UNIT,
                        M_PURUN = a.M_PURUN,
                        M_CONTPRICE = a.M_CONTPRICE,
                        MAT_CLASS = a.MAT_CLASS,
                        M_STOREID = a.M_STOREID,
                        CHK_QTY = a.CHK_QTY,
                        CHK_REMARK = a.CHK_REMARK,
                        CHK_UID = a.CHK_UID,
                        CHK_UID_NAME = a.CHK_UID_NAME,
                        STORE_LOC = a.STORE_LOC,
                        STORE_LOC_NAME = a.STORE_LOC_NAME,
                        STATUS_INI = a.STATUS_INI,
                        STATUS_INI_NAME = a.STATUS_INI_NAME,
                    } into g
                    select new CHK_DETAIL
                    {
                        WH_NO = g.Key.WH_NO,
                        MMCODE = g.Key.MMCODE,
                        MMNAME_C = g.Key.MMNAME_C,
                        MMNAME_E = g.Key.MMNAME_E,
                        BASE_UNIT = g.Key.BASE_UNIT,
                        M_PURUN = g.Key.M_PURUN,
                        M_CONTPRICE = g.Key.M_CONTPRICE,
                        MAT_CLASS = g.Key.MAT_CLASS,
                        M_STOREID = g.Key.M_STOREID,
                        CHK_QTY = g.Key.CHK_QTY,
                        CHK_REMARK = g.Key.CHK_REMARK,
                        CHK_UID = g.Key.CHK_UID,
                        CHK_UID_NAME = g.Key.CHK_UID_NAME,
                        STORE_LOC = g.Key.STORE_LOC,
                        STORE_LOC_NAME = g.Key.STORE_LOC_NAME,
                        STATUS_INI = g.Key.STATUS_INI,
                        STATUS_INI_NAME = g.Key.STATUS_INI_NAME,
                        tempDetails = g.ToList()
                    };

            return o.ToList();
        }

        [HttpPost]
        public ApiResponse GetDetailExclude(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var chk_no1 = form.Get("chk_no1");
            var list = form.Get("list");
            var wh_no = form.Get("wh_no");
            var f_u_price = form.Get("F_U_PRICE") == null ? 0 : float.Parse(form.Get("F_U_PRICE"));
            var f_number = form.Get("F_NUMBER") == null ? 0 : float.Parse(form.Get("F_NUMBER"));
            var f_amount = form.Get("F_AMOUNT") == null ? 0 : float.Parse(form.Get("F_AMOUNT"));
            var f_mmcode = form.Get("F_MMCODE");

            //IEnumerable<CHK_DETAIL> exists = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(list);

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0008Repository(DBWork);
                    IEnumerable<CHK_DETAIL> excludes = repo.GetExcludeDetails(chk_no, chk_no1, wh_no, f_u_price, f_number, f_amount, f_mmcode, page, limit, sorters);

                    if (wh_no == "PH1S")
                    {
                        foreach (CHK_DETAIL exclude in excludes)
                        {
                            exclude.STORE_QTYC = exclude.NEW_STORE_QTYC;
                        }
                    }
                    //excludes = GetIncludeGroups(excludes);
                    //foreach (CHK_DETAIL exclude in excludes)
                    //{
                    //    float inv_qty = 0;
                    //    foreach (CHK_DETAIL tempDetail in exclude.tempDetails)
                    //    {
                    //        inv_qty += float.Parse(tempDetail.STORE_QTYC);
                    //    }
                    //    exclude.INV_QTY = inv_qty;
                    //    exclude.STORE_QTYC = exclude.INV_QTY.ToString();
                    //    exclude.QTY_DIFF = float.Parse(exclude.CHK_QTY) - exclude.INV_QTY;
                    //}

                    session.Result.etts = excludes;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailSave(FormDataCollection form)
        {
            //IEnumerable<CHK_DETAIL> details = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(detailInput.ITEM_STRING);
            var chk_no = form.Get("chk_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    var repoCE0008 = new CE0008Repository(DBWork);
                    //CHK_DETAIL firstItem = details.First<CHK_DETAIL>();
                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(chk_no);
                    CHK_MAST mast = repoCE0008.GetChkMast(chk_no);

                    IEnumerable<CHK_DETAIL_TEMP> temps = repo.DetailTempAll(chk_no, string.Empty);
                    if (mast.CHK_WH_KIND == "E" || mast.CHK_WH_KIND == "C")
                    {
                        foreach (CHK_DETAIL_TEMP temp in temps)
                        {
                            CHK_DETAIL detail = new CHK_DETAIL()
                            {
                                CHK_NO = chk_no,
                                WH_NO = temp.WH_NO,
                                MMCODE = temp.MMCODE,
                                STATUS_INI = "0",
                                CHK_UID = temp.CHK_UID,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP
                            };
                            session.Result.afrs = repoCE0008.InsertChkdetailCE(detail);
                        }
                    }
                    else
                    if (mast.CHK_WH_GRADE == "1" && mast.CHK_WH_KIND != "E" && mast.CHK_WH_KIND != "C")
                    {
                        if (mast.CHK_WH_NO == "PH1S")
                        {
                            foreach (CHK_DETAIL_TEMP temp in temps)
                            {
                                CHK_DETAIL detail = new CHK_DETAIL()
                                {
                                    CHK_NO = chk_no,
                                    CHK_NO1 = mast.CHK_NO1,
                                    WH_NO = temp.WH_NO,
                                    MMCODE = temp.MMCODE,
                                    STATUS_INI = "0",
                                    CHK_UID = temp.CHK_UID,
                                    CREATE_USER = DBWork.UserInfo.UserId,
                                    UPDATE_USER = DBWork.UserInfo.UserId,
                                    UPDATE_IP = DBWork.ProcIP
                                };
                                session.Result.afrs = repoCE0008.InsertChkdetailPH1S(detail, false);
                            }
                        }
                        else
                        {
                            foreach (CHK_DETAIL_TEMP temp in temps)
                            {
                                CHK_DETAIL detail = new CHK_DETAIL()
                                {
                                    CHK_NO = chk_no,
                                    CHK_NO1 = mast.CHK_NO1,
                                    WH_NO = temp.WH_NO,
                                    MMCODE = temp.MMCODE,
                                    STATUS_INI = "0",
                                    CHK_UID = temp.CHK_UID,
                                    CREATE_USER = DBWork.UserInfo.UserId,
                                    UPDATE_USER = DBWork.UserInfo.UserId,
                                    UPDATE_IP = DBWork.ProcIP
                                };
                                session.Result.afrs = repo.InsertChkdetail(detail, mast.CHK_WH_GRADE, mast.CHK_WH_KIND);
                            }
                        }
                    }
                    else
                    {
                        foreach (CHK_DETAIL_TEMP temp in temps)
                        {
                            CHK_DETAIL detail = new CHK_DETAIL()
                            {
                                CHK_NO = chk_no,
                                CHK_NO1 = mast.CHK_NO1,
                                WH_NO = temp.WH_NO,
                                MMCODE = temp.MMCODE,
                                STATUS_INI = "0",
                                CHK_UID = "",
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP
                            };
                            if (repoCE0008.CheckDetailExists(mast.CHK_NO1, detail.MMCODE) == false)
                            {
                                session.Result.afrs = repoCE0008.InsertChkDetailFromTemp(detail);
                            }
                            else
                            {
                                session.Result.afrs = repoCE0008.InsertChkdetail(detail, mast.CHK_WH_GRADE);
                            }
                        }
                    }

                    repo.DeleteDetailTempAll(chk_no);

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
        public ApiResponse CreateSheet(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var chk_wh_kind = form.Get("chk_wh_kind");
            var chk_wh_grade = form.Get("chk_wh_grade");
            var wh_no = form.Get("wh_no");
            var usersString = form.Get("users") == null ? string.Empty : form.Get("users");
            bool is_distri = (form.Get("is_distri") == "true");
            var orderway = form.Get("orderway");
            var chk_no1 = form.Get("chk_no1");
            bool rechoose = (form.Get("rechoose") == "true");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<BC_WHCHKID> users = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(usersString);

                try
                {
                    var repo = new CE0002Repository(DBWork);
                    var repoCE0008 = new CE0008Repository(DBWork);

                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(chk_no);
                    //string chk_wh_grade = repo.GetChkWhGrade(chk_no);

                    IEnumerable<CHK_DETAIL_TEMP> temps = repo.DetailTempAll(chk_no, string.Empty);
                    if (chk_wh_kind == "E" || chk_wh_kind == "C")
                    {
                        foreach (CHK_DETAIL_TEMP temp in temps)
                        {
                            CHK_DETAIL detail = new CHK_DETAIL()
                            {
                                CHK_NO = chk_no,
                                WH_NO = temp.WH_NO,
                                MMCODE = temp.MMCODE,
                                STATUS_INI = "1",
                                CHK_UID = DBWork.UserInfo.UserId,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP,
                                WH_KIND = chk_wh_kind
                            };
                            session.Result.afrs = repo.InsertChkdetailCE(detail);
                        }
                    }
                    else
                    if (chk_wh_grade == "1" && chk_wh_kind != "E" && chk_wh_kind != "C")
                    {
                        if (chk_wh_kind == "0")
                        {
                            if (rechoose)
                            {
                                IEnumerable<CHK_DETAIL_TEMP> detail_temps = repo.DetailTempOrder(chk_no, new CE0002Repository.Sorter() { property = orderway });
                                IEnumerable<CHK_DETAIL> details = GetChkDetails(detail_temps, is_distri);

                                if (is_distri)
                                {
                                    details = GetDistriDetails(details, users).ToList();
                                }
                                else
                                {
                                    foreach (BC_WHCHKID user in users)
                                    {
                                        CHK_NOUID nouid = new CHK_NOUID()
                                        {
                                            CHK_NO = chk_no,
                                            CHK_UID = user.WH_CHKUID,
                                            CREATE_USER = DBWork.UserInfo.UserId,
                                            UPDATE_USER = DBWork.UserInfo.UserId,
                                            UPDATE_IP = DBWork.ProcIP
                                        };

                                        session.Result.afrs = repo.InsertChknouid(nouid);
                                    }
                                }

                                foreach (CHK_DETAIL detail in details)
                                {
                                    detail.CHK_NO1 = chk_no1;
                                    detail.CREATE_USER = DBWork.UserInfo.UserId;
                                    detail.UPDATE_USER = DBWork.UserInfo.UserId;
                                    detail.UPDATE_IP = DBWork.ProcIP;

                                    session.Result.afrs = repoCE0008.InsertChkdetailPH1S(detail, true);
                                }
                            }
                            else
                            {
                                foreach (CHK_DETAIL_TEMP temp in temps)
                                {
                                    CHK_DETAIL detail = new CHK_DETAIL()
                                    {
                                        CHK_NO = chk_no,
                                        CHK_NO1 = chk_no1,
                                        WH_NO = temp.WH_NO,
                                        MMCODE = temp.MMCODE,
                                        STATUS_INI = "1",
                                        CHK_UID = temp.CHK_UID,
                                        CREATE_USER = DBWork.UserInfo.UserId,
                                        UPDATE_USER = DBWork.UserInfo.UserId,
                                        UPDATE_IP = DBWork.ProcIP
                                    };

                                    session.Result.afrs = repoCE0008.InsertChkdetailPH1S(detail, false);

                                }
                            }
                        }
                        else
                        {
                            foreach (CHK_DETAIL_TEMP temp in temps)
                            {
                                CHK_DETAIL detail = new CHK_DETAIL()
                                {
                                    CHK_NO = chk_no,
                                    WH_NO = temp.WH_NO,
                                    MMCODE = temp.MMCODE,
                                    STATUS_INI = "1",
                                    CHK_UID = temp.CHK_UID,
                                    CREATE_USER = DBWork.UserInfo.UserId,
                                    UPDATE_USER = DBWork.UserInfo.UserId,
                                    UPDATE_IP = DBWork.ProcIP
                                };
                                session.Result.afrs = repo.InsertChkdetail(detail, chk_wh_grade, chk_wh_kind);
                            }
                        }
                    }
                    else
                    {
                        foreach (CHK_DETAIL_TEMP temp in temps)
                        {
                            CHK_DETAIL detail = new CHK_DETAIL()
                            {
                                CHK_NO = chk_no,
                                WH_NO = temp.WH_NO,
                                MMCODE = temp.MMCODE,
                                STATUS_INI = "1",
                                CHK_UID = temp.CHK_UID,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP
                            };
                            session.Result.afrs = repoCE0008.InsertChkdetail(detail, chk_wh_grade);
                        }
                    }

                    repo.DeleteDetailTempAll(chk_no);

                    session.Result.afrs = repo.UpdateMaster(chk_no);

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

    
    public IEnumerable<CHK_DETAIL> GetChkDetails(IEnumerable<CHK_DETAIL_TEMP> temps, bool is_distri)
    {
        List<CHK_DETAIL> list = new List<CHK_DETAIL>();
        foreach (CHK_DETAIL_TEMP temp in temps)
        {
            CHK_DETAIL detail = new CHK_DETAIL()
            {
                CHK_NO = temp.CHK_NO,
                WH_NO = temp.WH_NO,
                MMCODE = temp.MMCODE,
                CHK_UID = is_distri ? temp.CHK_UID: string.Empty
            };
            list.Add(detail);
        }
        return list;
    }
    public IEnumerable<CHK_DETAIL> GetDistriDetails(IEnumerable<CHK_DETAIL> details, IEnumerable<BC_WHCHKID> users)
    {
        // 每人應分配項數
        int base_num = details.Count() / users.Count();
        // 零頭數
        int mod_num = details.Count() % users.Count();
        int group_count = 0;
        int index = 0;

        foreach (CHK_DETAIL detail in details)
        {
            detail.STORE_QTYC = detail.INV_QTY.ToString();
            detail.CHK_QTY = "";
            detail.CHK_REMARK = "";
            detail.CHK_TIME = "";
            detail.STATUS_INI = "1";

            if (group_count < base_num)
            {
                group_count++;
                detail.CHK_UID = users.Skip(index).Take(1).FirstOrDefault<BC_WHCHKID>().WH_CHKUID;
            }
            else if (group_count == base_num && mod_num > 0)
            {
                mod_num--;
                group_count = 0;
                detail.CHK_UID = users.Skip(index).Take(1).FirstOrDefault<BC_WHCHKID>().WH_CHKUID;
                index++;
            }
            else if (group_count == base_num && mod_num == 0)
            {
                group_count = 1;
                index++;
                detail.CHK_UID = users.Skip(index).Take(1).FirstOrDefault<BC_WHCHKID>().WH_CHKUID;

            }
        }

        return details;
    }


    [HttpPost]
        public ApiResponse CreateSheetWard(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var chk_no1 = form.Get("chk_no1");
            var chk_wh_kind = form.Get("chk_wh_kind");
            var chk_wh_grade = form.Get("chk_wh_grade");
            var wh_no = form.Get("wh_no");
            var usersString = form.Get("users") == null ? string.Empty : form.Get("users");
            var preLevel = form.Get("preLevel");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    var repoCE0005 = new CE0005Repository(DBWork);
                    var repoCE0008 = new CE0008Repository(DBWork);

                    IEnumerable<BC_WHCHKID> users = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(usersString);
                    if (users.Any() == false)
                    {
                        string preChkno = repoCE0005.GetPreChkno(chk_no1, preLevel);
                        users = repoCE0005.GetChknoids(preChkno);
                    }

                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(chk_no);
                    IEnumerable<CHK_DETAIL_TEMP> temps = repo.DetailTempAll(chk_no, string.Empty);

                    foreach (CHK_DETAIL_TEMP temp in temps)
                    {
                        CHK_DETAIL detail = new CHK_DETAIL()
                        {
                            CHK_NO = chk_no,
                            CHK_NO1 = chk_no1,
                            WH_NO = temp.WH_NO,
                            MMCODE = temp.MMCODE,
                            STATUS_INI = "1",
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP
                        };
                        if (repoCE0008.CheckDetailExists(chk_no1, detail.MMCODE) == false)
                        {
                            session.Result.afrs = repoCE0008.InsertChkDetailFromTemp(detail);
                        }
                        else
                        {
                            session.Result.afrs = repoCE0008.InsertChkdetail(detail, chk_wh_grade);
                        }
                    }

                    foreach (BC_WHCHKID user in users)
                    {
                        CHK_NOUID nouid = new CHK_NOUID()
                        {
                            CHK_NO = chk_no,
                            CHK_UID = user.WH_CHKUID,
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP
                        };

                        session.Result.afrs = repo.InsertChknouid(nouid);
                    }

                    session.Result.afrs = repo.UpdateMaster(chk_no);

                    repo.DeleteDetailTempAll(chk_no);

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

        #region combo
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0008Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion
    }
}