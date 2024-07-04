using System;
using System.Collections.Generic;
using System.Data;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;

namespace WebAppVen.Repository.UR
{
    public class UR_IDRepository : JCLib.Mvc.BaseRepository
    {
        public UR_IDRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckExists(UR_ID ur_id)
        {
            string sql = @"SELECT 1 FROM UR_ID WHERE TUSER=@TUSER";
            return !(DBWork.Connection.ExecuteScalar(sql, ur_id, DBWork.Transaction) == null);
        }

        public int Create(UR_ID ur_id)
        {
            var salt = JCLib.Encrypt.GetSalt();
            var hashPwd = JCLib.Encrypt.GetHash(ur_id.PA, salt);
            ur_id.PA = hashPwd;
            ur_id.SL = salt;
            var sql = @"INSERT INTO UR_ID (TUSER, INID, PA, SL, UNA, IDDESC, EMAIL, TEL, EXT, TITLE, FAX, FL, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, BOSS)  
                        VALUES (@TUSER, @INID, @PA, @SL, @UNA, @IDDESC, @EMAIL, @TEL, @EXT, @TITLE, @FAX, @FL, getdate(), @CREATE_USER, getdate(), @UPDATE_USER, @UPDATE_IP, @BOSS)";
            return DBWork.Connection.Execute(sql, ur_id, DBWork.Transaction);
        }

        public int Update(UR_ID ur_id)
        {
            var _afrs = 0;
            var sql = @"UPDATE UR_ID 
                            SET INID = @INID,
                            UNA = @UNA,
                            IDDESC =@IDDESC,
                            EMAIL = @EMAIL,
                            TEL = @TEL,
                            EXT =@EXT,
                            TITLE = @TITLE,
                            FAX = @FAX,
                            FL=@FL,
                            UPDATE_TIME=getdate(),
                            UPDATE_USER=@UPDATE_USER,
                            UPDATE_IP=@UPDATE_IP,
                            BOSS=@BOSS
                            WHERE TUSER=@TUSER";

            // 取消修改密碼
            //var salt = JCLib.Encrypt.GetSalt();
            //var hashPwd = JCLib.Encrypt.GetHash(ur_id.PA, salt);
            //ur_id.PA = hashPwd;
            //ur_id.SL = salt;

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
                var sql = @"UPDATE UR_ID SET PA=@NEW_PWD, SL=@SLT WHERE TUSER=@TUSER";
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
            var sql_email = "SELECT EMAIL FROM UR_ID WHERE TUSER=@TUSER";
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
            var sql = @"UPDATE UR_ID SET PA=@NEW_PWD, SL=@SLT WHERE TUSER=@TUSER";
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
            var sql = @"DELETE FROM UR_ID WHERE TUSER =@TUSER";
            return DBWork.Connection.Execute(sql, new { TUSER = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_ID> Get(string id)
        {
            var sql = @"SELECT TUSER, INID, UNA, IDDESC, EMAIL, TEL, EXT, TITLE, FAX, FL,'' AS PWD, BOSS 
                            from UR_ID where TUSER=@TUSER";
            return DBWork.Connection.Query<UR_ID>(sql, new { TUSER = id }, DBWork.Transaction);
        }
        public IEnumerable<UR_ID> GetInfo(string id)
        {
            // COLLATE Chinese_Taiwan_Stroke_CI_AS_WS: 區別全形半形
            var sql = @"SELECT TUSER, UNA, INID, INID+'_NAME' AS INID_NAME FROM UR_ID WHERE TUSER=@TUSER COLLATE Chinese_Taiwan_Stroke_CI_AS_WS";
            return DBWork.Connection.Query<UR_ID>(sql, new { TUSER = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_ID> GetAll(string tuser, string una)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TUSER, INID,
                               UNA, IDDESC, EMAIL, TEL,
                               EXT, TITLE, FAX, FL, BOSS FROM UR_ID where 1=1 ";

            if (tuser != "")
            {
                sql += " AND TUSER LIKE @p0 ";
                p.Add("@p0", string.Format("%{0}%", tuser));
            }
            if (una != "")
            {
                sql += " AND UNA LIKE @p1 ";
                p.Add("@p1", string.Format("%{0}%", una));
            }

            return DBWork.PagingQuery<UR_ID>(sql, p, DBWork.Transaction);
        }
        
        public DataTable GetExcel(string ts)
        {
            var dt = new DataTable();
            var sql = "SELECT TUSER '帳號',INID '成本中心', UNA '使用者名稱' FROM UR_ID WHERE TUSER LIKE @TS";
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
            var sql_sl = @"SELECT SL FROM UR_ID WHERE TUSER=@TUSER COLLATE Chinese_Taiwan_Stroke_CI_AS_WS
                              AND FL='1' ";
            var saltObj = DBWork.Connection.ExecuteScalar(sql_sl, new { TUSER = id }, DBWork.Transaction);
            if (saltObj == null) return false;
            else
            {
                var hashPwd = JCLib.Encrypt.GetHash(password, saltObj.ToString());
                var sql = @"SELECT UNA FROM UR_ID WHERE TUSER=@TUSER COLLATE Chinese_Taiwan_Stroke_CI_AS_WS
                               AND PA=@PA 
                               and FL='1' ";
                var obj = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id, PA = hashPwd }, DBWork.Transaction);
                if (obj != null)
                    return true;
            }
            return false;
        }
        public int WriteLogin(string sid, string tuser, string user_ip, string ap_ip)
        {
            var sql = @"INSERT INTO UR_LOGIN (SID, TUSER, USER_IP, AP_IP) VALUES (
                                                 @SID, @TUSER, @USER_IP, @AP_IP) ";
            return DBWork.Connection.Execute(sql, new { SID = (sid == "") ? null : sid, TUSER = tuser, USER_IP = user_ip, AP_IP = ap_ip }, DBWork.Transaction);
        }
        public int WriteLogout(string sid, string tuser)
        {
            var sql = @"UPDATE UR_LOGIN SET LOGOUT_DATE = GETDATE()  
                           WHERE SID = @SID AND TUSER = @TUSER";
            return DBWork.Connection.Execute(sql, new { SID = sid, TUSER = tuser }, DBWork.Transaction);
        }
        public int WriteLogoutCookieless(string tuser)
        {
            var sql = @"UPDATE UR_LOGIN SET LOGOUT_DATE = GETDATE()  
                           WHERE LOGIN_DATE = (SELECT LOGIN_DATE FROM (
                            SELECT TOP 1 LOGIN_DATE, LOGOUT_DATE FROM UR_LOGIN WHERE TUSER = @TUSER ORDER BY LOGIN_DATE DESC) 
                            V WHERE V.LOGOUT_DATE IS NULL)";
            return DBWork.Connection.Execute(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public object CheckAccess(string fg, string tuser)
        {
            var sql = @"SELECT str(R,1)+','+str(U,1)+','+str(P,1) FROM UR_TACL WHERE FG=@FG AND V=1 AND RLNO IN (SELECT RLNO FROM UR_UIR WHERE TUSER=@TUSER)";
            var obj = DBWork.Connection.ExecuteScalar(sql, new { FG = fg, TUSER = tuser }, DBWork.Transaction);
            if (obj != null)
            {
                var auth = obj.ToString().Split(',');
                return new { R = auth[0] == "1", U = auth[1] == "1", P = auth[2] == "1" };
            }

            return null;
        }

        public IEnumerable<ComboModel> GetEncBtn(string tuser)
        {
            var p = new DynamicParameters();

            var sql = @" select RLNO as VALUE from UR_UIR where TUSER = @TUSER ";

            return DBWork.Connection.Query<ComboModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }
    }
}