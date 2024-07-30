using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using JCLib.DB.Tool;

namespace ConsoleApp1
{
    public class LogController
    {
        string fileName = string.Empty;
        string folderName = string.Empty;
        static L l = new L("TransferLog");

        public LogController()
        {
        }
        public LogController(string fname) {
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
        string docPath = string.Format("{0}", "C:\\mmsms-schedule-Transfer-log");
        List<string> logs = new List<string>();

        public void AddLogs(string content)
        {
            logs.Add(string.Format("{0}: {1}", DateTime.Now.ToString(), content));
        }
        public void CreateLogFile(string text)
        {
            if (docPath.Contains(folderName) == false)
            {
                docPath = string.Format("{0}\\{1}", docPath, folderName);
            }
            string fileNameFull = string.Format("{0}_{1}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"), fileName);
            if (string.IsNullOrEmpty(text) == false) {
                fileNameFull = string.Format("{0}_{1}_{2}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"), fileName, text);
            }

            string content = GetLogString(logs);

            l.writeFile(docPath, fileNameFull, content);
            
        }

        private string GetLogString(List<string> logs) {
            string temp = string.Empty;
            foreach (string log in logs) {
                temp = string.Format("{0}{1}{2}", temp, 
                                                  temp == string.Empty ? string.Empty : Environment.NewLine,
                                                  log);
            }
            return temp;
        }
        public void ClearLogs() {
            logs = new List<string>();
        }
    }
}
