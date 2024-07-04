using System;
using System.Configuration;
using System.Collections.Specialized;

namespace JCLib
{
    public class Util
    {
        public static string GetEnvSetting(string key)
        {
            var envSettings = ConfigurationManager.GetSection("envSettings") 
                as NameValueCollection;
            return envSettings[key];
        }
    }
}
