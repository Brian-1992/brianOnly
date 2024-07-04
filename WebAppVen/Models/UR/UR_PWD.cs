namespace WebAppVen.Models
{
    public class UR_PWD
    {
        private string _tuser;
        public UR_PWD(string tuser)
        {
            this._tuser = tuser;
        }
        public string OLD_PWD { get; set; }
        public string NEW_PWD { get; set; }
        public string TUSER { get { return _tuser; } }
    }
}