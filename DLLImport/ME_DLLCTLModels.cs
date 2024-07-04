using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLLImport.Models
{
    public class ME_DLLCTLModels
    {
        public ME_DLLCTLModels() {
        }

        public string DLLCODE { get; set; } // 00.DLL代碼(VARCHAR2, 7)
        public string WH_NO { get; set; } // 01.庫房代碼(VARCHAR2, 10)
        public string ENDDATE { get; set; } // 02.結束時間(DATE)

    } // ec
} // en
