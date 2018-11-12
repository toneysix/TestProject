using System;
using System.Collections.Generic;
using Application.Model;
using Application.DB;

/// <summary>
/// Пространство имён бизнес логики
/// </summary>
namespace Application.BusinessLogic
{
    /// <summary>
    /// Класс, реализующий задачу
    /// </summary>
    public class MyTask
    {
        /// <summary>
        /// Структура, хранящая информацию о файле (полный путь, хеш-сумму)
        /// </summary>
        public struct FileHashSumInfo
        {
            /// <summary>Полный путь к файлу</summary>
            public string Path;
            /// <summary>Хеш-сумма файла</summary>
            public string Hash;
        }

        /// <summary>Список обработанных файлов и поставленных в поток на добавление в БД</summary>
        public List<FileHashSumInfo> FileHashSumInfoSet = new List<FileHashSumInfo>();

        /// <summary>Конструктор класса</summary>
        /// <param name="path">Полный путь к каталогу для обработки файлов и записи в БД</param>
        /// <param name="dbConnData">Сведения о соединении с БД (кортеж: host, port, sid, user, password)</param>
        public MyTask(string path, Tuple<string, int, string, string, string> dbConnData)
        {         
            int currentTick = System.Environment.TickCount;
            Console.WriteLine("Запуск модуля сохранения результатов в БД");
            DBWriter dbWriter;
            try
            {
                OracleDatabase dbHandle = new OracleDatabase(dbConnData.Item1, dbConnData.Item2, dbConnData.Item3, dbConnData.Item4, dbConnData.Item5);
                dbWriter = new DBWriter(dbHandle, FileHashSumInfoSet);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка запуска модуля сохранения результатов в БД");
                throw new Exception(ex.Message);
            }
            
            Console.WriteLine("Запуск модуля получения хеш-сумм");
            HashSumFileCollector hashSumFileCollector = new HashSumFileCollector(path, FileHashSumInfoSet);

            hashSumFileCollector.waitForTask();
            Console.WriteLine("Все файлы и папки обработаны, всего файлов обработано: {0}, ожидаем завершения работы с БД", hashSumFileCollector.ProcessedFilesCount);

            dbWriter.requestStop();
            dbWriter.waitForTask();
            Console.WriteLine("Работа с БД закончена, вставлено строк {0}", dbWriter.InsertedFilesCount);
            Console.WriteLine("Время выполнения задания (мс): {0}", System.Environment.TickCount - currentTick);
        }
    }
}
