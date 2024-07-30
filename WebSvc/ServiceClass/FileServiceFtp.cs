using System;
using System.IO;
using System.Net;
using System.Configuration;

namespace WebSvc
{
    // 注意: 您可以使用 [重構] 功能表上的 [重新命名] 命令同時變更程式碼、svc 和組態檔中的類別名稱 "Service1"。
    // 注意: 若要啟動 WCF 測試用戶端以便測試此服務，請在 [方案總管] 中選取 Service1.svc 或 Service1.svc.cs，然後開始偵錯。
    public class FileServiceFtp : IFileService
    {
        #region 檔案存在遠端FTP Server
        //private string ftp_path = @"ftp://192.168.99.37:8721/";
        private string ftp_path = @ConfigurationManager.AppSettings["Path"];
        public RemoteFileInfo DownloadFile(DownloadRequest request)
        {
            RemoteFileInfo result = new RemoteFileInfo();
            try
            {
                string filePath = System.IO.Path.Combine(ftp_path, request.FileName);
                FtpWebRequest ftpReq = (FtpWebRequest) WebRequest.Create(filePath);
                ftpReq.Credentials = new NetworkCredential("administrator", "Ifcs42001");
                ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;

                // check if exists

                /*
                using (Stream ftpStream = ftpReq.GetResponse().GetResponseStream())
                {
                    MemoryStream ms = new MemoryStream();
                    ftpStream.CopyTo(ms);
                    ftpStream.Flush();
                    result.FileName = request.FileName;
                    //result.Length = ftpStream.Length;
                    result.Length = ms.Length;
                    result.FileByteStream = ms;
                }*/
                Stream ftpStream = ftpReq.GetResponse().GetResponseStream();
                result.FileName = request.FileName;
                result.Length = 0;// ftpStream.Length;
                result.FileByteStream = ftpStream;
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public void UploadFile(RemoteFileInfo request)
        {
            string filePath = System.IO.Path.Combine(ftp_path, request.FileName);
            FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(filePath);
            ftpReq.Credentials = new NetworkCredential("administrator", "Ifcs42001");
            ftpReq.Method = WebRequestMethods.Ftp.UploadFile;

            Stream targetStream = null;
            Stream sourceStream = request.FileByteStream;

            using (targetStream = ftpReq.GetRequestStream())
            {
                sourceStream.CopyTo(targetStream);
                targetStream.Close();
                sourceStream.Close();
            }
        }

        public int DeleteFile(string file_guid)
        {
            int result = 0;
            try
            {
                string filePath = System.IO.Path.Combine(ftp_path, file_guid);
                FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(filePath);
                ftpReq.Credentials = new NetworkCredential("administrator", "Ifcs42001");
                ftpReq.Method = WebRequestMethods.Ftp.DeleteFile;
                WebResponse res = ftpReq.GetResponse();
                if (((System.Net.FtpWebResponse)res).StatusCode == FtpStatusCode.FileActionOK)
                {
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
