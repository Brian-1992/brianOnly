using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace MMSMSBAS
{
    public class TsghConn
    {
        //AIDC
        //IP 192.168.4.228(舊版)
        //PORT 5000
        //DB名稱 TSGHHIS
        //DB帳號 tsghhis
        //DB密碼 his@aidc0102

        /*6/18 AIDC黃小姐(DBA)來信
         * 因資料庫編碼的關係，我們改用另一台Sybase DB作測試，已成功匯入中文，
         *連線字串修改如下，異動部分以藍字標示，給您開發dll參考，謝謝。
         * Driver={Adaptive Server Enterprise}; server=192.168.1.48;port=5000;db=TSGHHIS;uid=tsghhis;pwd=his @aidc0102; Connect Timeout = 300; charset=utf8;
         */

        public string TsghDB_IPD()
        {
            var connStringIPD = "Driver={Adaptive Server Enterprise};server=三總IP;port=三總Port;db=三總資料庫名稱;uid=三總帳號;pwd=三總密碼;CharSet=big5;";
            var specConn = getSpecConn("DB_Sybase_Released");
            if (!String.IsNullOrEmpty(specConn)) // 如果在connection.config檔中，有特別設定的話，就讀DB_Sybase_Released的連線字串
                connStringIPD = specConn;
            return connStringIPD;
        }

        public string TsghDB_OPD()
        {
            var connStringOPD = "Driver={Adaptive Server Enterprise};server=三總IP;port=三總Port;db=三總資料庫名稱;uid=三總帳號;pwd=三總密碼;CharSet=big5;";
            var specConn = getSpecConn("DB_Sybase_Released");
            if (!String.IsNullOrEmpty(specConn)) // 如果在connection.config檔中，有特別設定的話，就讀DB_Sybase_Released的連線字串
                connStringOPD = specConn;
            return connStringOPD;
        }

        // 取得資料庫特別指定的連線字串
        String getSpecConn(String name)
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
