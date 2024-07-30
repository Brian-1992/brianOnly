using System;
using System.IO;
using System.Configuration;

namespace WebSvc
{
    // 注意: 您可以使用 [重構] 功能表上的 [重新命名] 命令同時變更程式碼、svc 和組態檔中的類別名稱 "Service1"。
    // 注意: 若要啟動 WCF 測試用戶端以便測試此服務，請在 [方案總管] 中選取 Service1.svc 或 Service1.svc.cs，然後開始偵錯。
    public class FileServiceNet : IFileService
    {
        #region 檔案存在區網內其他電腦(File Server)
        //private string net_path = @"\\192.168.99.37\d$";
        private string net_path = @ConfigurationManager.AppSettings["Path"];
        public void UploadFile(RemoteFileInfo request)
        {
            FileStream targetStream = null;
            Stream sourceStream = request.FileByteStream;

            string filePath = Path.Combine(net_path, request.FileName);

            using (targetStream = new FileStream(filePath, FileMode.Create,
                                  FileAccess.Write, FileShare.None))
            {
                sourceStream.CopyTo(targetStream, 65000);
                targetStream.Close();
                sourceStream.Close();
            }
        }
        public RemoteFileInfo DownloadFile(DownloadRequest request)
        {
            RemoteFileInfo result = new RemoteFileInfo();
            try
            {
                string filePath = System.IO.Path.Combine(net_path, request.FileName);
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);

                // check if exists
                if (!fileInfo.Exists)
                    throw new System.IO.FileNotFoundException("File not found",
                                                              request.FileName);

                // open stream
                System.IO.FileStream stream = new System.IO.FileStream(filePath,
                          System.IO.FileMode.Open, System.IO.FileAccess.Read);

                // return result 
                result.FileName = request.FileName;
                result.Length = fileInfo.Length;
                result.FileByteStream = stream;
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public int DeleteFile(string file_guid)
        {
            int result = 0;
            try
            {
                string filePath = System.IO.Path.Combine(net_path, file_guid);
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);

                // check if exists
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                    result = 1;
                }
            }
            catch (Exception ex)
            {
                result = 0;
            }

            return result;
        }
        #endregion

        public string TestService(string input)
        {
            return input + " received.";
        }
    }
}
