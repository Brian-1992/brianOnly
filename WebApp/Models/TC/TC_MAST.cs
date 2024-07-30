using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TC_MAST
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public TC_MAST() {
            MMCODE = string.Empty;
            MMNAME_C = string.Empty;
            CREATE_USER = string.Empty;
            UPDATE_USER = string.Empty;
            UPDATE_IP = string.Empty;
        }

        public TC_MAST(string mmcode, string mmname_c, string userId, string ip) {
            MMCODE = mmcode;
            MMNAME_C = mmname_c;
            CREATE_USER = userId;
            UPDATE_USER = userId;
            UPDATE_IP = ip;
        }
    }
}