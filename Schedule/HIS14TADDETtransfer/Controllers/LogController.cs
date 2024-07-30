using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIS14TADDETtransfer
{
    public class LogController
    {

        public void Exception_To_Log(string Exception_Str, string controller, string functionName)
        {
            string FileFolderName = string.Format("{0}_{1}", controller, functionName);
            string NOW_DATETIME = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (Directory.Exists(@"C:\mmsms-schedule-log\HIS14TADDETtransfer\" + FileFolderName + @"\") == true)
            {
            }
            else
                Directory.CreateDirectory(@"C:\mmsms-schedule-log\HIS14TADDETtransfer\" + FileFolderName);


            using (StreamWriter sw = new StreamWriter(@"C:\mmsms-schedule-log\HIS14TADDETtransfer\" + FileFolderName + @"\ServerError_" + NOW_DATETIME + ".txt"))
            {
                sw.WriteLine(DateTime.Now);  // Arbitrary objects can also be written to the file.

                sw.WriteLine("[Exception]" + Exception_Str);

                sw.Close();
            }
        }
    }
}
