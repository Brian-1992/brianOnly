using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using JCLib.Mvc;

namespace WebAppVen.Models
{
    
    public class LoginModel
    {
        [Required]
        [Display(Name = "帳號")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Drowssap{ get; set; }

        [Required]
        [Display(Name = "驗證碼")]
        public string CaptchaCode { get; set; }

        [Display(Name = "使用行動版")]
        public bool? UseMobile { get; set; }

        //[Required]
        public string Flag { get; set; }
        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }

        [Required]
        [HiddenInput]
        public DbConnType DbConnType { get; set; }
    }
}
