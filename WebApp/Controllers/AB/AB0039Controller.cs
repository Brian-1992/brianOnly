using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0039Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0039Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        //匯出
        [HttpPost]
        public void Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0039Repository repo = new AB0039Repository(DBWork);
                    var dt1 = repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);
                    var title = "院內代碼,健保碼,英文名稱,成份名稱,院內名稱,中文名稱,簡稱_hp專用,藥委會品項,藥品性質欄位_僅做藥品之分類_線上並無作用,醫令使用狀態,給付條文代碼,衛生署核准字號,藥品單複方,藥品成份1,成份量及單位,藥品成份2,成份量及單位2,藥品成份3,成份量及單位3,藥品成份4,成份量及單位4,醫囑單位,中文單位,廠商代碼_供應商代碼,合約碼,標案來源,健保價,自費價,買斷藥,住院消耗歸整,門急消耗歸整,累計用藥,製造廠名稱,申請商名稱,規格量及單位,申報_計價單位,扣庫單位,UD服務,門診倍數核發,住院倍數核發,預設給藥途徑,公藥分類_三總藥學專用欄位,藥品類別,研究用藥,開立限制,內審限制用量,限制狀態,限制次數,門診限制開立數量,門診限制開立日數,門診效期日數,住院限制開立數量,住院限制開立日數,住院效期日數,限制途徑,服用藥別,抗生素等級,管制用藥,住院給藥頻率,門診給藥頻率,預設劑量,預設劑量,藥品劑型,罕見疾病用藥,外審_健保專案用藥,內審用藥,一次極量,一日極量,不可剝半,不可磨粉,冷藏存放,避光存放,體重及安全量_計算別,體重及安全量_限制數量,高警訊藥品,高警訊藥品提示,藥品外觀,藥品外觀_英文,適應症_中文,適應症_英文,主要副作用_中文,主要副作用_英文,注意事項_中文,注意事項_英文,衛生署核准適應症,FDA核准適應症,處方集,授乳安全性,懷孕分級,藥品圖片檔名,藥品仿單檔名,TDM藥品,TDM合理治療濃度上限,TDM合理治療濃度下限,TDM合理PEAK起,TDM合理PEAK迄,TDM合理Trough起,TDM合理Trough迄,TDM危急值起,TDM危急值迄,TDM備註1,TDM備註2,TDM備註3,UD磨粉,藥包機品項,成份母層代碼1,成份母層代碼2,成份母層代碼3,成份母層代碼4,藥品包裝,藥理分類ATC1,藥理分類AHFS1,藥理分類ATC2,藥理分類AHFS2,藥理分類ATC3,藥理分類AHFS3,藥理分類ATC4,藥理分類AHFS4,老年人劑量調整,肝功能不良需調整劑量,腎功能不良需調整劑量,生物製劑,血液製劑,是否需冷凍,CDC藥品";

                    JCLib.Csv.Export("大批異動修改作業.csv", dt1, title);
                }
                catch
                {
                    throw;
                }
            }
        }

        //院內碼下拉式選單
        [HttpPost]
        public ApiResponse GetOrdercodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0039Repository(DBWork);
                    session.Result.etts = repo.GetOrdercodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //廠商代碼下拉式選單
        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0039Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //健保碼下拉式選單
        [HttpPost]
        public ApiResponse GetInsuOrderCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0039Repository(DBWork);
                    session.Result.etts = repo.GetInsuOrderCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //來源代碼下拉式選單
        [HttpPost]
        public ApiResponse GetCaseFromCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0039Repository(DBWork);
                    session.Result.etts = repo.GetCaseFromCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetSourceCodeCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0039Repository(DBWork);
                    session.Result.etts = repo.GetSourceCodeCombo();
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