using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using System.Data.Entity;
using System.Globalization;
using System.Runtime.Serialization;
using System.Web.ModelBinding;
using System.Web.Mvc;
using System.Web.Security;
using JCLib.Mvc;
using Newtonsoft.Json;

namespace WebApp.Models
{
    [DataContract]
    public class LoginModel
    {
        [Required]
        [BindRequired]
        [DataMember]
        [Display(Name = "帳號")]
        public string UserName { get; set; }

        [Required]
        [BindRequired]
        [DataMember]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Drowssap { get; set; }

        [Required]
        [BindRequired]
        [DataMember]
        [HiddenInput]
        public AuthType AuthType { get; set; } = AuthType.DB;

        [Required]
        [BindRequired]
        [HiddenInput]
        public DbConnType DbConnType { get; set; } = DbConnType.TEST;
    }
}
