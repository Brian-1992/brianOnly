using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    public class MEDLOCATIONModels
    {
        /// <summary>
        /// 1.院內碼
        /// </summary>
        public string CUREID { get; set; }

        /// <summary>
        /// 2.藥局位置
        /// 藥局位至SQL：select * from taizone.code_src where CODEGROUP = 'DRUGROOM'
        /// 汀州門診藥局(020)：020 021 022 023 024
        /// 汀州急診藥局：051
        /// 汀州化療藥局：062
        /// 內湖化療藥局：061
        /// 內湖急診藥局：071
        /// 內湖門診藥局(081)：081 082
        /// 內湖門診藥局(083)：083 084
        /// </summary>
        public string DRUGROOM { get; set; } 

        /// <summary>
        /// 3.儲位A
        /// </summary>
        public string LOCIDA { get; set; }

        /// <summary>
        /// 4.儲位B
        /// </summary>
        public string LOCIDB { get; set; }
    }
}
