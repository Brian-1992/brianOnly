using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class LoginUserRequest
    {
        public string Account { get; set; } = "";
        public string Password { get; set; } = "";
        public string Token { get; set; } = "";
    }
}