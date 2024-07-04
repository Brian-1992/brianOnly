using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.UR;
using WebAppVen.Models;

namespace WebAppVen.Controllers
{
    public class FileController : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Upload()
        {
            string upload_message = "";
            HttpRequest req = HttpContext.Current.Request;
            List<UR_UPLOAD> ufs = new List<UR_UPLOAD>();
            if (req.ContentLength < 524288000) //User HttpRequest不能超過 500MB
            {
                if (req.Files.Count > 0)
                {
                    IEnumerable<UR_PARAM_D> ec = new UR_PARAM_D[] { };
                    string ec_msg = "";
                    using (WorkSession session = new WorkSession())
                    {
                        UnitOfWork DBWork = session.UnitOfWork;
                        try
                        {
                            UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                            ec = repo.ListD(GetFullExtConstraint(req.Headers["EC"]));
                            foreach (UR_PARAM_D item in ec)
                            {
                                if (ec_msg != "") ec_msg += "、";
                                ec_msg += item.VALUE;
                            }
                        }
                        catch (Exception ex)
                        {
                            upload_message = ex.Message;
                        }
                    }

                    bool file_len_valid = true;
                    bool file_ext_valid = true;
                    foreach (string file in req.Files)
                    {
                        var postedFile = req.Files[file];
                        var ffn = postedFile.FileName;

                        var fn = ffn.Substring(ffn.LastIndexOf("\\") + 1);

                        if (postedFile.ContentLength > 104857600) //如果上傳檔案大於100MB
                        {
                            upload_message = string.Format("無法上傳檔案 {0} 大於100MB", postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\") + 1));
                            file_len_valid = false;
                            break;
                        }

                        int di = fn.LastIndexOf(".");
                        if (di == -1)
                        {
                            file_ext_valid = false;
                        }
                        else
                        {
                            var fext = fn.Substring(di + 1).ToUpper();
                            if (ec.Where((item) => { return item.VALUE == fext; }).Count() == 0)
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
                                uu.FG = Guid.NewGuid();

                                //接ePMSWcfService.UploadFile
                                FileService.IFileService clientUpload = new FileService.FileServiceClient();
                                FileService.RemoteFileInfo uploadRequestInfo = new FileService.RemoteFileInfo();
                                uploadRequestInfo.FileName = uu.FG.ToString();
                                uploadRequestInfo.Length = postedFile.ContentLength;
                                uploadRequestInfo.FileByteStream = postedFile.InputStream;
                                clientUpload.UploadFile(uploadRequestInfo);

                                uu.FP = req.Headers["FP"];
                                uu.TUSER = User.Identity.Name;
                                uu.FN = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\") + 1);
                                uu.FS = postedFile.ContentLength;
                                uu.FT = uu.FN.Substring(uu.FN.LastIndexOf(".") + 1);
                                uu.UK = req.Headers["UK"];
                                string[] UKv = uu.UK.Split(',');
                                int ub = UKv.Length;
                                uu.UK1 = ub > 0 ? UKv[0] : "";
                                uu.UK2 = ub > 1 ? UKv[1] : "";
                                uu.UK3 = ub > 2 ? UKv[2] : "";
                                uu.UK4 = ub > 3 ? UKv[3] : "";
                                uu.UK5 = ub > 4 ? UKv[4] : "";
                                uu.ST = "NEW";
                                uu.IP = req.UserHostAddress;
                                ufs.Add(uu);
                            }
                            catch (Exception ex)
                            {
                                upload_message = string.Format("上傳錯誤:{0}", ex.Message);
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

        public void Download(FormDataCollection f)
        {
            string file_guid = f.Get("FG");
            string file_name = "";
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                    file_name = repo.GetFileByGuid(file_guid).FN;
                }
                catch (Exception ex)
                {
                }
            }

            if (file_name != "")
            {
                HttpResponse res = HttpContext.Current.Response;
                try
                {
                    FileService.IFileService fs_client = new FileService.FileServiceClient();
                    FileService.DownloadRequest req = new FileService.DownloadRequest(file_guid);
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
                                "attachment; filename=\"" + HttpUtility.UrlEncode(file_name, System.Text.Encoding.UTF8) + "\"");

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
            string file_guid = f.Get("FG");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    if (file_guid != "")
                    {
                        UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                        session.Result.afrs = repo.DeleteByFG(file_guid);
                        session.Result.msg = string.Format("{0} 個檔案刪除成功。", session.Result.afrs);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.msg = string.Format("{0} 個檔案刪除成功。", session.Result.afrs);
                    }
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
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
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateVLD(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_UploadRepository repo = new UR_UploadRepository(DBWork);
                    session.Result.afrs = repo.UpdateVLD(f.Get("FG"), Convert.ToBoolean(f.Get("VLD")));
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetEC(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                    session.Result.etts = repo.ListD(GetFullExtConstraint(f.Get("EC")));
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                }
                return session.Result;
            }
        }

        public string GetFullExtConstraint(string ec_sub)
        {
            return string.Format("UPL.EX.CNST.{0}",
                ((ec_sub ?? "") == "") ? "DEF" : ec_sub);
        }
    }
}
