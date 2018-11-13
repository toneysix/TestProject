using System;
using System.Collections.Generic;
using System.Threading;
using Application.BusinessInterface;
using Application.BusinessLogic;
using Application.DB;
using System.Linq;

/// <summary>
/// Пространство имён модели
/// </summary>
namespace Application.Model
{
    /// <summary>
    /// Класс, реализующий запись в БД обработанных файлов
    /// </summary>
    public class DBTaskWriter : AbstractThreadTask<List<MyTask.FileHashSumInfo>>
    {
        #region Поля класса

        /// <summary>Количество вставленных (обновленных строк)</summary>
        public uint InsertedFilesCount = 0;
        /// <summary>Дескриптор БД</summary>
        private OracleDatabase dbHandle;

        #endregion

        #region Иниацилазция/Реализация

        /// <summary>Конструктор класса</summary>
        /// <param name="db">Дескриптор базы данных</param>
        /// <param name="fileHashSumInfoSet">Ссылка на список, откуда будут вставляться (обновляться) кортежи в БД</param>
        public DBTaskWriter(OracleDatabase db, List<MyTask.FileHashSumInfo> fileHashSumInfoSet)
        {            
            dbHandle = db;
            tasks = fileHashSumInfoSet;
            initDB();
        }

        /// <summary>Метод, создающий отношение в БД, куда будут вставляться кортежи обработанных файлов</summary>
        private void initDB()
        {
            // Создание таблицы с информацией по файлам
            int resultCode = dbHandle.executeNonQuery("create table FileInfo (FullPath VARCHAR2(260) PRIMARY KEY NOT NULL, MD5Hash VARCHAR2(100) NOT NULL)");
            if (resultCode != 0 && resultCode != 955)
                throw new Exception(String.Format("Ошибка при иницилазиации базы данных: невозможно создать таблицу, код ошибки ORA-{0}", resultCode));

            // Создание таблицы с логами
            resultCode = dbHandle.executeNonQuery("create table Logs (LogDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL, Message VARCHAR2(1024))");
            if (resultCode != 0 && resultCode != 955)
                throw new Exception(String.Format("Ошибка при иницилазиации базы данных: невозможно создать таблицу, код ошибки ORA-{0}", resultCode));

        }

        #endregion

        #region Методы, реализующие функционал класса

        /// <summary>Метод, логирующий в БД</summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Возвращает true в случае успешной вставки кортежа</returns>
        public bool writeLogInfo(string message)
        {
            int resultCode = dbHandle.simpleInsert("Logs", new Dictionary<string, object>() { { "Message", message } });
            if (resultCode == 0)
                return true;

            Console.WriteLine("Ошибки при вставки строки в таблицу логов ORA-{0}", resultCode);
            return false;
        }

        /// <summary>Метод, осуществляющий вставку (обновление) строки в БД</summary>
        /// <param name="fileHashSumInfo">Структура, специфицирующая результат обработки файла для вставки (обновления) в БД</param>
        /// <returns>Возвращает true в случае успешной вставки (обновления) кортежа</returns>
        public bool writeFileInfo(MyTask.FileHashSumInfo fileHashSumInfo)
        {
            int resultCode = dbHandle.simpleInsertOrUpdate("FileInfo", "FullPath", new Dictionary<string, object>() { { "FullPath", fileHashSumInfo.Path },
            { "MD5Hash", fileHashSumInfo.Hash } });
            if (resultCode == 0)
            {
                InsertedFilesCount++;
                return true;
            }

            Console.WriteLine("Ошибки при вставке/обновлении строки в БД, код ошибки ORA-{0}", resultCode);
            return false;
        }

        /// <summary>Процедура, получающая задания на добавление в отношение БД кортежей результатов обработанных файлов</summary>
        /// <param name="_params">Передаваемые параметры</param>
        protected override void backgroundWorker(object _params)
        {
            int taskCount = 0;
            while ((taskCount = tasks.Count) != 0 || !stopSignal)
            {
                if(taskCount > 0)
                {
                    List<MyTask.FileHashSumInfo> tempFileHashSumInfoSet;
                    lock (tasks)
                    {
                        tempFileHashSumInfoSet = new List<MyTask.FileHashSumInfo>(tasks.AsReadOnly());
                        tasks.Clear();
                    }

                    foreach(MyTask.FileHashSumInfo fileHashSumInfo in tempFileHashSumInfoSet)
                        writeFileInfo(fileHashSumInfo);
                }

                Thread.Sleep(150);
            }
        }

        #endregion
    }
}
