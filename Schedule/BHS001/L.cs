using System;
using System.Collections.Generic;

using System.Text;
using System.IO;


namespace BHS001
{
    public class L // : System.Web.Services.WebService
    {
        private string filename = null;
        private string sCls = "";

        //public L(
        //    string filename,
        //    string sCls
        //)
        //{
        //    this.filename = filename;
        //    this.sCls = sCls;
        //} // 

        public L(
            string sCls
        )
        {
            this.sCls = sCls;
        } // 




        // -- 寫檔 -- 
        private static readonly object LockFile = new object();
        public void lg(
            string sFun,
            string theText
        )
        {
            try
            {
                lock (LockFile)
                {
                    filename = "log.txt";
                    using (var fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var log = new StreamWriter(fs, System.Text.Encoding.GetEncoding("big5")))
                        {
                            String s = "";
                            s += DateTime.Now.ToString("yyyyMMddHHmmss.ffffff") + "\t";
                            s += this.sCls + "\t\t";
                            s += sFun + "\t\t";
                            s += theText;
                            s += Environment.NewLine;
                            // Console.Write(s);
                            log.Write(s);
                        } // 
                    }  // 
                }
            }
            catch (System.Exception ex)
            {
                // 寫檔產生錯誤
            }
        } // 
        public void le(
            string sFun,
            string theText
        )
        {
            try
            {
                lock (LockFile)
                {
                    filename = "errlog.txt";
                    using (var fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var log = new StreamWriter(fs, System.Text.Encoding.GetEncoding("big5")))
                        {
                            String s = "";
                            s += DateTime.Now.ToString("yyyyMMddHHmmss.ffffff") + "\t";
                            s += this.sCls + "\t\t";
                            s += sFun + "\t\t";
                            s += theText;
                            s += Environment.NewLine;
                            Console.Write(s);
                            log.Write(s);
                        } // 
                    }  // 
                }
            }
            catch (System.Exception ex)
            {
                // 寫檔產生錯誤
            }
        } // 

        // 寫檔(不加料)
        public void writeToStringFile(
            string filepath,
            string s
        )
        {
            try
            {

                lock (LockFile)
                {
                    using (var fs = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var log = new StreamWriter(fs)) // , Encoding.UTF8
                        {
                            log.Write(s);
                        } // 
                    }  // 
                }
            }
            catch (System.Exception ex)
            {
                // 寫檔產生錯誤
            }
        } // 
        public void writeToStringFile(
            string filepath,
            string s,
            Encoding encoding
        )
        {
            try
            {

                lock (LockFile)
                {
                    StreamWriter sw = new StreamWriter(filepath, false, encoding);
                    sw.Write(s);
                    sw.Close();
                }
            }
            catch (System.Exception ex)
            {
                // 寫檔產生錯誤
            }
        } // 
        


    } // end of class
} // end of namespace
