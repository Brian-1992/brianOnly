using System.Configuration;

namespace WebSvc
{
    // 注意: 您可以使用 [重構] 功能表上的 [重新命名] 命令同時變更程式碼、svc 和組態檔中的類別名稱 "Service1"。
    // 注意: 若要啟動 WCF 測試用戶端以便測試此服務，請在 [方案總管] 中選取 Service1.svc 或 Service1.svc.cs，然後開始偵錯。
    public class FileService : IFileService
    {
        public RemoteFileInfo DownloadFile(DownloadRequest request)
        {
            IFileService fs = CreateFileService();
            return fs.DownloadFile(request);
        }
        public void UploadFile(RemoteFileInfo request)
        {
            IFileService fs = CreateFileService();
            fs.UploadFile(request);
        }

        public int DeleteFile(string file_guid)
        {
            IFileService fs = CreateFileService();
            return fs.DeleteFile(file_guid);
        }

        public string TestService(string input)
        {
            return input + " received.";
        }

        private IFileService CreateFileService()
        {
            switch (ConfigurationManager.AppSettings["ServiceType"])
            {
                case "local":
                    return new FileServiceLocal();
                case "ftp":
                    return new FileServiceFtp();
                case "net":
                    return new FileServiceNet();
                default:
                    return new FileServiceLocal();
            }
        }
    }
}
