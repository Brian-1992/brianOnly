﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class ApiReturnItem
    {
        public string Message { get; set; }
        public IEnumerable<Object> Datas { get; set; }
    }
}