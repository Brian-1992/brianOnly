using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using JCLib.DB.Impl;

namespace JCLib.DB
{
    public class DBUsageExample
    {
        public DBUsageExample()
        {
            IDBAccess db = new SqlDB(new SqlConnection(""));

            using (db)
            {
                db.BeginTransaction();
                try
                {
                    //ExecScalar
                    CommandParam p1 = new CommandParam();
                    p1.CommandText = @"SELECT SINGLE_VALUE FROM TABLE";
                    p1.AddParam("參數名", "參數值");

                    object result1 = db.ExecScalar(p1);

                    //ExecReader
                    CommandParam p2 = new CommandParam();
                    p2.CommandText = @"SELECT SOME_VALUE FROM TABLE";
                    p2.AddParam("參數名", "參數值");

                    List<string> result2 = new List<string>();
                    db.ExecReader(p2, (reader) =>
                    {
                        //while reader.Read()時，迴圈內執行的內容
                        result2.Add(reader["SOME_VALUE"].ToString());
                    });

                    //ExecQuery
                    CommandParam p3 = new CommandParam();
                    p3.CommandText = @"SELECT SOME_VALUE FROM TABLE";
                    p3.AddParam("參數名", "參數值");

                    DataTable result3 = db.ExecQuery(p3);

                    //ExecNonQuery
                    CommandParam p4 = new CommandParam();
                    p4.CommandText = @"UPDATE TABLE SET SOME_FIELD = SOME_VALUE";
                    p4.AddParam("參數名", "參數值");

                    int result4 = db.ExecNonQuery(p4);

                    db.Commit();
                }
                catch (Exception ex)
                {
                    db.Rollback();
                    throw ex;
                }
            }
        }
    }
}