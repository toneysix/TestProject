using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace Application.DB
{
    public class OracleDatabase
    {
        private OracleConnection connection;

        public OracleDatabase(string host, int port, string sid, string user, string password)
        {
            Console.WriteLine("Устанавливаем соединение с БД");

            string connString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = "
                 + host + ")(PORT = " + port + "))(CONNECT_DATA = (SERVER = DEDICATED)(SID = "
                 + sid + ")));Password=" + password + ";User ID=" + user;

            connection = new OracleConnection();
            connection.ConnectionString = connString;
            connection.Open();

            Console.WriteLine("Соединение с базой данной установлено");
        }

        ~OracleDatabase()
        {
            connection.Close();
            connection.Dispose();
        }

        public int simpleInsertOrUpdate(string tableName, string whereField, Dictionary<string, object> _params)
        {
            lock (connection)
            {
                StringBuilder cmdText = new StringBuilder();
                cmdText.AppendFormat("INSERT INTO {0} ({1}) VALUES (:{2})", tableName, String.Join(",", _params.Keys.ToArray<string>()), 
                    String.Join(",:", _params.Keys.ToArray<string>()));

                using (OracleCommand cmd = new OracleCommand(cmdText.ToString(), connection))
                {
                    foreach (KeyValuePair<string, object> p in _params)
                        cmd.Parameters.Add(p.Key, p.Value);

                    try
                    {
                        cmd.ExecuteNonQuery();           
                    }
                    catch(OracleException ex1)
                    {
                        if(ex1.Number == 1)
                        {
                            cmdText.Clear();
                            cmdText.AppendFormat("UPDATE {0} SET ", tableName);
                            foreach(KeyValuePair<string, object> p in _params)
                            {
                                if(p.Key.CompareTo(whereField) != 0)
                                    cmdText.AppendFormat("{0} = :{0},", p.Key);
                            }

                            cmdText.Remove(cmdText.Length - 1, 1);
                            cmdText.AppendFormat(" WHERE {0} = :{0}", whereField);
                            cmd.CommandText = cmdText.ToString();
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch(OracleException ex2)
                            {
                                return ex2.Number;
                            }

                            return 0;
                        }
                        return ex1.Number;
                    }
                }
            }
            return 0;
        }

        public int executeNonQuery(string cmdText)
        {
            lock (connection)
            {
                using (OracleCommand cmd = new OracleCommand(cmdText, connection))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch(OracleException ex)
                    {
                        return ex.Number;
                    }
                }
            }
            return 0;
        }
    }
}
