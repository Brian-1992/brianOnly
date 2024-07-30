using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AB;
using WebApp.Repository.C;

namespace WebApp.Controllers.AB
{
    public class AB0106Controller : SiteBase.BaseApiController
    {
        /* 2022-06-14修改檢查規則
            1.【給付類別】為0不給付,1給付,2條件給付→系統不可點選扣庫 ,顯示訊息 : 屬於[扣庫品，請於其他系統紀錄消耗]   
            2. 【來源代碼】：C→系統不可點選扣庫 ,顯示訊息 :屬於[寄售品項 ，請於其他系統紀錄消耗]
            3. 【停用】: Y →系統不可點選扣庫 ,顯示訊息 :屬於[停用品項]
            4. 非1項條件→扣庫品項 , 系統可點選扣庫
            5.不用再判斷盤差種類了
         */

        [HttpPost]
        public ApiResponse GetBarcodeRecord(FormDataCollection form) {
            string wh_no = form.Get("wh_no") == null ? string.Empty : form.Get("wh_no").Trim();
            string barcode = form.Get("barcode") == null ? string.Empty : form.Get("barcode").Trim();
            string mmcode = form.Get("mmcode") == null ? string.Empty : form.Get("mmcode").Trim() ;
            string use_type = form.Get("use_type");

            // 給付類別清單
            IEnumerable<string> paykinds = new List<string>() { "0", "1", "2" };

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0106Repository(DBWork);

                    // 檢查庫房是否有權限
                    if (repo.CheckWhnoValid(wh_no, DBWork.UserInfo.UserId) == false) {
                        session.Result.success = false;
                        session.Result.msg = "無此庫房權限，請重新確認";
                        return session.Result;
                    }

                    string search_barcode = string.Empty;

                    // barcode = null，以院內碼查詢；barcode有資料，以barcode為主
                    if (string.IsNullOrEmpty(mmcode) == false && string.IsNullOrEmpty(barcode) == true)
                    {
                        search_barcode = mmcode;
                    }
                    else {
                        search_barcode = barcode;
                    }

                    AB0106Item item = new AB0106Item();
                    List<AB0106Item> returnList = new List<AB0106Item>();
                    // 查詢BC_UDI_LOG
                    BC_UDI_LOG udi_log = repo.GetUdiLog(search_barcode);
                    // 有找到，以院內碼查詢
                    if (udi_log != null) {
                        item = repo.GetDataByMmcode(udi_log.WMMID, wh_no);
                        #region 院內碼檢核
                        // 檢查院內碼是否存在
                        if (item == null) {
                            session.Result.success = false;
                            session.Result.msg = "查無院內碼";
                            return session.Result;
                        }
                        // 檢查是否為衛材
                        if (item.MAT_CLASS != "02") {
                            session.Result.success = false;
                            session.Result.msg = "必須為衛材";
                            return session.Result;
                        }
                        // 檢查是否全院停用
                        if (item.CANCEL_ID == "Y")
                        {
                            session.Result.success = false;
                            session.Result.msg = "院內碼已全院停用";
                            return session.Result;
                        }
                        // 檢查來源類別是否為C寄售
                        if (item.E_SOURCECODE == "C")
                        {
                            session.Result.success = false;
                            session.Result.msg = string.Format("屬於[寄售品項] ，請於其他系統記錄{0}", use_type == "A" ? "消耗" : "繳回");
                        }
                        // 檢查給付類別是否為0不給付,1給付,2條件給付
                        
                        if (paykinds.Contains(item.M_PAYKIND) && string.IsNullOrEmpty(session.Result.msg))
                        {
                            session.Result.success = false;
                            session.Result.msg = string.Format("屬於[扣庫品] ，請於其他系統記錄{0}", use_type == "A" ? "消耗" : "繳回");
                        }

                        if (session.Result.msg != string.Empty) {
                            string seq = repo.GetSUseSeq();

                            item.LOT_NO = udi_log.WMLOT;
                            item.EXP_DATE_UDI = udi_log.WMEFFCDATE;
                            if (string.IsNullOrEmpty(item.EXP_DATE_UDI) == false)
                            {
                                DateTime tempDateTime = DateTime.ParseExact(item.EXP_DATE_UDI, "yyyyMMdd", CultureInfo.InvariantCulture);
                                item.EXP_DATE = tempDateTime.ToString("yyyy-MM-dd");
                            }
                            item.TRATIO = "1";
                            item.BARCODE = search_barcode;
                            item.IS_UDI = "Y";

                            session.Result.afrs = repo.InsertScanUseLogZ(seq, wh_no, item.MMCODE, use_type,
                                                                         item.TRATIO, item.BASE_UNIT, item.INV_QTY, item.WEXP_ID,
                                                                         item.LOT_NO, item.EXP_DATE, item.EXP_DATE_UDI,
                                                                         item.M_TRNID, item.E_SOURCECODE, DBWork.UserInfo.UserId, DBWork.ProcIP,
                                                                         item.BARCODE, item.SUSE_NOTE, item.M_PAYKIND);
                            DBWork.Commit();
                            return session.Result;
                        }
                        #endregion

                        item.LOT_NO = udi_log.WMLOT;
                        item.EXP_DATE_UDI = udi_log.WMEFFCDATE;
                        if (string.IsNullOrEmpty(item.EXP_DATE_UDI) == false) {
                            DateTime tempDateTime =  DateTime.ParseExact(item.EXP_DATE_UDI, "yyyyMMdd", CultureInfo.InvariantCulture);
                            item.EXP_DATE = tempDateTime.ToString("yyyy-MM-dd");
                        }
                        item.TRATIO = "1";
                        item.BARCODE = search_barcode;
                        item.IS_UDI = "Y";

                        returnList.Add(item);
                        session.Result.etts = returnList;
                        return session.Result
;
                    }

                    // 沒找到udi_log，呼叫udi api
                    int insertUdiLog = GetUdiData(DBWork, search_barcode);
                    // 有insert，再查詢一次udi_log
                    if (insertUdiLog > 0) {
                        // 查詢BC_UDI_LOG
                        udi_log = repo.GetUdiLog(search_barcode);
                        // 有找到，以院內碼查詢
                        if (udi_log != null)
                        {
                            item = repo.GetDataByMmcode(udi_log.WMMID, wh_no);
                            #region 院內碼檢核
                            // 檢查院內碼是否存在
                            if (item == null)
                            {
                                session.Result.success = false;
                                session.Result.msg = "查無院內碼";
                                return session.Result;
                            }
                            // 檢查是否為衛材
                            if (item.MAT_CLASS != "02")
                            {
                                session.Result.success = false;
                                session.Result.msg = "必須為衛材";
                                return session.Result;
                            }
                            // 檢查是否全院停用
                            if (item.CANCEL_ID == "Y")
                            {
                                session.Result.success = false;
                                session.Result.msg = "院內碼已全院停用";
                                return session.Result;
                            }
                            // 檢查來源類別是否為C寄售
                            if (item.E_SOURCECODE == "C")
                            {
                                session.Result.success = false;
                                session.Result.msg = string.Format("屬於[寄售品項] ，請於其他系統記錄{0}", use_type == "A" ? "消耗" : "繳回");
                            }
                            // 檢查給付類別是否為0不給付,1給付,2條件給付

                            if (paykinds.Contains(item.M_PAYKIND) && string.IsNullOrEmpty(session.Result.msg))
                            {
                                session.Result.success = false;
                                session.Result.msg = string.Format("屬於[扣庫品] ，請於其他系統記錄{0}", use_type == "A" ? "消耗" : "繳回");
                            }

                            if (session.Result.msg != string.Empty)
                            {
                                string seq = repo.GetSUseSeq();

                                item.LOT_NO = udi_log.WMLOT;
                                item.EXP_DATE_UDI = udi_log.WMEFFCDATE;
                                if (string.IsNullOrEmpty(item.EXP_DATE_UDI) == false)
                                {
                                    DateTime tempDateTime = DateTime.ParseExact(item.EXP_DATE_UDI, "yyyyMMdd", CultureInfo.InvariantCulture);
                                    item.EXP_DATE = tempDateTime.ToString("yyyy-MM-dd");
                                }
                                item.TRATIO = "1";
                                item.BARCODE = search_barcode;
                                item.IS_UDI = "Y";

                                session.Result.afrs = repo.InsertScanUseLogZ(seq, wh_no, item.MMCODE, use_type,
                                                                             item.TRATIO, item.BASE_UNIT, item.INV_QTY, item.WEXP_ID,
                                                                             item.LOT_NO, item.EXP_DATE, item.EXP_DATE_UDI,
                                                                             item.M_TRNID, item.E_SOURCECODE, DBWork.UserInfo.UserId, DBWork.ProcIP,
                                                                             item.BARCODE, item.SUSE_NOTE, item.M_PAYKIND);
                                DBWork.Commit();
                                return session.Result;
                            }
                            #endregion

                            item.LOT_NO = udi_log.WMLOT;
                            item.EXP_DATE_UDI = udi_log.WMEFFCDATE;
                            if (string.IsNullOrEmpty(item.EXP_DATE_UDI) == false)
                            {
                                DateTime tempDateTime = DateTime.ParseExact(item.EXP_DATE_UDI, "yyyyMMdd", CultureInfo.InvariantCulture);
                                item.EXP_DATE = tempDateTime.ToString("yyyy-MM-dd");
                            }
                            item.TRATIO = "1";
                            item.BARCODE = search_barcode;
                            item.IS_UDI = "Y";

                            DBWork.Commit();

                            returnList.Add(item);
                            session.Result.etts = returnList;
                            return session.Result
    ;
                        }
                    }

                    // 沒找到udi_log，改查bc_barcode
                    BC_BARCODE bc_barcode = repo.GetMmcodeFromBarcode(search_barcode);
                    if (bc_barcode == null) {
                        session.Result.success = false;
                        session.Result.msg = "查無院內碼";
                        return session.Result;
                    }

                    // 有找到，以院內碼查詢
                    item = repo.GetDataByMmcode(bc_barcode.MMCODE, wh_no);
                    #region 院內碼檢核
                    // 檢查院內碼是否存在
                    if (item == null)
                    {
                        session.Result.success = false;
                        session.Result.msg = "查無院內碼";
                        return session.Result;
                    }
                    // 檢查是否為衛材
                    if (item.MAT_CLASS != "02")
                    {
                        session.Result.success = false;
                        session.Result.msg = "必須為衛材";
                        return session.Result;
                    }
                    // 檢查是否全院停用
                    if (item.CANCEL_ID == "Y")
                    {
                        session.Result.success = false;
                        session.Result.msg = "院內碼已全院停用";
                        return session.Result;
                    }
                    // 檢查來源類別是否為C寄售
                    if (item.E_SOURCECODE == "C")
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("屬於[寄售品項] ，請於其他系統記錄{0}", use_type == "A" ? "消耗" : "繳回");
                    }
                    // 檢查給付類別是否為0不給付,1給付,2條件給付

                    if (paykinds.Contains(item.M_PAYKIND) && string.IsNullOrEmpty(session.Result.msg))
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("屬於[扣庫品] ，請於其他系統記錄{0}", use_type == "A" ? "消耗" : "繳回");
                    }

                    if (session.Result.msg != string.Empty)
                    {
                        string seq = repo.GetSUseSeq();

                        item.TRATIO = bc_barcode.TRATIO;
                        item.BARCODE = search_barcode;

                        session.Result.afrs = repo.InsertScanUseLogZ(seq, wh_no, item.MMCODE, use_type,
                                                                     item.TRATIO, item.BASE_UNIT,  item.INV_QTY, item.WEXP_ID,
                                                                     item.LOT_NO, item.EXP_DATE, item.EXP_DATE_UDI,
                                                                     item.M_TRNID, item.E_SOURCECODE, DBWork.UserInfo.UserId, DBWork.ProcIP,
                                                                     item.BARCODE, item.SUSE_NOTE, item.M_PAYKIND);
                        DBWork.Commit();
                        return session.Result;
                    }

                    #endregion

                    item.TRATIO = bc_barcode.TRATIO;
                    item.BARCODE = search_barcode;

                    returnList.Add(item);
                    session.Result.etts = returnList;
                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    session.Result.success = false;
                    throw;
                }
            }
        }

        public int GetUdiData(UnitOfWork DBWork, string barcode) {
            int afrs = -1;
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
                        try
                        {
                            var repo = new CC0002Repository(DBWork);
                            
                            if (repo.getUdiMmcodeCnt(deResponse.WmMid, deResponse.WmLot) > 0)
                                afrs = repo.UpdateUdiLog(deResponse);
                            else
                                afrs = repo.CreateUdiLog(deResponse);

                            return afrs;
                        }
                        catch (TimeoutException ex)
                        {
                            return afrs;
                        }
                    }


                }
            }
            catch(IndexOutOfRangeException e)
            {
                return afrs;
            }
            catch (TimeoutException e)
            {
                return afrs;
            }
            catch (FormatException e)
            {
                return afrs;
            }

            return afrs;
        }

        [HttpPost]
        public ApiResponse SetData(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            string mmcode = form.Get("mmcode");
            string use_type = form.Get("use_type");
            string tratio = form.Get("tratio");
            string acktimes = form.Get("acktimes");
            string adjqty = form.Get("adjqty");
            string base_unit = form.Get("base_unit");
            string bf_invqty = form.Get("bf_invqty");
            string wexp_id = form.Get("wexp_id");
            string lot_no = form.Get("lot_no");
            string exp_date = form.Get("exp_date");
            string m_trnid = form.Get("m_trnid");
            string e_sourcecode = form.Get("e_sourcecode");
            string scan_barcode = form.Get("scan_barcode");
            string suse_note = form.Get("suse_note");
            string exp_date_udi = form.Get("exp_date_udi");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0106Repository(DBWork);

                    int tratio_i = int.Parse(tratio);
                    int acktimes_i = int.Parse(acktimes);
                    int adjqty_i = int.Parse(adjqty);
                    int use_qty = tratio_i * acktimes_i + adjqty_i;
                    if (use_qty == 0) {
                        session.Result.success = false;
                        session.Result.msg = string.Format("{0} 不可為0", use_type == "A" ? "扣庫量" : "繳回量");
                        return session.Result;
                    }

                    string seq = repo.GetSUseSeq();
                    // 新增SCAN_USE_LOG
                    session.Result.afrs = repo.InsertScanUseLog(seq, wh_no, mmcode, use_type,
                        tratio, acktimes, adjqty, use_qty.ToString(),
                        base_unit, bf_invqty, wexp_id, lot_no, exp_date, exp_date_udi,
                        m_trnid, e_sourcecode, DBWork.UserInfo.UserId, DBWork.ProcIP,
                        scan_barcode, suse_note);

                    // 檢查是否存在MI_WHINV
                    if (repo.CheckWhinvExists(wh_no, mmcode) == false)
                    {
                        // 不存在，先新增
                        session.Result.afrs = repo.InsertWhinv(wh_no, mmcode);
                    }
                    int use_qty_whinv = use_qty;
                    // 若為繳回，* -1
                    if (use_type == "B")
                    {
                        use_qty_whinv = use_qty_whinv * (-1);
                    }
                    session.Result.afrs = repo.UpdateWhinv(wh_no, mmcode, use_qty_whinv.ToString());

                    // 新增WHTRNS
                    string tr_docno = repo.GetWhtrnsDocno(seq);
                    session.Result.afrs = repo.InsertWhtrns(wh_no, mmcode, use_qty.ToString(), tr_docno, bf_invqty, use_type);

                    // 批號效期管制
                    if (wexp_id == "Y")
                    {
                        // 檢查是否存在MI_WEXPINV
                        if (repo.CheckWexpinvExists(wh_no, mmcode, lot_no, exp_date) == false)
                        {
                            // 不存在，新增
                            session.Result.afrs = repo.InsertWexpinv(wh_no, mmcode, exp_date, lot_no, use_qty_whinv.ToString(), use_type,
                                                                    DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                        else
                        {
                            // 存在，更新
                            session.Result.afrs = repo.UpdateWexpinv(wh_no, mmcode, exp_date, lot_no, use_qty_whinv.ToString(), use_type,
                                                                    DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                    }

                    // 更新SCAN_USE_LOG.ISUSE = 'Y'
                    session.Result.afrs = repo.UpdateScanUseLogY(seq);

                    DBWork.Commit();
                    session.Result.success = true;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    session.Result.success = false;
                    throw;
                }

                return session.Result;
            }
        }

        #region combo
        [HttpGet]
        public ApiResponse GetWhnoCombo() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;

                try {
                    var repo = new AB0106Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(DBWork.UserInfo.UserId);
                }
                catch(Exception e){
                    throw;
                }
                return session.Result;
            }
        }

        #endregion
    }
}