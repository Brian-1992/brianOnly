using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class FileController : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Upload()
        {
            string upload_message = "";
            HttpRequest req = HttpContext.Current.Request;

            //如果req.Headers["FS"]為N，則無論如何資料庫都Insert FS=N
            string ST = req.Headers["ST"];

            //檢查UploadKey是否為合法的GUID
            bool isGuid = IsGuid(req.Headers["UK"]);

            //如果沒有傳合法的UploadKey，則產生一個新的GUID當成UploadKey，最後要回傳給Client端
            string uploadKey = isGuid ? req.Headers["UK"] : Guid.NewGuid().ToString();

            /*
             * 如果沒有傳合法的UploadKey，則fileStatus=N (未儲存)，反之fileStatus=Y (已儲存)
             * 系統應定時清除fileStatus=N的紀錄
             * Client端收到本程式新產生的UploadKey之後
             * 應進行Ur_UploadRepository.Confirm(新UploadKey)
             * 才會將fileStatus更新成Y
             */
            string fileStatus = ST == "N" ? "N" :(isGuid ? "Y" : "N");

            bool isCheckExtension = (req.Headers["EC"]??"").Trim() == "" ? false : true;

            List<UR_UPLOAD> ufs = new List<UR_UPLOAD>();
            if (req.ContentLength < 524288000) //User HttpRequest不能超過 500MB
            {
                if (req.Files.Count > 0)
                {
                    string[] ec = { }; //可上傳的副檔名
                    string ec_msg = ""; //副檔名限制訊息

                    if (isCheckExtension)
                    {
                        //取得副檔名限制 Extension Constraint，必須是逗號分隔的多種副檔名
                        ec = JCLib.Util.GetEnvSetting(string.Format("UPLOAD.EC.{0}", req.Headers["EC"])).ToUpper().Split(',');

                        //串接副檔名限制訊息
                        foreach (string item in ec)
                        {
                            if (ec_msg != "") ec_msg += "、";
                            ec_msg += item;
                        }
                    }

                    bool file_len_valid = true;
                    bool file_ext_valid = true;
                    foreach (string file in req.Files)
                    {
                        var postedFile = req.Files[file];
                        var ffn = postedFile.FileName;

                        var fn = ffn.Substring(ffn.LastIndexOf("\\") + 1);

                        //檢查上傳檔案是否大於100MB
                        if (postedFile.ContentLength > 104857600) 
                        {
                            upload_message = string.Format("無法上傳檔案 {0} 大於100MB", postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\") + 1));
                            file_len_valid = false;
                            break;
                        }

                        //檢查副檔名是否合法
                        if (isCheckExtension)
                        {
                            int di = fn.LastIndexOf(".");
                            if (di == -1)
                            {
                                file_ext_valid = false;
                            }
                            else
                            {
                                var fext = fn.Substring(di + 1).ToUpper();
                                if (ec.Where((item) => { return item == fext; }).Count() == 0)
                                {
                                    file_ext_valid = false;
                                }
                            }

                            if (!file_ext_valid)
                            {
                                upload_message = string.Format("無法上傳 {0}, 須為:{1}", fn, ec_msg);
                                break;
                            }
                        }
                    }

                    if (file_len_valid && file_ext_valid)
                    {
                        foreach (string file in req.Files)
                        {
                            var postedFile = req.Files[file];

                            FileService.IFileService client = new FileService.FileServiceClient();

                            try
                            {
                                /*
                                var rtn = client.TestService(postedFile.FileName);
                                Console.WriteLine(rtn);*/
                                UR_UPLOAD uu = new UR_UPLOAD();
                                uu.FG = Guid.NewGuid().ToString();

                                //接WebSvc.UploadFile
                                FileService.IFileService clientUpload = new FileService.FileServiceClient();
                                FileService.RemoteFileInfo uploadRequestInfo = new FileService.RemoteFileInfo();
                                uploadRequestInfo.FileName = uu.FG.ToString();
                                uploadRequestInfo.Length = postedFile.ContentLength;
                                uploadRequestInfo.FileByteStream = postedFile.InputStream;
                                clientUpload.UploadFile(uploadRequestInfo);
                                
                                uu.TUSER = User.Identity.Name;
                                uu.FN = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\") + 1);
                                uu.FS = postedFile.ContentLength;
                                uu.FT = uu.FN.Substring(uu.FN.LastIndexOf(".") + 1);
                                uu.FP = "P";
                                uu.UK = uploadKey;
                                /*
                                string[] UKv = uu.UK.Split(',');
                                int ub = UKv.Length;
                                uu.UK1 = ub > 0 ? UKv[0] : "";
                                uu.UK2 = ub > 1 ? UKv[1] : "";
                                uu.UK3 = ub > 2 ? UKv[2] : "";
                                uu.UK4 = ub > 3 ? UKv[3] : "";
                                uu.UK5 = ub > 4 ? UKv[4] : "";
                                */
                                uu.ST = fileStatus;
                                uu.IP = req.UserHostAddress;
                                ufs.Add(uu);
                            }
                            catch (TimeoutException ex)
                            {
                                upload_message = string.Format("上傳錯誤:{0}", ex.Message);
                                LogUrErrM(ex.Message, ex.Message, "FileController", "Upload");
                            }
                            catch (IndexOutOfRangeException ex)
                            {
                                upload_message = string.Format("上傳錯誤:{0}", ex.Message);
                                LogUrErrM(ex.Message, ex.Message, "FileController", "Upload");
                            }                       
                        }
                    }
                }
                else
                {
                    upload_message = "無上傳檔案。";
                }
            }
            else
            {
                upload_message = "上傳的檔案太大，請分批上傳，或聯絡管理人員。";
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                if (upload_message == "")
                {
                    try
                    {
                        UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                        session.Result.etts = new string[] { uploadKey, isGuid ? "Y" : "N" };
                        session.Result.afrs = repo.Create(ufs);
                        //session.Result.etts = repo.GetFilesByUploadKey(req.Headers["UK"]);
                        session.Result.msg = string.Format("{0} 個檔案上傳成功。", session.Result.afrs);
                        //JCLib.Mail.Send("主旨", "內容", "user1@ms.aidc.com.tw;user2@ms.aidc.com.tw", "user3@ms.aidc.com.tw;user4@ms.aidc.com.tw");
                        //JCLib.Mail.Send("主旨", "內容", "user1@ms.aidc.com.tw;user2@ms.aidc.com.tw");
                    }
                    catch (Exception ex)
                    {
                        session.Result.msg = ex.Message;
                    }
                }
                else
                {
                    session.Result.success = false;
                    session.Result.msg = upload_message;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public void DownloadImage(string id)
        {
            DownloadByFG(id);
        }

        public void Download(FormDataCollection f)
        {
            if (f.Get("FG") != null)
                DownloadByFG(f.Get("FG"));
            else if (f.Get("UK") != null)
                DownloadByUK(f.Get("UK"));
        }

        public void DownloadByUK(string uploadKey)
        {
            string fileGuid = "";
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                    fileGuid = repo.GetFirstFileGuidByUK(uploadKey);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            if (fileGuid != "")
                DownloadByFG(fileGuid);
        }

        public void DownloadByFG(string fileGuid)
        {
            string fileName = "";
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                    fileName = repo.GetFileByGuid(fileGuid).FN;
                }
                catch (Exception ex)
                {
                }
            }

            if (fileName != "")
            {
                HttpResponse res = HttpContext.Current.Response;
                try
                {
                    FileService.IFileService fs_client = new FileService.FileServiceClient();
                    FileService.DownloadRequest req = new FileService.DownloadRequest(fileGuid);
                    FileService.RemoteFileInfo rfi = new FileService.RemoteFileInfo();

                    rfi = fs_client.DownloadFile(req);

                    res.BufferOutput = false;   // to prevent buffering 
                    byte[] buffer = new byte[6500];
                    int bytesRead = 0;

                    res.Clear();
                    res.ClearHeaders();
                    res.HeaderEncoding = System.Text.Encoding.Default;
                    res.ContentType = "application/octet-stream";
                    res.AddHeader("Content-Disposition",
                                "attachment; filename=\"" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8) + "\"");

                    bytesRead = rfi.FileByteStream.Read(buffer, 0, buffer.Length);

                    while (bytesRead > 0)
                    {
                        // Verify that the client is connected.
                        if (res.IsClientConnected)
                        {
                            res.OutputStream.Write(buffer, 0, bytesRead);
                            // Flush the data to the HTML output.
                            res.Flush();

                            buffer = new byte[6500];
                            bytesRead = rfi.FileByteStream.Read(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            bytesRead = -1;
                        }
                    }
                    rfi.FileByteStream.Close();
                }
                catch (Exception ex)
                {
                    // Trap the error, if any.
                    System.Web.HttpContext.Current.Response.Write("Error : " + ex.Message);
                    LogUrErrM(ex.Message, ex.Message, "FileController", "DownloadByFG");
                }
                finally
                {
                    res.Flush();
                    res.Close();
                    res.End();
                    System.Web.HttpContext.Current.Response.Close();
                }
            }
        }

        
        [HttpPost]
        public ApiResponse Delete(FormDataCollection f)
        {
            string[] fileGuids = f.Get("FG").Split(',');

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    foreach(string fileGuid in fileGuids)
                    {
                        if (fileGuid != "")
                        {
                            UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                            session.Result.afrs += repo.DeleteByFG(fileGuid);
                        }
                    }
                    session.Result.msg = string.Format("{0} 個檔案刪除成功。", session.Result.afrs);
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    LogUrErrM(ex.Message, ex.Message, "FileController", "Delete");
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse GetByKey(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                    session.Result.etts = repo.GetFilesByUploadKey(f.Get("UK"));
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    LogUrErrM(ex.Message, ex.Message, "FileController", "GetByKey");
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateFD(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                    session.Result.etts = repo.UpdateFD(f.Get("FG"), f.Get("FD"));
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    LogUrErrM(ex.Message, ex.Message, "FileController", "UpdateFD");
                }
                return session.Result;
            }
        }

        //[HttpPost]
        //public ApiResponse GetEC(FormDataCollection f)
        //{
        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            UR_ParamRepository repo = new UR_ParamRepository(DBWork);
        //            session.Result.etts = repo.ListD(GetFullExtConstraint(f.Get("EC")));
        //        }
        //        catch (Exception ex)
        //        {
        //            session.Result.msg = ex.Message;
        //            LogUrErrM(ex.Message, ex.Message, "FileController", "GetEC");
        //        }
        //        return session.Result;
        //    }
        //}

        public ApiResponse NewGuid()
        {
            ApiResponse res = new ApiResponse();
            res.etts = new string[] { Guid.NewGuid().ToString() };
            return res;
        }

        private bool IsGuid(string stringGuid)
        {
            Guid newGuid;
            return (Guid.TryParse(stringGuid, out newGuid));
        }

        //private string GetFullExtConstraint(string ec_sub)
        //{
        //    return string.Format("UPL.EX.CNST.{0}",
        //        ((ec_sub ?? "") == "") ? "DEF" : ec_sub);
        //}

        private string LogUrErrM(string msg, string st, string ctrl, string act)
        {
            string idno = "";
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repoM = new UR_ERR_MRepository(DBWork);
                    UR_ERR_M ur_err_m = new UR_ERR_M();
                    ur_err_m.MSG = msg;
                    ur_err_m.ST = st;
                    ur_err_m.CTRL = ctrl;
                    ur_err_m.ACT = act;
                    ur_err_m.TUSER = DBWork.ProcUser;
                    ur_err_m.IP = DBWork.ProcIP;
                    idno = repoM.GetNextIdno();
                    ur_err_m.IDNO = idno;
                    repoM.Create(ur_err_m);
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    // To-Do: Log(e.StackTrace);
                }
            }
            return idno;
        }
    }
}
