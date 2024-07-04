using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    /// <summary>
    /// 介接HIS資料庫 code_src資料表(代碼定義檔)
    /// </summary>
    public class BASCODEAModels
    {
        public string CODEID { get; set; } //1.代碼代號
        public string CODEGROUP { get; set; } //2.所屬群屬代碼
        public string CODECLASS1 { get; set; } //3.層級1代碼
        public string CODECLASS2 { get; set; } //4.層級2代碼
        public string CODELABEL { get; set; } //5.標題
        public string MEMO { get; set; } //6.備註
        public string BUILDDATE { get; set; } //7.建立日期
        public string MODIDATE { get; set; } //8.最後修改日期
        public string MAXCODELENGTH { get; set; } //9.各代碼代號最大長度（配合各Table）
        public string AREAGROUP { get; set; } //10.區域代碼
    }
}
