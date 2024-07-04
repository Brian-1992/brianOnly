using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0038VM : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; } //順序
        public string CNAME { get; set; } //中文名稱
        public string ENAME { get; set; } //英文名稱
        public string CHK { get; set; } //選擇FLAG
        public string IP { get; set; } //使用者IP
    }
}
