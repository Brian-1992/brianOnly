using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BG0011 : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO { get; set; }         // �t�ӥN�X
        public string AGEN_NAMEC { get; set; }      // �t�ӦW��
        public string INVOICE { get; set; }         // �o�����X
        public string UNI_NO { get; set; }          // �Τ@�s��
        public string MONEY_1 { get; set; }         // ��I���B
        public string MONEY_2 { get; set; }         // �X���u�f
        public string MONEY_3 { get; set; }         // ���I���B
    }
}