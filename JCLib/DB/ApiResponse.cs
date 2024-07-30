using System;
using System.Data;
using System.Collections.Generic;

namespace JCLib.DB
{
    public class ApiResponse
    {
        private int _rc = 0;
        private IEnumerable<object> _etts;
        public DataSet ds { get; set; }
        public bool success { get; set; }
        public string msg { get; set; }
        public int afrs;
        public int rc { get { return _rc; } }
        public object etts {
            get {
                return _etts;
            }
            set {
                if (value != null)
                {
                    _etts = (IEnumerable<object>)value;
                    var _e = _etts.GetEnumerator();
                    if (_e.MoveNext())
                        if(_e.Current is JCLib.Mvc.BaseModel)
                            _rc = ((JCLib.Mvc.BaseModel)_e.Current).RC ?? 0;
                }
            }
        }

        public ApiResponse()
        {
            //ds = new DataSet();
            success = true;
            msg = "";
            afrs = 0;
        }
    }
}