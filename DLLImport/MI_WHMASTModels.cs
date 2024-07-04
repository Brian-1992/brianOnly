using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLLImport.Models
{
    public class MI_WHMASTModels
    {
        public string WH_NO { get; set; } // 00.庫房代碼(VARCHAR2, 8)
        public string WH_NAME { get; set; } // 01.庫房名稱(VARCHAR2, 40)
        public string WH_KIND { get; set; } // 02.庫別分類(VARCHAR2, 1)
        public string WH_GRADE { get; set; } // 03.庫別級別(VARCHAR2, 1)
        public string PWH_NO { get; set; } // 04.上級庫(VARCHAR2, 8)
        public string INID { get; set; } // 05.責任中心(VARCHAR2, 6)
        public string SUPPLY_INID { get; set; } // 06.撥補責任中心(VARCHAR2, 6)
        public string TEL_NO { get; set; } // 07.電話分機(VARCHAR2, 20)
        public string CANCEL_ID { get; set; } // 08.是否作廢(VARCHAR2, 1)
        public string CREATE_TIME { get; set; } // 09.建立時間(DATE)
        public string CREATE_USER { get; set; } // 10.建立人員代碼(VARCHAR2, 8)
        public string UPDATE_TIME { get; set; } // 11.異動時間(DATE)
        public string UPDATE_USER { get; set; } // 12.異動人員代碼(VARCHAR2, 8)
        public string UPDATE_IP { get; set; } // 13.異動IP(VARCHAR2, 20)

    } // ec
} // en
