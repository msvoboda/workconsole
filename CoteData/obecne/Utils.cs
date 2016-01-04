using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace IntegratorService.Utils
{
    public static class FileUtils
    {
        public static string loadFile(string fname)
        {
            Uri uri = new Uri(fname);
            StreamReader streamReader = new StreamReader(uri.LocalPath);
            string text = streamReader.ReadToEnd();
            streamReader.Close();               
            return text;           
        }
    }
}
