using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Utils;
using ServerData;

namespace PgSql.Config
{    
    public class CartmasterConfiguration : ICmConfiguration
    {
        private DataDictionary<string,Slonconfig> _configurations = new DataDictionary<string, Slonconfig>();

        public bool LoadPgConfig(string iniFile)
        {
            string path = "";
            if (iniFile.Length > 2 && !(iniFile[1] == '\\' || iniFile[1] == ':')) //neni-li plna cesta
            {
                path = Assembly.GetExecutingAssembly().Location;
                path = path.Remove(path.LastIndexOf("\\") + 1);
            }
            path += iniFile;
            if (!File.Exists(path))
                return false;
            
            IniFile nf = new IniFile(path);
            string[] sections = nf.IniReadSections();

            foreach (string section in sections)
            {
                Slonconfig cfg = new Slonconfig();
                cfg.strName = section;
                cfg.strServer = nf.IniReadValue(section, "HOST", "");
                cfg.strPort = nf.IniReadValue(section, "PORT", "");
                cfg.strUser = nf.IniReadValue(section, "USER", "");
                cfg.strPassword = Slonconfig.DecodeString(nf.IniReadValue(section, "PASS", ""));
                cfg.strDatabase = nf.IniReadValue(section, "DATABASE", "");
                cfg.strTimeout = nf.IniReadValue(section, "TIMEOUT", "");
                _configurations.Add(section, cfg);
            }
            
            return true;
        }

        public IConnectionConfig GetConfiguration(string name)
        {
            if (_configurations.ContainsKey(name) == false)
                return null;
            else
                return _configurations[name];
        }
    }

    public class IniFile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
          string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
          string key, string def, StringBuilder retVal,
          int size, string filePath);

        [DllImport("kernel32.dll")]
        private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

        public IniFile(string INIPath)
        {
            path = INIPath;
        }

        public string[] IniReadSections()
        {
            uint MAX_BUFFER = 32767;
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER);            
            uint nSize=0;

            uint bytesReturned = GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, path);
            if (bytesReturned == 0)
            {
                Marshal.FreeCoTaskMem(pReturnedString);
                return null;
            }
            string local = Marshal.PtrToStringAnsi(pReturnedString, (int)bytesReturned).ToString();
            Marshal.FreeCoTaskMem(pReturnedString);
            //use of Substring below removes terminating null for split
            return local.Substring(0, local.Length - 1).Split('\0');
        }

        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }

        public string IniReadValue(string Section, string Key, string vdef)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, vdef, temp, 255, this.path);
            return temp.ToString();
        }
    }

    public class Slonconfig : IConnectionConfig
    {
        public string strName;
        public string strServer;
        public string strPort;
        public string strUser;
        public string strPassword;
        public string strDatabase;
        public string strTimeout;
        public int connectTimeout;

        public static bool LoadSlonConfig(string iniFile, string section, ref Slonconfig cfg)
        {
            string path = "";
            if (iniFile.Length > 2 && !(iniFile[1] == '\\' || iniFile[1] == ':')) //neni-li plna cesta
            {
                path = Assembly.GetExecutingAssembly().Location;
                path = path.Remove(path.LastIndexOf("\\") + 1);
            }
            path += iniFile;
            if (!File.Exists(path))
                return false;
            IniFile nf = new IniFile(path);
            cfg.strName = section;
            cfg.strServer = nf.IniReadValue(section, "HOST", "");
            cfg.strPort = nf.IniReadValue(section, "PORT", "");
            cfg.strUser = nf.IniReadValue(section, "USER", "");
            cfg.strPassword = DecodeString(nf.IniReadValue(section, "PASS", ""));
            cfg.strDatabase = nf.IniReadValue(section, "DATABASE", "");
            cfg.strTimeout = nf.IniReadValue(section, "TIMEOUT", "");

            //cfg.connectTimeout = MainCnf.ConfigGet("dbTimeout", 30); //30 je default pro mou aplikaci = v sekundach

            return true;
        }

        public static string DecodeString(string ins)
        {
            if (ins.Length < 2)
                return "";
            try
            {
                string key = "CARTMASTER", s;
                int n = ins.Length / 2 - 1;
                byte[] outs = new byte[n + 1];
                char[] c = new char[2];

                for (int i = 0; i < n + 1; i++)
                {
                    c[0] = ins[i * 2];
                    c[1] = ins[i * 2 + 1];
                    outs[i] = (byte)Convert.ToInt32(new string(c), 16);
                }

                byte x0 = outs[0];
                for (int i = 0; i < n; i++)
                {
                    byte xa = Convert.ToByte(key[(i % key.Length)]);
                    byte x = Convert.ToByte((sbyte)((outs[i + 1] ^ xa) - x0));
                    x0 = outs[i + 1];
                    outs[i + 1] = x;
                }
                s = System.Text.ASCIIEncoding.ASCII.GetString(outs); // + 1
                return s.Substring(1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "";
            }
        }

        public string Name
        {
            get
            {
                return strName;
            }
            set
            {
                strName = value;
            }
        }

        public string Database
        {
            get
            {
                return strDatabase;
            }
            set
            {
                strDatabase = value;
            }
        }

        public string Password
        {
            get
            {
                return strPassword;
            }
            set
            {
                strPassword = value;
            }
        }

        public int Port
        {
            get
            {
                return Convert.ToInt32(strPort);
            }
            set
            {
                strPort = value.ToString();
            }
        }

        public string Server
        {
            get
            {
                return strServer;
            }
            set
            {
                strServer = value;
            }
        }

        public string User
        {
            get
            {
                return strUser;
            }
            set
            {
                strUser = value;
            }
        }
    }
}