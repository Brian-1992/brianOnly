namespace WebApp.Models
{
    public class AA0082 : JCLib.Mvc.BaseModel
    {
        public string ROWITEM { get; set; } //項次
        public string MMCODE { get; set; } //藥品院內碼
        public string MMNAME_E { get; set; } //藥品名稱
        public string DAYAVGQTY { get; set; } //日平均消耗量
        public string DAYAMOUNT { get; set; } //日平均消耗金額
        public string MONAVGQTY { get; set; } //月平均消耗量
        public string MONAMOUNT { get; set; } //月平均消耗金額
        public string PURCHASE1 { get; set; } //甲案
        public string PURCHASE2 { get; set; } //乙案
        public string SIGN_TIME { get; set; }
    }
}