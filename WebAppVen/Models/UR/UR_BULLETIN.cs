namespace WebAppVen.Models
{
    public class UR_BULLETIN : JCLib.Mvc.BaseModel
    {
        public string ID { get; set; }
        public string TITLE { get; set; }
        public string CONTENT { get; set; }
        public string TARGET { get; set; }
        public string ON_DATE { get; set; }
        public string OFF_DATE { get; set; }
        public string VALID { get; set; }
        public string CREATE_BY { get; set; }
        public string CREATE_DT { get; set; }
        public string UPDATE_BY { get; set; }
        public string UPDATE_DT { get; set; }

        public string ATTACH_CNT { get; set; }
    }
}