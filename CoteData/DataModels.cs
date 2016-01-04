using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PgSql;
using Npgsql;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace ServerData
{

    /// <summary>
    /// RADIO API - snazime se o postgres indipendent data
    /// 
    /// </summary>
    public class RadioData
    {
        PgSqlManager _dbmanager;
        private string _section;

        // DATA
        Users _users = null;
        Stations _stations = null;
        Machines _machines = null;

        public RadioData(PgSqlManager data, string dbsection)
        {            
            _dbmanager = data;
            _section = dbsection;
        }

        PgSqlData _db=null;
        private PgSqlData OpenConnection()
        {
            if (_db == null)
            {
                _db = _dbmanager.OpenConnection(_section, false);
            }

            _db.OpenConnection();
            return _db;
        }

        private void CloseConnection()
        {
            if (_db == null)
                return;

            _db.CloseConnection();
        }

        /// <summary>
        /// Vraci uzivatele, ale password je vzdy null
        /// </summary>
        /// <returns></returns>        
        public Users GetUsers()
        {
            if (_users == null)
            {
                _users = new Users();
                string dotaz = "SELECT \"ID\", \"Login\", \"IsGuest\", \"IsSupervisor\", \"Name\",  \"Alias\", \"Email\", \"LastLogin\"  FROM \"Users\";";
                PgSqlData database = OpenConnection();
                NpgsqlDataReader reader = database.ExecuteSelectQuery(dotaz);
                try
                {
                    if (reader.HasRows == true)
                    {
                        while (reader.Read() == true)
                        {

                            User user_new = readUser(reader);
                            _users.Add(user_new);
                        }
                    }
                }
                finally
                {
                    if (reader.IsClosed == false)
                        reader.Close();

                    CloseConnection();
                }
            }
            return _users;
        }

        private User readUser(NpgsqlDataReader reader)
        {
            User user_new = new User();
            user_new.ID = (int)reader[0];
            try { user_new.Name = (string)reader[4]; }
            catch { }

            user_new.Login = (string)reader[1];
            if (reader.IsDBNull(5) == false)
                user_new.Alias = (string)reader["Alias"];
            if (reader.IsDBNull(6) == false)
                user_new.Email = (string)reader["Email"];

            user_new.IsGuest = (bool)reader["IsGuest"];
            user_new.IsSupervisor = (bool)reader["IsSupervisor"];
            if (reader.IsDBNull(7) == false)
                user_new.lastLogin = (DateTime)reader["LastLogin"];

            return user_new;
        }
       
        public Stations GetStations()
        {
            if (_stations == null)
            {
                _stations = new Stations();
                string dotaz = "SELECT \"ID\", \"Name\", \"IDStr\", \"GroupID\"  FROM \"Stations\"";
                PgSqlData database = OpenConnection();
                NpgsqlDataReader reader = database.ExecuteSelectQuery(dotaz);
                try
                {
                    if (reader.HasRows == true)
                    {
                        while (reader.Read() == true)
                        {

                            Station user_new = readStation(reader);
                            _stations.Add(user_new);
                        }
                    }
                }
                finally
                {
                    if (reader.IsClosed == false)
                        reader.Close();

                    CloseConnection();
                }
            }
            return _stations;
        }

        public Station readStation(NpgsqlDataReader reader)
        {
            Station st = new Station();


            st.ID = (int)reader[0];
            st.Name = (string)reader[1];
            st.ShortName = (string)reader[2];
            if (reader.IsDBNull(3)==false)
                st.GroupID = (int)reader[3];

            return st;
        }

        public Machines GetMachines()
        {
            if (_machines == null)
            {
                _machines = new Machines();
                string dotaz = "SELECT \"ID\", \"IDStr\", \"GroupID\"  FROM \"Machines\"";
                PgSqlData database = OpenConnection();
                NpgsqlDataReader reader = database.ExecuteSelectQuery(dotaz);
                try
                {
                    if (reader.HasRows == true)
                    {
                        while (reader.Read() == true)
                        {

                            Machine mr_new = readMachine(reader);
                            _machines.Add(mr_new);
                        }
                    }
                }
                finally
                {
                    if (reader.IsClosed == false)
                        reader.Close();

                    CloseConnection();
                }
            }
            return _machines;
        }

        public Machine readMachine(NpgsqlDataReader reader)
        {
            Machine m = new Machine();


            m.ID = (int)reader[0];            
            m.ShortName = (string)reader[1];
            if (reader.IsDBNull(2)==false)
                m.GroupID = (int)reader[2];

            return m;
        }

        public Options GetOptions(string app, int stationid=-1, int machineid=-1, int userid=-1)
        {
            Options opts = new Options();

            string query = "select \"IDStr\", \"Type\", \"Index\",  \"integer\", \"real\", \"boolean\", \"text\", \"interval\", \"timestamp\"  from \"Options\"('"+app+"');";
            PgSqlData database = OpenConnection();
            NpgsqlDataReader reader = database.ExecuteSelectQuery(query);
            try
            {
                if (reader.HasRows == true)
                {
                    while (reader.Read() == true)
                    {
                        try
                        {
//                          OptionValue opt = readOption(reader);
                            object opt = readOption(reader);
                            opts.Add(opt);
                        }
                        catch (Exception e)
                        {
                            int a = 0;
                        }
                    }
                }
            }
            catch
            {
            }          

            return opts;
        }

        public object readOption(NpgsqlDataReader reader)
        {            
            string name = (string)reader.GetString(0);
            object op = null;// new OptionValue(name);            
            string typ = (string)reader.GetString(1);
            if (typ == "text")
            {
                op = new StringValue(name);
                if (reader.IsDBNull(6) == false)
                {
                    ((StringValue)op).Value = reader.GetString(6);
                }
            }
            else if (typ == "integer")
            {
                op = new IntValue(name);
                if (reader.IsDBNull(3) == false)
                {
                    ((IntValue)op).Value = reader.GetInt32(3);
                }
            }
            else if (typ == "boolean")
            {
                op = new BoolValue(name);
                if (reader.IsDBNull(5) == false)
                    ((BoolValue)op).Value = reader.GetBoolean(5);
            }

            return op;
        }

        public string Login(string user, string pass)
        {
            string dotaz = "SELECT \"Login\", \"Password\" from \"Users\" where \"Login\"='" + user + "' \"Password\"=\"md5\"('" + pass + "')";
            PgSqlData database = OpenConnection();
            NpgsqlDataReader reader = database.ExecuteSelectQuery(dotaz);
            try
            {
            }
            catch
            {
            }
          
            return pass;
        }
        
    }

    [CollectionDataContract(Name = "Users")]
    public class Users : List<User>
    {
    }
        
    
    public class User
    {        
        public int ID { get; set; }

        private string m_login;        
        public string Login
        {
            get
            {
                return m_login;
            }
            set
            { m_login = value; }
        }
        //public string Password { set; }        
        public bool IsGuest { get; set; }        
        public bool IsSupervisor { get; set; }

        private string _name = "";        
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                Console.WriteLine(_name);                
            }
        }        
        public string Alias { get; set; }        
        public string Email { get; set; }        
        public DateTime lastLogin { get; set; }
        /*
        private string m_password;        
        public string Password
        {
            get { return m_password; }
            set
            { m_password = value; }
        } */     

        public override string ToString()
        {
            return Login + ", " + Name + ", " + Email;
        }               
    }

     [CollectionDataContract(Name = "Stations")]
    public class Stations : List<Station>
    {

    }

    public class Station
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int GroupID { get; set; }

        public override string ToString()
        {
            return ID + ", " + Name;
        }
    }

    [CollectionDataContract(Name = "Machines")]
    public class Machines : List<Machine>
    {

    }

    public class Machine
    {
        public int ID { get; set; }        
        public string ShortName { get; set; }
        public int GroupID { get; set; }
    }
    
    [CollectionDataContract(Name = "Options")]
    /*
    [KnownType(typeof(IntValue))]
    [KnownType(typeof(StringValue))]
    [KnownType(typeof(BoolValue))]
    [KnownType(typeof(FloatValue))]
    [KnownType(typeof(TimeValue))]
    [KnownType(typeof(DateValue))]*/
    public class Options : List<object>
    {

    }


    [DataContract]    
    public class IntValue
    {
        public IntValue()
        {
        }

        public IntValue(string name)
        {
            Name = name;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public new Int32 Value { get; set; }
    }

    [DataContract]
    public class BoolValue
    {
        public BoolValue()
        {
        }

        public BoolValue(string name)
        {
            Name = name;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public new bool Value { get; set; }
    }

    [DataContract]
    public class StringValue
    {
        public StringValue()
        {
        }

        public StringValue(string name)
        {
            Name = name;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public new String Value { get; set; }
    }

    [DataContract]
    public class TimeValue
    {
        public TimeValue()
        {
        }

        public TimeValue(string name)
        {
            Name = name;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public new TimeSpan Value { get; set; }
    }

    [DataContract]
    public class DateValue
    {
        public DateValue()
        {

        }

        public DateValue(string name)
        {
            Name = name;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public new DateTime Value { get; set; }
    }

    [DataContract]
    public class FloatValue
    {
        public FloatValue()
        {
        }

        public FloatValue(string name)
        {
            Name = name;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public float Value { get; set; }
    }


    [DataContract]
    //[KnownType(typeof(Timw))]
    //[KnownType(typeof(DateValue))]
    public class OptionValue
    {
        public OptionValue(string name)
        {
            Name = name;        
        }

        public OptionValue()
        {
            
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public object Value { get; set; }
    }
}
