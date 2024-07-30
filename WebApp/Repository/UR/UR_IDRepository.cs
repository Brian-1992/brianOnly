using System;
using System.Collections.Generic;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.UR
{
    public class UR_IDRepository : JCLib.Mvc.BaseRepository
    {
        public UR_IDRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckExists(UR_ID ur_id)
        {
            string sql = @"SELECT 1 FROM UR_ID WHERE TUSER=:TUSER";
            return !(DBWork.Connection.ExecuteScalar(sql, ur_id, DBWork.Transaction) == null);
        }

        public int Create(UR_ID ur_id)
        {
            var salt = JCLib.Encrypt.GetSalt();
            var hashPwd = JCLib.Encrypt.GetHash(ur_id.PA, salt);
            ur_id.PA = hashPwd;
            ur_id.SL = salt;
            var sql = @"INSERT INTO UR_ID (TUSER, INID, PA, SL, UNA, IDDESC, EMAIL, TEL, EXT, TITLE, FAX, FL, ADUSER, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3)  
                        VALUES (:TUSER, :INID, :PA, :SL, :UNA, :IDDESC, :EMAIL, :TEL, :EXT, :TITLE, :FAX, :FL, :ADUSER, sysdate, :CREATE_USER, sysdate, :UPDATE_USER, :UPDATE_IP, :WHITELIST_IP1, :WHITELIST_IP2, :WHITELIST_IP3)";
            return DBWork.Connection.Execute(sql, ur_id, DBWork.Transaction);
        }

        public int CreateForAdUser(UR_ID ur_id)
        {
            var sql = @"INSERT INTO UR_ID (TUSER, INID, UNA, EMAIL, ADUSER, FL, CREATE_TIME, CREATE_USER)  
                        VALUES (:TUSER, :INID, :UNA, :EMAIL, :ADUSER, '0', sysdate, 'system')";
            return DBWork.Connection.Execute(sql, ur_id, DBWork.Transaction);
        }

        public int UpdateAduser(string aduser)
        {
            var sql = @" update UR_ID set ADUSER = :ADUSER
                        where lower(ADUSER) = lower(:ADUSER) ";
            return DBWork.Connection.Execute(sql, new { ADUSER = aduser }, DBWork.Transaction);
        }

        public int Update(UR_ID ur_id)
        {
            var _afrs = 0;
            var sql = @"UPDATE UR_ID 
                            SET INID = :INID,
                            UNA = :UNA,
                            IDDESC =:IDDESC,
                            EMAIL = :EMAIL,
                            TEL = :TEL,
                            EXT =:EXT,
                            TITLE = :TITLE,
                            FAX = :FAX,
                            FL=:FL,
                            UPDATE_TIME=sysdate,
                            UPDATE_USER=:UPDATE_USER,
                            UPDATE_IP=:UPDATE_IP,
                            WHITELIST_IP1 = :WHITELIST_IP1, 
                            WHITELIST_IP2 = :WHITELIST_IP2, 
                            WHITELIST_IP3 = :WHITELIST_IP3
                            WHERE TUSER=:TUSER";

            /* 取消修改密碼
            var salt = JCLib.Encrypt.GetSalt();
            var hashPwd = JCLib.Encrypt.GetHash(ur_id.PA, salt);
            ur_id.PA = hashPwd;
            ur_id.SL = salt;
            */

            _afrs = DBWork.Connection.Execute(sql, ur_id, DBWork.Transaction);

            return _afrs;
        }

        public int Change(UR_PWD ur_pwd)
        {
            var _afrs = 0;
            if (CheckLogin(ur_pwd.TUSER, ur_pwd.OLD_PWD))
            {
                var salt = JCLib.Encrypt.GetSalt();
                var hashPwd = JCLib.Encrypt.GetHash(ur_pwd.NEW_PWD, salt);
                var sql = @"UPDATE UR_ID SET PA=:NEW_PWD, SL=:SLT WHERE TUSER=:TUSER";
                _afrs = DBWork.Connection.Execute(sql, new { NEW_PWD = hashPwd, SLT = salt, TUSER = ur_pwd.TUSER }, DBWork.Transaction);
            }
            return _afrs;
        }

        public bool ResetPassword(string tuser, string subject = "密碼更新通知函", string content = "您的新密碼:{0}")
        {
            return (ResetPasswordByAdmin(tuser, subject, content) == "");
        }

        public string ResetPasswordByAdmin(string tuser, string subject = "密碼更新通知函", string content = "您的新密碼:{0}")
        {
            var sql_email = "SELECT EMAIL FROM UR_ID WHERE TUSER=:TUSER";
            var email = DBWork.Connection.ExecuteScalar(sql_email, new { TUSER = tuser }, DBWork.Transaction);
            if (email == null) return "無此帳號";
            if (email == DBNull.Value) return "此帳號無Email";

            var new_pwd = JCLib.Password.Generate(8);
            var _afrs = UpdatePassword(tuser, new_pwd);

            if (_afrs == 0) return "更新密碼失敗";
            else if (_afrs > 0)
            {
                var _msg = JCLib.Mail.Send(subject, string.Format(content, new_pwd), email.ToString());
                if (_msg != "") return string.Format("郵件寄送有誤:{0}", _msg);
            }

            return "";
        }

        public int UpdatePassword(string tuser, string pwd)
        {
            var _afrs = 0;
            var salt = JCLib.Encrypt.GetSalt();
            var hashPwd = JCLib.Encrypt.GetHash(pwd, salt);
            var sql = @"UPDATE UR_ID SET PA=:NEW_PWD, SL=:SLT WHERE TUSER=:TUSER";
            _afrs = DBWork.Connection.Execute(sql, new { NEW_PWD = hashPwd, SLT = salt, TUSER = tuser }, DBWork.Transaction);

            return _afrs;
        }

        public int EncryptAll()
        {
            var _afrs = 0;
            var sql = @"SELECT TUSER, PA FROM UR_ID WHERE SL IS NULL";
            var users = DBWork.Connection.Query<UR_ID>(sql, null, DBWork.Transaction);

            foreach(UR_ID user in users)
            {
                _afrs += UpdatePassword(user.TUSER, user.PA);
            }
            return _afrs;
        }

        public int Delete(string id)
        {
            var sql = @"DELETE FROM UR_ID WHERE TUSER =:TUSER";
            return DBWork.Connection.Execute(sql, new { TUSER = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_ID> Get(string id)
        {
            var sql = @"SELECT TUSER, INID, (select INID_NAME from UR_INID where INID = UR_ID.INID) as INID_NAME, 
                            UNA, IDDESC, EMAIL, TEL, EXT, TITLE, FAX, FL,'' AS PWD, ADUSER, WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3
                            from UR_ID where TUSER=:TUSER";
            return DBWork.Connection.Query<UR_ID>(sql, new { TUSER = id }, DBWork.Transaction);
        }
        public IEnumerable<UR_ID> GetInfo(string id)
        {
            var sql = @"SELECT TUSER, UNA, INID, INID_NAME(INID) AS INID_NAME FROM UR_ID WHERE TUSER=:TUSER";
            return DBWork.Connection.Query<UR_ID>(sql, new { TUSER = id }, DBWork.Transaction);
        }
        public IEnumerable<UR_ID> GetInfoByAD(string id)
        {
            var sql = @"SELECT TUSER, UNA, INID, INID_NAME(INID) AS INID_NAME FROM UR_ID WHERE lower(ADUSER)=lower(:ADUSER) order by FL desc, TUSER ";
            return DBWork.Connection.Query<UR_ID>(sql, new { ADUSER = id }, DBWork.Transaction);
        }

        public bool GetEnabledByAD(string id)
        {
            var sql = @"SELECT NVL(FL, 0) FROM UR_ID WHERE lower(ADUSER)=lower(:ADUSER) order by FL desc, TUSER ";
            return DBWork.Connection.ExecuteScalar(sql, new { ADUSER = id }, DBWork.Transaction).ToString() == "1";
        }

        public string GetNextTuserByAD(string id)
        {
            //var sql = @"SELECT MAX(TUSER) FROM UR_ID WHERE ADUSER IS NOT NULL AND LENGTH(ADUSER)>=8";
            var sql = @"SELECT MAX(TUSER) FROM UR_ID WHERE ADUSER IS NOT NULL";
            string tuser = (DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction) ?? "A0000000").ToString();
            char fc = tuser.ToCharArray()[0];
            int seq = int.Parse(tuser.Substring(1, 7));
            if (seq == 9999999)
            {
                fc++;
                seq = 1;
            }
            else
            {
                seq++;
            }

            return Char.ConvertFromUtf32(fc) + seq.ToString().PadLeft(7, '0');
        }

        public string TryGetInid(string inid)
        {
            var sql = @"SELECT INID FROM UR_INID WHERE INID = :INID ";

            return (DBWork.Connection.ExecuteScalar(sql, new { INID = inid }, DBWork.Transaction) ?? "zzzzzz").ToString();
        }

        public IEnumerable<UR_ID> GetAll(string tuser, string una, string aduser, string fl, string inid)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TUSER, INID, (select INID_NAME from UR_INID where INID = UR_ID.INID) as INID_NAME,
                               UNA, IDDESC, EMAIL, TEL,
                               EXT, TITLE, FAX, FL, ADUSER,  
                               WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3
                               FROM UR_ID where 1=1 ";

            if (tuser != "" && tuser != null)
            {
                sql += " AND TUSER LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", tuser));
            }
            if (una != "" && una != null)
            {
                sql += " AND UNA LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", una));
            }
            if (aduser != "" && aduser != null)
            {
                sql += " AND ADUSER LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", aduser));
            }
            if (fl != "" && fl != null)
            {
                if (fl == "1")
                {
                    sql += " AND FL = :p3 ";
                    p.Add(":p3", fl);
                }
                else if (fl == "0")
                    sql += " AND (FL = '0' or FL is null) ";
            }
            if (inid != "" && inid != null)
            {
                sql += " AND INID like :p4 ";
                p.Add(":p4", string.Format("{0}%", inid));
            }

            return DBWork.PagingQuery<UR_ID>(sql, p, DBWork.Transaction);
        }
        
        public DataTable GetExcel(string ts)
        {
            var dt = new DataTable();
            var sql = "SELECT TUSER '帳號',INID '成本中心', UNA '使用者名稱' FROM UR_ID WHERE TUSER LIKE :TS";
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { TS = ts + "%" }, DBWork.Transaction))
            {
                dt.Load(rdr);
            }
            return dt;
        }

        public bool CheckLogin(LoginModel login)
        {
            return CheckLogin(login.UserName, login.Drowssap);
        }

        private bool CheckLogin(string id, string password)
        {
            var sql_sl = "SELECT SL FROM UR_ID WHERE TUSER=:TUSER AND FL='1'";
            var saltObj = DBWork.Connection.ExecuteScalar(sql_sl, new { TUSER = id }, DBWork.Transaction);
            if (saltObj == null) return false;
            else
            {
                var hashPwd = JCLib.Encrypt.GetHash(password, saltObj.ToString());
                var sql = "SELECT UNA FROM UR_ID WHERE TUSER=:TUSER AND PA=:PA and FL='1' ";
                var obj = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id, PA = hashPwd }, DBWork.Transaction);
                if (obj != null)
                    return true;
            }
            return false;
        }

        public bool CheckHISDBLogin(string id, string password)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = "SELECT EM_EMPNAME, EM_DEPTNO FROM PacsTREEMP WHERE EM_EMPNO=@TUSER AND EM_PASSWORD=@PA ";
            p.Add("@TUSER", string.Format("{0}", id));
            p.Add("@PA", string.Format("{0}", password));
            var obj = DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction);
            if (obj != null)
                return true;
            return false;
        }

        public UR_ID GetHISDBLogin(string id, string password)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @" SELECT EM_EMPNAME, EM_DEPTNO FROM PacsTREEMP WHERE EM_EMPNO=@TUSER AND EM_PASSWORD=@PA ";
            p.Add("@TUSER", string.Format("{0}", id));
            p.Add("@PA", string.Format("{0}", password));
            return DBWork.Connection.QueryFirstOrDefault<UR_ID>(sql, p, DBWork.Transaction);
        }

        public bool CheckValidIp(LoginModel login, string procIp)
        {
            bool rtnBool = true;

            // 若PARAM_D的LOGIN_CHECK值有設為Y, 則需要檢查登入帳號對應的IP是否可登入
            string sql1 = @" select count(*) from PARAM_D
                            where GRP_CODE = 'LOGIN_CHECK'
                            and DATA_VALUE = 'Y' ";

            int needChkIp = DBWork.Connection.QueryFirst<int>(sql1, null, DBWork.Transaction);

            if (needChkIp > 0)
            {
                string sql2 = @" select count(*) from UR_ID
                            where TUSER = :TUSER
                            and (WHITELIST_IP1 = :USERIP1 or WHITELIST_IP2 = :USERIP2 or WHITELIST_IP3 = :USERIP3) ";

                int chkIp = DBWork.Connection.QueryFirst<int>(sql2, new { TUSER = login.UserName, USERIP1 = procIp, USERIP2 = procIp, USERIP3 = procIp }, DBWork.Transaction);
                if (chkIp > 0)
                    rtnBool = true;
                else
                    rtnBool = false;
            }
            else
                rtnBool = true;



            return rtnBool;
        }

        public int WriteLogin(string sid, string tuser, string user_ip, string ap_ip)
        {
            var sql = @"INSERT INTO UR_LOGIN (SID, TUSER, USER_IP, AP_IP, LOGIN_DATE) VALUES (
                                                 :SID, :TUSER, :USER_IP, :AP_IP, SYSDATE) ";
            return DBWork.Connection.Execute(sql, new { SID = (sid == "") ? null : sid, TUSER = tuser, USER_IP = user_ip, AP_IP = ap_ip }, DBWork.Transaction);
        }
        public int WriteLogout(string sid, string tuser)
        {
            var sql = @"UPDATE UR_LOGIN SET LOGOUT_DATE = SYSDATE   
                           WHERE SID = :SID AND TUSER = :TUSER";
            return DBWork.Connection.Execute(sql, new { SID = sid, TUSER = tuser }, DBWork.Transaction);
        }
        public int WriteLogoutCookieless(string tuser)
        {
            var sql = @"UPDATE UR_LOGIN SET LOGOUT_DATE = SYSDATE  
                           WHERE LOGIN_DATE = (SELECT LOGIN_DATE FROM (
                            SELECT TOP 1 LOGIN_DATE, LOGOUT_DATE FROM UR_LOGIN WHERE TUSER = :TUSER ORDER BY LOGIN_DATE DESC) 
                            V WHERE V.LOGOUT_DATE IS NULL)";
            return DBWork.Connection.Execute(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public object CheckAccess(string fg, string tuser)
        {
            var sql = @"SELECT R||','||U||','||P FROM UR_TACL WHERE FG=:FG AND V=1 AND RLNO IN (SELECT RLNO FROM UR_UIR WHERE TUSER=:TUSER)
                        UNION
                        SELECT R||','||U||','||P FROM UR_TACL2 WHERE FG=:FG AND TUSER=:TUSER";

            /*
            var obj = DBWork.Connection.ExecuteScalar(sql, new { FG = fg, TUSER = tuser }, DBWork.Transaction);

            if (obj != null)
            {
                var auth = obj.ToString().Split(',');
                return new { R = auth[0] == "1", U = auth[1] == "1", P = auth[2] == "1" };
            }

            return null;
            */

            var hasAccess = false;
            var result = new bool[] { false, false, false };
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { FG = fg, TUSER = tuser }, DBWork.Transaction))
            {
                while (rdr.Read())
                {
                    hasAccess = true;
                    var auth = rdr[0].ToString().Split(',');
                    for (int i = 0; i < 3; i++)
                        result[i] |= auth[i] == "1";
                }
            }

            if (hasAccess) return new { R = result[0], U = result[1], P = result[2] };
            return null;
        }

        public int chkADdup(string tuser, string aduser, string una, string fl)
        {
            // 當本次修改資料, 有將FL從0改成1,且此AD帳號已有同名且啟用
            var p = new DynamicParameters();
            var sql = @" select count(*) from UR_ID
                            where TUSER = :TUSER
                            and FL = '0'
                            and :FL = '1'
                            and (select count(*) from UR_ID where lower(ADUSER) = lower(:ADUSER) and UNA = :UNA and FL = '1') > 0";

            return DBWork.Connection.QueryFirst<int>(sql, new { TUSER = tuser, ADUSER = aduser, UNA = una, FL = fl }, DBWork.Transaction);
        }

        #region 2020-05-01 衛材上線準備
        //public string GetCurrentDate() {
        //    string sql = @"select twn_date(sysdate) from dual";

        //    return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        //}
        #endregion

        public string GetSysContactMsg()
        {
            string sql = @"select
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'CONTACT' and DATA_NAME = 'SysInid')
                || (select DATA_VALUE from PARAM_D where GRP_CODE = 'CONTACT' and DATA_NAME = 'SysTel')
                || (select DATA_VALUE from PARAM_D where GRP_CODE = 'CONTACT' and DATA_NAME = 'SysName')
                as CONTACT
                from dual";

            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public string GetAdContactMsg()
        {
            string sql = @"select
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'CONTACT' and DATA_NAME = 'AdInid')
                || (select DATA_VALUE from PARAM_D where GRP_CODE = 'CONTACT' and DATA_NAME = 'AdTel')
                || (select DATA_VALUE from PARAM_D where GRP_CODE = 'CONTACT' and DATA_NAME = 'AdName')
                as CONTACT
                from dual";

            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public string CheckValidString(string inputstr)
        {
            var sql = @"SELECT :RTNSTR FROM dual ";

            return (DBWork.Connection.ExecuteScalar(sql, new { RTNSTR = inputstr }, DBWork.Transaction) ?? "").ToString();
        }
    }
}