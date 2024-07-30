using System.IO;
using System.Web;

namespace JCLib
{
    public class Export
    {
        public static void OutputFile(MemoryStream memoryStream, string fileName)
        {
            HttpResponse res = HttpContext.Current.Response;

            res.BufferOutput = false;
            res.Clear();
            res.ClearHeaders();
            res.HeaderEncoding = System.Text.Encoding.Default;
            res.ContentType = "application/octet-stream";
            res.AddHeader("Content-Disposition",
                        "attachment; filename=\"" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8) + "\"");
            res.BinaryWrite(memoryStream.ToArray());
            res.Flush();
            res.End();
        }
    }
}
