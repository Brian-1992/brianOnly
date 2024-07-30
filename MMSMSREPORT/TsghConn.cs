using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MMSMSREPORT
{
    class TsghConn
    {
        //AIDC
        //IP 192.168.4.228
        //PORT 5000
        //DB名稱 TSGHHIS
        //DB帳號 tsghhis
        //DB密碼 his@aidc0102
        public string TsghDB_IPD()
        {
            var connStringIPD = "Driver={Adaptive Server Enterprise};server=三總IP;port=三總Port;db=三總資料庫名稱;uid=三總帳號;pwd=三總密碼;CharSet=big5;";
            var specConn = getSpecConn("DB_Sybase_Released");
            if (!String.IsNullOrEmpty(specConn)) // 如果在connection.config檔中，有特別設定的話，就讀DB_Sybase_Released的連線字串
                connStringIPD = specConn;
            return connStringIPD;
        }

        // 取得資料庫特別指定的連線字串
        public static String getSpecConn(String name)
        {
            if (ConfigurationManager.ConnectionStrings[name] != null)
            {
                if (!String.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[name].ConnectionString))
                {
                    return ConfigurationManager.ConnectionStrings[name].ConnectionString;
                }
            }
            return "";
        } // 
    }
}
