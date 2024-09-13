using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Utilities.models;

namespace WebApi.Utilities
{
    public class LogTool : JCLib.Mvc.BaseRepository
    {
        private static LogTool _instance;
        private static readonly object _lock = new object();
        private readonly IUnitOfWork _unitOfWork;
        private LogTool(IUnitOfWork unitOfWork) : base(unitOfWork) {
            _unitOfWork = unitOfWork;
        }
        public static LogTool getInstance(UnitOfWork UnitOfWork)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LogTool(UnitOfWork);
                    }
                }
            }
            return _instance;
        }

        public void WriteLog(logModel logContent)
        {
            try
            {
                string sql = @"
                INSERT INTO ADC_LOG (ID, ADCNO, REQUESTDATETIME, RESPONSECONTENT, REQUESTDATA,RESPONSEDATETIME,RESPONSESTATUS,FUNCTIONNAME) 
                VALUES (:ID, :ADCNO, :REQUESTDATETIME, :RESPONSECONTENT, :REQUESTDATA,:RESPONSEDATETIME,:RESPONSESTATUS,:FUNCTIONNAME)";

                if (string.IsNullOrEmpty(logContent.ID))
                {
                    logContent.ID = getHexString();
                }
                _unitOfWork.Connection.Execute(sql, logContent);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing log to database: {ex.Message}");
            }
        }

        public string getHexString() {
            Guid guid = Guid.NewGuid();
            string hexString = BitConverter.ToString(guid.ToByteArray()).Replace("-", "");
            return hexString;
        }

    }
}