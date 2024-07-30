using JCLib.DB.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UridDeactivate
{
    public class LogController
    {
        string fileName = string.Empty;
        string folderName = string.Empty;
        static L l = new L("UridDeactivate");

        public LogController()
        {
        }
        public LogController(string fname)
        {
            fileName = fname;
        }
        public LogController(string fdName, string fname)
        {
            fileName = fname;
            folderName = fdName;
        }
        public LogController(string fdName, string subfdName, string fname)
        {
            fileName = fname;
            folderName = string.Format("{0}\\{1}", fdName, subfdName);
        }

        //string docPath = string.Format("{0}\\logs", System.Environment.CurrentDirectory);
        string docPath = string.Format("{0}", "C:\\mmsms-schedule-UridDeactivate-log");
        List<string> logs = new List<string>();

        public void AddLogs(string content)
        {
            logs.Add(string.Format("{0}: {1}", DateTime.Now.ToString(), content));
        }
        public void CreateLogFile()
        {
            if (docPath.Contains(folderName) == false)
            {
                docPath = string.Format("{0}\\{1}", docPath, folderName);
            }
            string fileNameFull = string.Format("{0}_{1}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"), fileName);

            string content = GetLogString(logs);

            l.writeFile(docPath, fileNameFull, content);

        }

        private string GetLogString(List<string> logs)
        {
            string temp = string.Empty;
            foreach (string log in logs)
            {
                temp = string.Format("{0}{1}{2}", temp,
                                                  temp == string.Empty ? string.Empty : Environment.NewLine,
                                                  log);
            }
            return temp;
        }

        public void DeletePastLogs(string controller, string functionName)
        {
           // string FileFolderName = string.Format("{0}_{1}", controller, functionName);
            string[] files = Directory.GetFiles(@"C:\mmsms-schedule-UridDeactivate-log");

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddMonths(-6))
                    fi.Delete();
            }
        }
    }
}
