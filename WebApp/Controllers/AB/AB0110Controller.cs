using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json.Linq;
using WebApp.Models.AB;
using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using System.Net.Http;

namespace WebApp.Controllers.AB
{
    public class AB0110Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse CheckCrDoc(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0110Repository(DBWork);
                    // 檢查是否有單號
                    if (repo.CheckCrDocNoExists(crdocno) == false) {
                        session.Result.success = false;
                        session.Result.msg = "查無單號，請重新確認";
                        return session.Result;
                    }

                    // 檢查狀態
                    string invalid_status = repo.CheckCrDocStatuInvalid(crdocno);
                    if (string.IsNullOrEmpty(invalid_status) == false) {
                        session.Result.success = false;
                        session.Result.msg = string.Format("狀態：{0}，不可點收", invalid_status);
                        return session.Result;
                    }

                    // 檢查入庫庫房
                    string invalid_wh_name = repo.CheckCrDocWhNoInvalid(crdocno, DBWork.UserInfo.UserId);
                    if (string.IsNullOrEmpty(invalid_wh_name) == false) {
                        session.Result.success = false;
                        session.Result.msg = string.Format("入庫庫房：{0}，不可點收", invalid_wh_name);
                        return session.Result;
                    }

                    session.Result.success = true;

                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
                
                // 檢查狀態

            }
        }

        [HttpPost]
        public ApiResponse Master(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0110Repository(DBWork);
                    session.Result.etts = repo.GetMaster(crdocno);

                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Details(FormDataCollection form)
        {
            string crdocno = form.Get("crdocno");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0110Repository(DBWork);
                    session.Result.etts = repo.GetDetails(crdocno);

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse InsertSingle(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string inqty = form.Get("inqty");
            string lot_no = form.Get("lot_no");
            string exp_date = form.Get("exp_date");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0110Repository(DBWork);

                    

                    session.Result.afrs = repo.Insert(crdocno, lot_no, exp_date, inqty, "N", DBWork.UserInfo.UserId, DBWork.ProcIP);

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Update(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string cr_d_seq = form.Get("cr_d_seq");
            string inqty = form.Get("inqty");
            string lot_no = form.Get("lot_no");
            string exp_date = form.Get("exp_date");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0110Repository(DBWork);



                    session.Result.afrs = repo.Update(crdocno, cr_d_seq, lot_no, exp_date, inqty,  DBWork.UserInfo.UserId, DBWork.ProcIP);

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Delete(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string cr_d_seq = form.Get("cr_d_seq");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0110Repository(DBWork);

                    session.Result.afrs = repo.Delete(crdocno, cr_d_seq);

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse CheckUdiData(FormDataCollection form)
        {
            string barcode = form.Get("barcode");
            string ackmmcode = form.Get("ackmmcode");
            string crdocno = form.Get("crdocno");

            using (WorkSession session = new WorkSession())
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        DecodeRequest request = new DecodeRequest();//請先建立POST 物件Class 見本原始碼最底處
                        request.WmCmpy = "04125805A"; //三總統編
                        //request.CrVmpy = "";
                        request.WmOrg = "010802";
                        request.WmWhs = "010802W";
                        //request.WmOrg = "";
                        //request.WmWhs = "";
                        //request.WmSku = "08080722-088";
                        request.WmSku = "";
                        request.WmPak = "";
                        request.WmQy = "";
                        request.WmLot = "";
                        request.WmEffcDate = "";
                        request.WmSeno = "";
                        request.WmBox = "";
                        request.WmLoc = "";
                        request.WmSrv = "";
                        //request.CrItm = "";
                        //request.ThisBarcode = "01047119081201531730123110LOT123"; //UDI 條碼
                        request.ThisBarcode = barcode; //UDI 條碼
                        request.UdiBarcodes = "";
                        request.NhiBarcodes = "";
                        request.GtinString = "";
                        //request.Check();

                        client.BaseAddress = new Uri("https://tsghudi.ndmctsgh.edu.tw/api/DecodeInfo"); //請於ServerIP 填入院內伺服器IP及PORT
                        string stringData = JsonConvert.SerializeObject(request);
                        var data = new StringContent(stringData, System.Text.Encoding.UTF8, "application/json");
                        HttpResponseMessage response = client.PostAsync(client.BaseAddress, data).Result;
                        string newMsg = response.Content.ReadAsStringAsync().Result;
                        // string newMsg = "[{\"WmBox\":\"\",\"WmLoc\":\"\",\"WmSrv\":\"\",\"IsChgItm\":\"N\",\"WmCmpy\":null,\"WmWhs\":null,\"WmOrg\":\"010802\",\"CrVmpy\":\"\",\"CrItm\":\"\",\"WmRefCode\":\"\",\"WmSku\":\"08080722-088\",\"WmMid\":\"08080722\",\"WmMidName\":\"LAUNCHER GUIDE CATHETERS\",\"WmMidNameH\":\"馬克導引導管\",\"WmSkuSpec\":\"MODEL-6F MACH 1 JL4\",\"WmBrand\":\"\",\"WmMdl\":\"\",\"WmMidCtg\":\"SKU\",\"WmEffcDate\":\"\",\"WmLot\":\"\",\"WmSeno\":\"\",\"WmPak\":\"個\",\"WmQy\":\"1\",\"ThisBarcode\":\"8714729351863\",\"UdiBarcodes\":\"8714729351863\",\"GtinString\":\"8714729351863<;>0871\",\"NhiBarcode\":\"(01)8714729351863\",\"NhiBarcodes\":\"(01)8714729351863\",\"BarcodeType\":\"UDI\",\"GtinInString\":\"'8714729351863' '08714729351863'\",\"Result\":\"SUCCESS\",\"ErrMsg\":\"\"}]";
                        newMsg = newMsg.Replace(",\"", "^");
                        newMsg = newMsg.Replace("\"", "");
                        newMsg = newMsg.Replace("[{", "");
                        newMsg = newMsg.Replace("}]", "");
                        newMsg = newMsg.Replace(@"""", "");

                        string[] strArray = newMsg.Split(new Char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
                        DecodeResponse deResponse = new DecodeResponse();

                        foreach (string innerStr in strArray)
                        {
                            string dataString = innerStr.Substring(innerStr.IndexOf(":") + 1);
                            if (dataString == "null")
                                dataString = "";

                            if (innerStr.IndexOf("WmBox:") >= 0)
                                deResponse.WmBox = dataString;
                            else if (innerStr.IndexOf("WmLoc:") >= 0)
                                deResponse.WmLoc = dataString;
                            else if (innerStr.IndexOf("WmSrv:") >= 0)
                                deResponse.WmSrv = dataString;
                            else if (innerStr.IndexOf("IsChgItm:") >= 0)
                                deResponse.IsChgItm = dataString;
                            else if (innerStr.IndexOf("WmCmpy:") >= 0)
                                deResponse.WmCmpy = dataString;
                            else if (innerStr.IndexOf("WmWhs:") >= 0)
                                deResponse.WmWhs = dataString;
                            else if (innerStr.IndexOf("WmOrg:") >= 0)
                                deResponse.WmOrg = dataString;
                            else if (innerStr.IndexOf("CrVmpy:") >= 0)
                                deResponse.CrVmpy = dataString;
                            else if (innerStr.IndexOf("CrItm:") >= 0)
                                deResponse.CrItm = dataString;
                            else if (innerStr.IndexOf("WmRefCode:") >= 0)
                                deResponse.WmRefCode = dataString;
                            else if (innerStr.IndexOf("WmSku:") >= 0)
                                deResponse.WmSku = dataString;
                            else if (innerStr.IndexOf("WmMid:") >= 0)
                                deResponse.WmMid = dataString;
                            else if (innerStr.IndexOf("WmMidName:") >= 0)
                                deResponse.WmMidName = dataString;
                            else if (innerStr.IndexOf("WmMidNameH:") >= 0)
                                deResponse.WmMidNameH = dataString;
                            else if (innerStr.IndexOf("WmSkuSpec:") >= 0)
                                deResponse.WmSkuSpec = dataString;
                            else if (innerStr.IndexOf("WmBrand:") >= 0)
                                deResponse.WmBrand = dataString;
                            else if (innerStr.IndexOf("WmMdl:") >= 0)
                                deResponse.WmMdl = dataString;
                            else if (innerStr.IndexOf("WmMidCtg:") >= 0)
                                deResponse.WmMidCtg = dataString;
                            else if (innerStr.IndexOf("WmEffcDate:") >= 0)
                                deResponse.WmEffcDate = dataString;
                            else if (innerStr.IndexOf("WmLot:") >= 0)
                                deResponse.WmLot = dataString;
                            else if (innerStr.IndexOf("WmSeno:") >= 0)
                                deResponse.WmSeno = dataString;
                            else if (innerStr.IndexOf("WmPak:") >= 0)
                                deResponse.WmPak = dataString;
                            else if (innerStr.IndexOf("WmQy:") >= 0)
                                deResponse.WmQy = dataString;
                            else if (innerStr.IndexOf("ThisBarcode:") >= 0)
                                deResponse.ThisBarcode = dataString;
                            else if (innerStr.IndexOf("UdiBarcodes:") >= 0)
                                deResponse.UdiBarcodes = dataString;
                            else if (innerStr.IndexOf("GtinString:") >= 0)
                                deResponse.GtinString = dataString;
                            else if (innerStr.IndexOf("NhiBarcode:") >= 0)
                                deResponse.NhiBarcode = dataString;
                            else if (innerStr.IndexOf("NhiBarcodes:") >= 0)
                                deResponse.NhiBarcodes = dataString;
                            else if (innerStr.IndexOf("BarcodeType:") >= 0)
                                deResponse.BarcodeType = dataString;
                            else if (innerStr.IndexOf("GtinInString:") >= 0)
                                deResponse.GtinInString = dataString;
                            else if (innerStr.IndexOf("Result:") >= 0)
                                deResponse.Result = dataString;
                            else if (innerStr.IndexOf("ErrMsg:") >= 0)
                                deResponse.ErrMsg = dataString;

                        }

                        if (deResponse.Result == "SUCCESS" && string.IsNullOrEmpty(deResponse.WmMid) == false)
                        {
                            var DBWork = session.UnitOfWork;
                            try
                            {
                                if (ackmmcode != deResponse.WmMid) {
                                    session.Result.success = false;
                                    session.Result.msg = string.Format("院內碼不符(點收院內碼：{0}，刷入院內碼：{1})，請重新確認", ackmmcode, deResponse.WmMid);
                                    return session.Result;
                                }

                                var repo = new AB0110Repository(DBWork);
                                if (repo.getUdiMmcodeCnt(deResponse.WmMid, deResponse.WmLot) > 0)
                                    session.Result.afrs = repo.UpdateUdiLog(deResponse);
                                else
                                    session.Result.afrs = repo.CreateUdiLog(deResponse);

                                session.Result.success = true;
                                session.Result.etts = repo.GetUdiInfo(barcode);
                            }
                            catch(Exception e)
                            {
                                throw;
                            }
                        }


                    }
                }
                catch (Exception e)
                {

                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckBcBarcode(FormDataCollection form)
        {
            string barcode = form.Get("barcode");
            string ackmmcode = form.Get("ackmmcode");
            string crdocno = form.Get("crdocno");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0110Repository(DBWork);
                    IEnumerable<AB0110> list = repo.GetBcBarcode(barcode);
                    if (list.Any() == false) {
                        session.Result.success = false;
                        session.Result.msg = "查無條碼資料，請重新確認";
                        return session.Result;
                    }
                    AB0110 item = list.FirstOrDefault<AB0110>();
                    if (item.ACKMMCODE != ackmmcode) {
                        session.Result.success = false;
                        session.Result.msg = string.Format("院內碼不符(點收院內碼：{0}，刷入院內碼：{1})，請重新確認", ackmmcode, item.ACKMMCODE);
                        return session.Result;
                    }

                    session.Result.etts = list;
                    session.Result.success = true;
                    return session.Result;
                    
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse InsertMulti(FormDataCollection form) {
            var itemString = form.Get("list");

            IEnumerable<AB0110> items = JsonConvert.DeserializeObject<IEnumerable<AB0110>>(itemString);

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0110Repository(DBWork);

                    foreach (AB0110 item in items) {
                        session.Result.afrs = repo.Insert(item.CRDOCNO, item.LOT_NO, item.EXP_DATE, item.INQTY, item.ISUDI, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    }

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
                
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0110Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChangeMmcode(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string ackmmcode = form.Get("ackmmcode");
            string usewhere = form.Get("usewhere");
            string usewhen = form.Get("usewhen");
            string tel = form.Get("tel");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0110Repository(DBWork);
                    session.Result.afrs = repo.UpdateAckmmcode(crdocno, ackmmcode, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    session.Result.afrs = repo.DeleteAllD(crdocno);

                    // 檢查是否為小採
                    bool isApplykind3 = repo.CheckIsAppkykind3(ackmmcode);
                    if (isApplykind3)
                    {
                        session.Result.afrs = repo.MergeCrdoc(crdocno, usewhen, usewhere, tel);
                    }
                    else {
                        // 刪除CR_DOC_D
                        session.Result.afrs = repo.DeleteDocSmall(crdocno);
                    }


                    session.Result.success = true;

                    DBWork.Commit();

                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse CheckQtyValid(FormDataCollection form) {
            string crdocno = form.Get("CRDOCNO");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0110Repository(DBWork);

                    if (repo.CheckDExists(crdocno) == false) {
                        session.Result.msg = "請輸入點收資訊";
                        session.Result.success = false;
                        return session.Result;
                    }

                    session.Result.msg = repo.CheckQty(crdocno);

                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Confirm(FormDataCollection form) {
            string crdocno = form.Get("CRDOCNO");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try {
                    var repo = new AB0110Repository(DBWork);

                    session.Result.afrs = repo.Confirm(crdocno, DBWork.UserInfo.UserId, DBWork.ProcIP);


                    DBWork.Commit();

                    return session.Result;
                } catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Reject(FormDataCollection form) {
            string crdocno = form.Get("CRDOCNO");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0110Repository(DBWork);

                    session.Result.afrs = repo.Reject(crdocno, DBWork.UserInfo.UserId, DBWork.ProcIP);


                    DBWork.Commit();

                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }

        }

        [HttpPost]
        public ApiResponse Import(FormDataCollection form) {
            string crdocno = form.Get("CRDOCNO");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0110Repository(DBWork);

                    if (repo.CheckCrDocLotNoExists(crdocno) == false) {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "廠商未回傳資料，請使用「新增」或「刷條碼新增」功能手動輸入";
                        return session.Result;
                    }

                    session.Result.afrs = repo.Import(crdocno, DBWork.UserInfo.UserId, DBWork.ProcIP);


                    DBWork.Commit();

                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }
    }
}