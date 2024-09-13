using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json;
using JCLib.Mvc;

using System.Web;
using System.Web.UI;

namespace JCLib.DB
{
    public class WorkSession : IDisposable
    {
        IDbConnection _connection = null;
        UnitOfWork _unitOfWork = null;
        ApiResponse _apiResponse = null;

        public static string ConnectionStringOFFICIAL { get
            {
                var _connstr = ConfigurationManager.ConnectionStrings["DB"].ConnectionString; // 正式DB                   
                //var _connParam = _connstr.Split(';');
                //var _usr = "";
                //var _pwd = "";
                //foreach (string cp in _connParam)
                //{
                //    if (cp.IndexOf("Password") >= 0)
                //    {
                //        _pwd = cp.Split('=')[1];
                //    }
                //    if (cp.IndexOf("User ID") >= 0)
                //    {
                //        _usr = cp.Split('=')[1];
                //    }
                //}
                //var _newUsr = _usr.Remove(_usr.Length - 2, 1);
                //var _newPwd = _pwd.Remove(3, 2);
                //return _connstr.Replace(_usr, _newUsr).Replace(_pwd, _newPwd);
                return _connstr;
            }
        }

        public static string ConnectionStringTEST
        {
            get
            {
                var _connstr = ConfigurationManager.ConnectionStrings["DBT"].ConnectionString; // 測試DB          
                _connstr = "Data Source=localhost:1521/ORCL;User ID=SYSTEM;Password=zxcv1234";
                return _connstr;
            }
        }

        public static string DatabaseType
        {
            get
            {
                return JCLib.Util.GetEnvSetting("DB_TYPE");
            }
        }

        public static string GetPagingStatementORA(string sql, string sort = "")
        {
            // string db_version = JCLib.Util.GetEnvSetting("DB_VERSION");
            // 2024/02/15調整寫法以改善查詢速度,捨棄參數DB_VERSION
            sql = @"
                    SELECT * FROM(
                    SELECT TB.*, ROWNUM RN FROM(
                        SELECT COUNT(1) OVER() RC, V.* FROM ( " + sql + ") V " +
                "{0} ) TB WHERE ROWNUM <= :OFFSET + :PAGE_SIZE) WHERE RN >= :OFFSET + 1 ";

            if (sort == "")
                sort = " ORDER BY 1 ASC ";
            else
                sort = string.Format(" ORDER BY {0} ", GetSortStatement(sort));
            return string.Format(sql, sort);
        }

        public static string GetPagingStatementSQL(string sql, string sort = "")
        {
            //若效能不夠，再改用CTE來實作
            sql = @"SELECT COUNT(1) OVER() RC, V.* FROM ( " + sql + ") V " +
                    "{0} OFFSET @OFFSET ROWS FETCH NEXT @PAGE_SIZE ROWS ONLY ";
            if (sort == "")
                sort = " ORDER BY 1 ASC ";
            else
                sort = string.Format(" ORDER BY {0} ", GetSortStatement(sort));

            return string.Format(sql, sort);
        }

        public static string GetSortStatement(string json_sorters)
        {
            var s0 = JsonConvert.DeserializeObject(json_sorters).ToString();
            dynamic[] ds = JsonConvert.DeserializeObject<dynamic[]>(s0);
            List<string> sl = new List<string>();
            foreach (dynamic so in ds)
            {
                sl.Add(string.Format("{0} {1}", so.property, so.direction));
            }

            return string.Join(",", sl.ToArray<string>());
        }

        public WorkSession(IWebDataProvider provider = null, string connName = "", string DBType = "")
        {
            // 預設connName為空白(預設連線), 可指定connName決定要連線的connectionString名稱
            if (connName == "")
            {
                string DbConnType = "OFFICIAL"; // 預設為正式DB
                HttpContext context = HttpContext.Current;

                if (context != null)
                {
                    if (context.Session != null)
                    {
                        if (context.Session.Count > 0)
                        {
                            if (context.Session["DbConnType"] != null)
                            {
                                // 原測試機是由登入頁面切換,現改為取envSettings.config的DB_CONN_TYPE做判斷
                                //DbConnType = context.Session["DbConnType"].ToString();
                                if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
                                    DbConnType = "TEST";
                            }

                        }

                    }
                    else
                    {
                        if (context.Request.Cookies != null)
                        {
                            if (context.Request.Cookies["MmsmsLoginUser"] != null)
                            {
                                // 原測試機是由登入頁面切換,現改為取envSettings.config的DB_CONN_TYPE做判斷
                                // DbConnType = (string)context.Request.Cookies["MmsmsLoginUser"]["DbConnType"];
                                if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
                                    DbConnType = "TEST";
                            }

                        }
                    }
                }

                Func<string, string, string> GetPagingStatementFunc;
                switch (DatabaseType)
                {
                    case "ORA":
                        if(DbConnType == "OFFICIAL")
                        {
                            _connection = new OracleConnection(JCLib.DB.WorkSession.ConnectionStringTEST); // 測試DB
                        }
                        //if (DbConnType == "TEST")
                        //    _connection = new OracleConnection(JCLib.DB.WorkSession.ConnectionStringTEST); // 測試DB
                        //else
                        //    _connection = new OracleConnection(JCLib.DB.WorkSession.ConnectionStringOFFICIAL); // 正式DB
                        GetPagingStatementFunc = GetPagingStatementORA;
                        break;
                    case "SQL":
                        if (DbConnType == "TEST")
                            _connection = new SqlConnection(JCLib.DB.WorkSession.ConnectionStringTEST); // 測試DB
                        else
                            _connection = new SqlConnection(JCLib.DB.WorkSession.ConnectionStringOFFICIAL); // 正式DB
                        GetPagingStatementFunc = GetPagingStatementSQL;
                        break;
                    default:
                        if (DbConnType == "TEST")
                            _connection = new OracleConnection(JCLib.DB.WorkSession.ConnectionStringTEST); // 測試DB
                        else
                            _connection = new OracleConnection(JCLib.DB.WorkSession.ConnectionStringOFFICIAL); // 正式DB
                        GetPagingStatementFunc = GetPagingStatementORA;
                        break;
                }

                _connection.Open();
                _unitOfWork = new UnitOfWork(_connection, provider);
                _unitOfWork.PagingStatementFunc = GetPagingStatementFunc;
            }
            else
            {
                // 若connName有指定值則使用指定的connectionString
                if (DBType == "") // 預設為Oracle
                {
                    Func<string, string, string> GetPagingStatementFunc;
                    _connection = new OracleConnection(ConfigurationManager.ConnectionStrings[connName].ConnectionString);
                    GetPagingStatementFunc = GetPagingStatementORA;

                    _connection.Open();
                    _unitOfWork = new UnitOfWork(_connection, provider);
                    _unitOfWork.PagingStatementFunc = GetPagingStatementFunc;
                }
                else if (DBType == "SQL")
                {
                    Func<string, string, string> GetPagingStatementFunc;
                    _connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connName].ConnectionString);
                    GetPagingStatementFunc = GetPagingStatementSQL;

                    _connection.Open();
                    _unitOfWork = new UnitOfWork(_connection, provider);
                    _unitOfWork.PagingStatementFunc = GetPagingStatementFunc;
                }
            }

            _apiResponse = new ApiResponse();
        }

        public UnitOfWork UnitOfWork
        {
            get { return _unitOfWork; }
        }

        public ApiResponse Result
        {
            get { return _apiResponse; }
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            _connection.Dispose();
        }
    }
}
