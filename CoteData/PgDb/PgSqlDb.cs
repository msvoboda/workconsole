using System;
using System.Data;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Web;
using System.Timers;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;
using Log;
using ServerData;

// Michal Svoboda 2011              //
// PgSQL Spravce pripojeni          //
// PgSQL Pool - navrhovy vzor fond  //
// - zakazan reditelem caryfukem    //
// http://cs.wikipedia.org/wiki/Object_pool
/// Sql
namespace PgSql
{
    public delegate void PgNotifyEvent(object sender, string notify);

    public class PgConnectionException : Exception
    {
        private IConnectionConfig _config;

        public PgConnectionException(string msg, IConnectionConfig config)
            : base(msg)
        {
            _config = config;
        }

        public IConnectionConfig Configuration
        {
            get { return _config; }
        }
    }

    public class PgSqlDataException : Exception
    {
    }
    
    public class PgSqlManager
    {
        private ICmConfiguration _cmconfig = null;
        private int _max = 0;
        ILogger _log;

        private Dictionary<string, PgSectionConnections> _connectionSections = new Dictionary<string, PgSectionConnections>();
        private Dictionary<string, PgSqlData> _notifyConnections = new Dictionary<string, PgSqlData>();

        public PgSqlManager(ICmConfiguration cmconfig, ILogger log, int max_section_conn)
        {
            _cmconfig = cmconfig;
            _max = max_section_conn;
            _log = log;
        }

        /// <summary>
        /// Vrati spojeni na databazi podle zadane PgSection
        /// </summary>
        /// <param name="pgsection">nazev pgsection</param>
        /// <param name="open"></param>
        /// <returns></returns>
        public PgSqlData OpenConnection(string pgsection, bool open=true)
        {
            IConnectionConfig config = _cmconfig.GetConfiguration(pgsection);
            if (config != null)
            {
                if (_connectionSections.ContainsKey(pgsection) == true)
                {
                    PgSectionConnections sec = _connectionSections[pgsection];
                    return sec.OpenConnection(open);
                }
                else
                {
                    PgSectionConnections new_pg = new PgSectionConnections(pgsection, config, _max);
                    _connectionSections.Add(pgsection, new_pg);
                    return new_pg.OpenConnection(open);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Vrati připojení zpět
        /// </summary>
        public void CloseConnection(PgSqlData pgdata)
        {
            string pgsection = pgdata.ConnectionConfiguration.Name;
            _connectionSections[pgsection].CloseConnection(pgdata);
        }

        public void SubscribeNotify(string pgsection, NotificationEventHandler notify)
        {
            IConnectionConfig config = _cmconfig.GetConfiguration(pgsection);
            if (config != null)
            {
                if (_notifyConnections.ContainsKey(pgsection) == true)
                {
                    PgSqlData pg_d = _notifyConnections[pgsection];
                    pg_d.OnPgNotify += notify;                       
                }
                else
                {
                    PgSqlData pg_d = new PgSqlData(_log);
                    pg_d.setConnection(config,true, true);
                    if (pg_d.OpenConnection() == false)
                    {
                        
                    }
                    pg_d.OnPgNotify += notify;
                    _notifyConnections.Add(pgsection, pg_d);
                }
            }
        }

        public void UnsubscribeNotify()
        {

        }
    }

    
    public class PgSectionConnections
    {
        IConnectionConfig _config = null;
        private Queue<PgSqlData> _connections = new Queue<PgSqlData>();
        private readonly string _name = "";
        private int _max = 0;
        
        public PgSectionConnections(string name, IConnectionConfig config, int max_connections)
        {
            _config = config;
            _name = name;
            _max = max_connections;

            for (int i = 0; i < _max; i++)
            {
                PgSqlData new_connection = new PgSqlData(_log);
                if (i == 0)
                {
                    if (new_connection.setConnection(config, true) == false)
                    {
                       throw new PgConnectionException("Connection failed!", config);
                    }
                    else
                    {
                        new_connection.setConnection(config, false);
                    }
                }
                _connections.Enqueue(new_connection);
             
            }
        }

        private ILogger _log;
        public ILogger Log
        {
            get { return _log; }
            set { _log = value; }
        }

        public PgSqlData OpenConnection(bool open)
        {
            try
            {
                PgSqlData data = _connections.Dequeue();
                if (open == true)
                {
                    if (data.OpenConnection() == true)
                    {
                        return data;
                    }
                }
                else
                {
                    return data;
                }
            }
            catch(InvalidOperationException oe)
            {
                PgSqlData new_connect = new PgSqlData(_log);
                new_connect.setConnection(_config, true);                
                return new_connect;
            }

            return null;
        }

        public void CloseConnection(PgSqlData data)
        {
            _connections.Enqueue(data);
        }

        public string Name
        {
            get { return _name; }
        }

    }

   
    public class PgSqlData
    {
        IConnectionConfig _config = null;
        protected NpgsqlConnection m_connection = new NpgsqlConnection();
        protected NpgsqlCommand m_command = new NpgsqlCommand();

        ILogger _log;

        public PgSqlData(ILogger log)
        {
            m_connection.Notification += new NotificationEventHandler(PqNotification);
            _log = log;
            /*
            if (timer == true)
            {
                m_notiftimer = new PgSqlNotifyTimer();
            }*/
        }

        public PgSqlData(PgSqlData data)
        {
            //Nemam WriteLogInfo !!!
            _config = data.ConnectionConfiguration;
            m_connection.ConnectionString = data.ConnectionString;
            m_connection.Notification += new NotificationEventHandler(PqNotification);
            //m_notiftimer = new PgSqlNotifyTimer();          

            m_command.Connection = m_connection;
            bool ret = CheckConnection();

            //Log("Clone connetion - test spojeni:" + ret.ToString(), XType.ERROR, MethodInfo.GetCurrentMethod().Name);
            _log.Info("Clone connetion - test spojeni:" + ret.ToString());
            //LogReport.Info("Clone connetion - test spojeni:" + ret.ToString(), MethodInfo.GetCurrentMethod());
        }

        public IConnectionConfig ConnectionConfiguration
        {
            get
            {
                return _config;
            }
        }

        /*
        public void Log(string text, ServerLog.XType type, string source="", string location="", string reason="")
        {
            if (logFce != null)
                logFce(text, type, source, location, reason);
        }*/

        static string _last = "";
        void PqNotification(object sender, NpgsqlNotificationEventArgs e)
        {            
            if (_last == e.AdditionalInformation)
                return;
            
            //LogReport.Info(e.PID + "; " + e.Condition + "; " + e.AdditionalInformation, MethodInfo.GetCurrentMethod());
            _last = e.AdditionalInformation;
            if (OnPgNotify != null)
            {
                OnPgNotify(sender, e);
            }
        }

        public NotificationEventHandler OnPgNotify = null;

        public bool setConnection(IConnectionConfig config, bool test, bool notify_conn=false)
        {
            _config = config;
            return setConnection(config.Server, config.Database, config.Port.ToString(), config.User, config.Password, test, notify_conn);
        }


        /// <summary>
        /// Nastaveni pripojeni na server
        /// </summary>
        /// <param name="server"></param>
        /// <param name="db"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        private bool setConnection(string server, string db, string port,string user, string pass, bool test, bool notify_con=false)
        {
            string con = String.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};CommandTimeout=600", server, port, user, pass, db);
            if (notify_con == true)
            {
                con += ";SyncNotification";
            }
            m_connection.ConnectionString = con;
            m_command.Connection = m_connection;

            if (test == true)
            {
                bool ret = CheckConnection();
                if (ret == false)
                {
                    _log.Error("Test spojeni:" + ret.ToString());
                    //Log("Test spojeni:" + ret.ToString(), XType.ERROR, MethodInfo.GetCurrentMethod().Name);
                }
                return ret;
            }
            else
            {
                return true;
            }
        }

        public int ExecuteQuery(string dotaz, List<NpgsqlParameter> pars=null)
        {
            m_command.CommandText = dotaz;
            m_command.Parameters.Clear();
            if (pars != null)
            {
                m_command.Parameters.AddRange(pars.ToArray());
            }            
            return m_command.ExecuteNonQuery(); 
        }

        public NpgsqlDataReader ExecuteSelectQuery(string dotaz, List<NpgsqlParameter> pars = null)
        {
            m_command.CommandText = dotaz;
            m_command.Parameters.Clear();
            
            if (pars != null)
            {
                m_command.Parameters.AddRange(pars.ToArray());
            }
            
            return m_command.ExecuteReader();
        }

        public DataSet ExecuteDatasetQuery(string dotaz, List<NpgsqlParameter> pars=null)
        {

            try
            {                
                NpgsqlDataAdapter sql_ad = new NpgsqlDataAdapter(m_command);
                DataSet data_set = new DataSet();
                m_command.CommandText = dotaz;
                m_command.Parameters.Clear();
                if (pars != null)
                {
                    m_command.Parameters.AddRange(pars.ToArray());
                }
                sql_ad.Fill(data_set);

                return data_set;
            }
            catch (Exception e)
            {
            }

            return null;
        }

        /// <summary>
        /// Otestuje databazove spojeni
        /// </summary>
        /// <returns></returns>
        public bool CheckConnection()
        {
            try
            {
                //LogReport.Error("CheckConnection" + m_conectString);
                m_connection.Open();
                m_connection.Close();
                //m_connection.ClearPool();
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
                // Log(e.Message, XType.ERROR);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Otevre databazove spojeni
        /// </summary>
        /// <returns></returns>
        public bool OpenConnection()
        {
            try
            {
                if (m_connection.State != ConnectionState.Open)
                {
                    m_connection.Open();
                }
                else
                {
                    _log.Error("PgSqlData DB Connection is already open!!!");
                    //Log("PgSqlData DB Connection is already open!!!",XType.ERROR);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Uzavre databazove spojeni
        /// </summary>
        /// <returns></returns>
        public bool CloseConnection()
        {
            try
            {
                if (m_connection.State != ConnectionState.Closed)
                {
                    m_connection.Close();
                    //m_connection.ClearPool();
                }

                //m_notiftimer.Stop();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public string ConnectionString
        {
            get { return m_connection.ConnectionString; }
        }
    }

    /*
    public class PgSqlNotifyTimer : PgSqlData
    {
        private Timer m_Timer = new Timer();        
        private bool m_start = false;

        public PgSqlNotifyTimer()
        :base((WriteLogLineInfo)null)
        {            
            m_Timer.Interval = 350;
            m_Timer.Elapsed += new ElapsedEventHandler(m_Timer_Elapsed);            
        }

        public void StartListenNotify(List<string> stations)
        {
            OpenConnection();
            foreach (string s in stations)
            {
                m_command.CommandText = "LISTEN \"DBI:" + s + "\"";
                m_command.ExecuteNonQuery();
            }
            Start();

        }

        void m_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (m_start == true)
            {
                try
                {

                    //ExecuteDirectQuery(";");
                }
                catch (Exception ex)
                {
                    _log.
                    //Log(ex.Message, XType.ERROR, MethodBase.GetCurrentMethod().Name);                    
                    //Stop();
                }
            }
        }

        public void Start()
        {
            m_start = true;
            m_Timer.Start();
        }

        public void Stop()
        {
            m_start = false;
            m_Timer.Stop();
        }
    
    }
    */

    /// <summary>
    /// Tovarni trida - utiliti na praci z databazi
    /// </summary>
    public static class PgDataToolFactory
    {
        public static DateTime ParseEventToTime(string payload)
        {
            DateTime eventTime = new DateTime();
            string[] pars = payload.Split(new Char[] { '|' });
            foreach (string p in pars)
            {
                int pidx = p.IndexOf("begin:");
                if (pidx != -1)                
                {

                    string dt = p.Substring(pidx+6);
                    eventTime = DateTime.Parse(dt);
                }
            }

            return eventTime;
        }
    }

}