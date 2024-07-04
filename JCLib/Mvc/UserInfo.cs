using Newtonsoft.Json;
using System;

namespace JCLib.Mvc
{
    /// <summary>
    /// 【使用者資訊物件】
    /// 1.建構子初始化，先傳入字串形式參數
    /// 2.要用到的時候再做轉換
    /// 3.屬性設計為唯讀
    /// </summary>
    public class UserInfo
    {
        private bool _parsed = false;
        private string _userData = "";
        private UserInfoBase _userInfoBase;
        public UserInfo() { }
        public UserInfo(string userData)
        {
            this._userData = userData;
        }
        public UserInfo(AuthType authType, string userId, string userName, string inid, string inidName, bool VIEWALL)
        {
            _userInfoBase = new UserInfoBase();
            _userInfoBase.SessionId = Guid.NewGuid();
            _userInfoBase.AuthType = authType;
            _userInfoBase.UserId = userId;
            _userInfoBase.UserName = userName;
            _userInfoBase.Inid = inid;
            _userInfoBase.InidName = inidName;
            _userInfoBase.VIEWALL = VIEWALL;
            _parsed = true;
        }

        private void TryParse()
        {
            if (!_parsed)
            {
                this._userInfoBase = JsonConvert.DeserializeObject<UserInfoBase>(_userData);
                _parsed = true;
            }
        }
        public Guid SessionId { get { TryParse(); return _userInfoBase.SessionId; } }
        public AuthType AuthType { get { TryParse(); return _userInfoBase.AuthType; } }
        public string UserId { get { TryParse(); return _userInfoBase.UserId; } }
        public string UserName { get { TryParse(); return _userInfoBase.UserName; } }
        public string Inid { get { TryParse(); return _userInfoBase.Inid; } }
        public string InidName { get { TryParse(); return _userInfoBase.InidName; } }
        public bool VIEWALL { get { TryParse(); return _userInfoBase.VIEWALL; } }

        class UserInfoBase
        {
            public Guid SessionId { get; set; }
            public AuthType AuthType { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string Inid { get; set; }
            public string InidName { get; set; }
            public bool VIEWALL { get; set; }
        }
    }

    public enum AuthType : int
    {
        DB = 0,
        AD = 1,
        API = 2,
        HISDB = 3
    }

    public enum DbConnType : int
    {
        OFFICIAL = 0,
        TEST = 1
    }
}
