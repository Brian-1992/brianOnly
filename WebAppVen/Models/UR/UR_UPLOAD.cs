using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppVen.Models
{
    public class UR_UPLOAD : JCLib.Mvc.BaseModel
    {
        /// <summary>
        /// File Upload GUID
        /// </summary>
        public Guid FG { get; set; }

        /// <summary>
        /// Upload Key
        /// </summary>
        public string UK { get; set; }
        public string UK1 { get; set; }
        public string UK2 { get; set; }
        public string UK3 { get; set; }
        public string UK4 { get; set; }
        public string UK5 { get; set; }

        /// <summary>
        /// Upload User
        /// </summary>
        public string TUSER { get; set; }

        /// <summary>
        /// Upload User Name
        /// </summary>
        public string UNA { get; set; }

        /// <summary>
        /// File Path
        /// </summary>
        public string FP { get; set; }

        /// <summary>
        /// File Name
        /// </summary>
        public string FN { get; set; }

        /// <summary>
        /// File Type
        /// </summary>
        public string FT { get; set; }

        /// <summary>
        /// File Size
        /// </summary>
        public long FS { get; set; }

        /// <summary>
        /// File Status
        /// </summary>
        public string ST { get; set; }

        /// <summary>
        /// Upload IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Upload Create Date
        /// </summary>
        public DateTime FC { get; set; }

        public string FD { get; set; }
    }
}