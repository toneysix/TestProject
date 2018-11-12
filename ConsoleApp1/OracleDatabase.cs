using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;

/// <summary>
/// Пространство имён работы с БД
/// </summary>
namespace Application.DB
{
    /// <summary>
    /// Класс по работе с Oracle DB
    /// </summary>
    public class OracleDatabase
    {
        #region Поля класса

        /// <summary>Дескриптор соединения БД</summary>
        private OracleConnection connection;

        #endregion

        #region Инициализация/Реализация

        /// <summary>Конструктор класса</summary>
        /// <param name="host">Имя хоста, на котором запущена БД</param>
        /// <param name="port">Порт, на котором слушается БД</param>
        /// <param name="sid">SID идентификатор БД</param>
        /// <param name="user">Пользователь под которым будет осуществляться новая сессия в БД</param>
        /// <param name="password">Пароль пользователя</param>
        public OracleDatabase(string host, int port, string sid, string user, string password)
        {
            Console.WriteLine("Устанавливаем соединение с БД");

            string connString = String.Format("Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1})) (CONNECT_DATA = (SERVER = DEDICATED)(SID = {2})));Password={3};User ID={4}", 
                host, port, sid, user, password);

            connection = new OracleConnection();
            connection.ConnectionString = connString;
            connection.Open();

            Console.WriteLine("Соединение с базой данной установлено");
        }

        /// <summary>Деструктор класса, осуществляющий закрытие сессии в БД и высвобождающий неуправляемые ресурсов</summary>
        ~OracleDatabase()
        {
            connection.Close();
            connection.Dispose();
        }

        #endregion

        #region Методы, реализующие фукцию класса

        /// <summary>Потокобезопасный метод, производящий генерацию запроса и вставку кортежа в БД, в случае, если он дублируется по уникальному ключу, производится его обновление</summary>
        /// <param name="tableName">Имя отношения (таблицы), в которую требуется вставить кортеж</param>
        /// <param name="whereField">Наименование атрибута, однозначно идентифицирующее кортеж для обновления в случае совпадения</param>
        /// <param name="_params">Ассоциативный массив, включающий имена атрибутов и их значений для вставки или обновления</param>
        /// <returns>Возвращает код ошибки выполненного запроса Oracle, 0 - запрос успешно выполнен</returns>
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

        /// <summary>Потокобезопасный метод, выполняющий запрос в БД</summary>
        /// <param name="cmdText">SQL запрос</param>
        /// <returns>Возвращает код ошибки выполненного запроса Oracle, 0 - запрос успешно выполнен</returns>
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

        #endregion
    }
}
