using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;

namespace WebApp.Controllers.C
{
    public class CC0005Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetLotNoFormData(FormDataCollection form)
        {
            var po_no = form.Get("po_no");
            var mmcode = form.Get("mmcode");
            var seq = form.Get("seq");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CC0005Repository repo = new CC0005Repository(DBWork);
                    //if (seq == "INREC")
                    //    session.Result.etts = repo.GetLotNoFormDataInrec(po_no, mmcode);
                    //else
                    session.Result.etts = repo.GetLotNoFormDataPhReply(po_no, mmcode, seq);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(CC0002D cc0002d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0005Repository(DBWork);
                    cc0002d.SEQ = repo.getSeq();
                    cc0002d.ACC_USER = User.Identity.Name;
                    if (repo.ChkDeli_Qty(cc0002d) == "N")
                    {
                        if (cc0002d.NEXTMON == "true" || cc0002d.NEXTMON == "TRUE")
                        {
                            cc0002d.NEXTMON = "Y";
                            cc0002d.STATUS = "N";
                            cc0002d.INQTY = cc0002d.INQTY_O; // 若為下月入帳,則取進貨數量預設值
                        }
                        else if (cc0002d.NEXTMON == "false" || cc0002d.NEXTMON == "FALSE")
                        {
                            cc0002d.NEXTMON = "N";
                            cc0002d.STATUS = "C";
                        }
                        /* 解決重複進貨的問題 
                         * 1.先檢查 PH_REPLY[STATUS]='B'是否還存在 ChkReplyStatus()
                         * 2.檢查 BC_CS_ACC_LOG 是否已新增 ChkAccLogExist()
                         * 3.重複進貨,前端回"接收數量存檔..成功", 讓使用者繼續
                        */
                        if (repo.ChkReplyStatus(cc0002d) == "B" && repo.ChkAccLogExist(cc0002d) == 0)
                        {
                            session.Result.afrs = repo.Create(cc0002d);
                            session.Result.afrs += repo.UpdatePhReply(cc0002d);

                            if (cc0002d.NEXTMON == "N")
                                session.Result.msg = repo.procDocin(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP); // 未勾選下個月入帳才call SP
                            else
                                session.Result.msg = "接收數量存檔..成功";

                            if (session.Result.msg == "接收數量存檔..成功")
                            {
                                if (cc0002d.WH_NO != "" && cc0002d.PURDATE != "")
                                {
                                    session.Result.msg = repo.procPoInrec(cc0002d.SEQ, User.Identity.Name, DBWork.ProcIP);
                                    if (session.Result.msg == "000") // 成功
                                    {
                                        DBWork.Commit();
                                        session.Result.msg = "接收數量存檔..成功";
                                    }
                                    else
                                    {
                                        DBWork.Rollback();
                                        session.Result.msg = "接收失敗";
                                    }
                                }
                                else
                                    DBWork.Commit();
                            }
                            else
                                DBWork.Rollback();
                        }
                        else
                        {
                            session.Result.msg = "此品項今日已接受過";
                        }
                    }
                    else
                    {
                        session.Result.msg = "接收數量累計超出訂單數量";
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        // 檢查MMCODE:
        [HttpPost]
        public ApiResponse ChkMmcode(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 院內碼

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    // 先檢查PH_REPLY,若無對應則到BC_BARCODE取院內碼
                    string rtnMsg = "";
                    var repo = new CC0005Repository(DBWork);
                    session.Result.etts = repo.GetPh_ReplyCombo(p0); // 若輸入的條碼或院內碼可直接在PH_REPLY對應到資料
                    if (((System.Collections.Generic.List<WebApp.Models.COMBO_MODEL>)session.Result.etts).Count == 0)
                    {
                        // 否則到BC_BARCODE找是否有對應的MMCODE
                        string barcodeChk = repo.ChkMmcodeBarcode(p0);
                        if (barcodeChk == "notfound")
                        {
                            rtnMsg = "條碼編號目前查無對應資料";
                        }
                        else
                        {
                            if (repo.ChkReceived(p0) > 0) //檢查當天是否有接收相同院內碼
                                rtnMsg = "此品項今日已接收過";
                        }
                    }
                    session.Result.msg = rtnMsg;
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