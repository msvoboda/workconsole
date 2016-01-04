using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerData.Tools
{
    class HttpTools
    {
        public static string GetFormat(List<KeyValuePair<string, string>> keys, string format)
        {            
            foreach (KeyValuePair<string, string> q in keys)
            {
                if (q.Key == format)
                {
                    return q.Value;
                }
            }

            return null;
        }
    }
}
