using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers.UR
{
    public class UR1016Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_DocRepository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(UR_DOC ur_doc)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_DocRepository(DBWork);
                    if (!repo.CheckExists(ur_doc.DK))
                    {
                        //儲存系統文件資訊
                        ur_doc.CREATE_USER = User.Identity.Name;
                        ur_doc.UPDATE_USER = User.Identity.Name;
                        ur_doc.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(ur_doc);
                        session.Result.etts = repo.Get(ur_doc.DK);

                        //確認系統文件上傳都已儲存
                        var repo2 = new UR_UploadRepository(DBWork);
                        repo2.Confirm(ur_doc.UK);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>文件編碼</span> 重複，請重新輸入。";
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
        public ApiResponse Update(UR_DOC ur_doc)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_DocRepository(DBWork);
                    if (repo.CheckExists(ur_doc.DK))
                    {
                        //儲存系統文件資訊
                        ur_doc.UPDATE_USER = User.Identity.Name;
                        ur_doc.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(ur_doc);
                        session.Result.etts = repo.Get(ur_doc.DK);

                        //確認系統文件上傳都已儲存
                        var repo2 = new UR_UploadRepository(DBWork);
                        repo2.Confirm(ur_doc.UK);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>文件編碼</span> 不存在。";
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
        public ApiResponse Delete(UR_DOC ur_doc)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_DocRepository(DBWork);
                    if (repo.CheckExists(ur_doc.DK))
                    {
                        //刪除系統文件資訊
                        ur_doc.UPDATE_USER = User.Identity.Name;
                        ur_doc.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Delete(ur_doc);
                        session.Result.etts = repo.Get(ur_doc.DK);

                        //刪除系統文件上傳檔案
                        var repo2 = new UR_UploadRepository(DBWork);
                        repo2.DeleteByUK(ur_doc.UK);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>文件編碼</span> 不存在。";
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
        public string GetUploadKey(UR_DOC ur_doc)
        {
            string uploadKey;
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    //取得UploadKey
                    object objUploadKey;
                    var repo = new UR_DocRepository(DBWork);
                    objUploadKey = repo.GetUploadKey(ur_doc.DK);

                    //若此UploadKey下，沒有上傳檔案，則返回空字串
                    //不讓使用者下載檔案
                    uploadKey = (objUploadKey ?? "").ToString();
                    var repo2 = new UR_UploadRepository(DBWork);
                    int fileCount = repo2.GetFileCountByUK(uploadKey);
                    if (fileCount == 0) uploadKey = "";
                }
                catch
                {
                    throw;
                }
                return uploadKey;
            }
        }

    }
}