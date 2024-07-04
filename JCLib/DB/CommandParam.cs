using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCLib.DB
{
    public class CommandParam
    {
        public string CommandText;
        public string TableName;

        private List<KeyValuePair<string, object>> _parameters;

        public List<KeyValuePair<string, object>> Parameters { get { return _parameters; } }

        public CommandParam(string command_text = "")
        {
            CommandText = command_text;
            _parameters = new List<KeyValuePair<string, object>>();
        }

        public void AddParam(string param_name, object param_value)
        {
            _parameters.Add(new KeyValuePair<string, object>(param_name, param_value));
        }
    }
}
