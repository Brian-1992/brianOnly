using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AA
{
    public class AA0180Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0180Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p1, p2, p3, User.Identity.Name, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0180Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse UpdateM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0180Repository(DBWork);
                    var docno = form.Get("DOCNO");
                    var seq = form.Get("SEQ");
                    var apvqty = form.Get("APVQTY");
                    string update_user = User.Identity.Name;
                    string update_ip = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(docno, seq, apvqty, update_user, update_ip);
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0180Repository repo = new AA0180Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        string rtn = string.Empty;
                        string v_docno = "";
                        string update_user = User.Identity.Name;
                        string update_ip = DBWork.ProcIP;

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsDN(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>補藥通知單編號(" + tmp[i] + ")明細尚有核可補藥量為0</span>不得核可。" + "<br/>";
                                DBWork.Rollback();
                                return session.Result;
                            }
                            v_docno = repo.GetDailyDocno(); // 取得每日單號 select GET_DAILY_DOCNO from DUAL 
                            if (!repo.CheckExists(v_docno)) // 新增前檢查主鍵是否已存在
                            {
                                //C.更新DGMISS
                                repo.ApplyD(tmp[i], v_docno, update_user, update_ip);
                                //c.新增ME_DOCM
                                repo.InsertMEDOCM(tmp[i], v_docno, update_user, update_ip);
                                //d.新增ME_DOCD
                                repo.InsertMEDOCD(tmp[i], v_docno, update_user, update_ip);
                                // 花蓮需求:核可後直接產生未核可申請單,這邊繼續做AB0127提出申請的部分
                                AB0127Repository ab0127repo = new AB0127Repository(DBWork);
                                ME_DOCM me_docm = new ME_DOCM();
                                me_docm.DOCNO = v_docno;
                                me_docm.TOWH = ab0127repo.getDocTowh(v_docno);
                                me_docm.DOCTYPE = ab0127repo.getDoctype(me_docm.TOWH);
                                me_docm.FLOWID = "0111";
                                me_docm.SENDAPVID = User.Identity.Name;
                                me_docm.UPDATE_USER = User.Identity.Name;
                                me_docm.UPDATE_IP = DBWork.ProcIP;
                                me_docm.ISARMY = ab0127repo.getDocIsarmy(v_docno);
                                me_docm.APPUNA = ab0127repo.getDocAppuna(v_docno);
                                IEnumerable<ME_DOCD> myEnum = ab0127repo.GetSplitValue(me_docm.DOCNO);
                                myEnum.GetEnumerator();
                                string item_FRWH = "", item_MCONTID = "";
                                foreach (var item in myEnum)
                                {
                                    // 第一個核撥庫房&合約識別
                                    if (item_FRWH == "" && item_MCONTID == "")
                                    {
                                        me_docm.FRWH = item.FRWH_D;
                                        me_docm.M_CONTID = item.M_CONTID;
                                        ab0127repo.MasterUpdateFrwhMcontid(me_docm);
                                    }
                                    else
                                    {
                                        // 拆單,並新建單號
                                        ME_DOCM me_docm_new = new ME_DOCM();
                                        me_docm_new.DOCNO = repo.GetDailyDocno();
                                        me_docm_new.CREATE_USER = User.Identity.Name;
                                        me_docm_new.UPDATE_USER = User.Identity.Name;
                                        me_docm_new.UPDATE_IP = DBWork.ProcIP;
                                        me_docm_new.APPID = User.Identity.Name;
                                        me_docm_new.APPLY_KIND = "2";
                                        me_docm_new.APPDEPT = DBWork.UserInfo.Inid;
                                        me_docm_new.USEDEPT = DBWork.UserInfo.Inid;
                                        me_docm_new.USEID = User.Identity.Name;
                                        me_docm_new.TOWH = me_docm.TOWH;        // 申請庫房
                                        me_docm_new.FRWH = item.FRWH_D;        // 核撥庫房
                                        me_docm_new.DOCTYPE = me_docm.DOCTYPE;
                                        me_docm_new.FLOWID = me_docm.FLOWID;
                                        me_docm_new.MAT_CLASS = "01";
                                        me_docm_new.SRCDOCNO = me_docm_new.DOCNO;
                                        me_docm_new.ISARMY = me_docm.ISARMY;
                                        me_docm_new.APPUNA = me_docm.APPUNA;
                                        me_docm_new.M_CONTID = item.M_CONTID;
                                        ab0127repo.CreateM(me_docm_new);
                                        // 將第二個核撥庫房+合約識別的項次,修改為新單號
                                        ab0127repo.DetailUpdateDocno(v_docno, item.FRWH_D, item.M_CONTID, me_docm_new.DOCNO, User.Identity.Name, DBWork.ProcIP);

                                        // 以便後面UpdateStatus更新FLOWID用
                                        me_docm.DOCNO = me_docm_new.DOCNO;
                                    }
                                    item_FRWH = item.FRWH_D;
                                    item_MCONTID = item.M_CONTID;

                                    // 提出申請更新me_docd.apl_contime, 預帶申請量
                                    session.Result.afrs = ab0127repo.ApplyD(me_docm);

                                    // 狀態更新
                                    session.Result.afrs = ab0127repo.ApplyM(me_docm);
                                }
                                // AB0127提出申請部分 end
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號</span>重複，請重新嘗試。";
                            }
                        }
                    }
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Cancel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0180Repository repo = new AA0180Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        string UPDATE_USER = User.Identity.Name;
                        string UPDATE_IP = DBWork.ProcIP;
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            session.Result.afrs = repo.Cancel(tmp[i].ToString(), UPDATE_USER, UPDATE_IP);
                        }
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
        public ApiResponse GetInidCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0180Repository repo = new AA0180Repository(DBWork);
                    session.Result.etts = repo.GetInidCombo();
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