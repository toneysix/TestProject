using System;
using System.Collections.Generic;
using System.IO;
using Application.Model;
using Application.DB;
using Application.Utils;

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
        #region Поля класса

        /// <summary>Путь к директории для обработки файлов</summary>
        private readonly string path;
        /// <summary>Список обработанных файлов и поставленных в поток на добавление в БД</summary>
        private List<FileHashSumInfo> FileHashSumInfoSet = new List<FileHashSumInfo>();
        /// <summary>Дескриптор БД</summary>
        private OracleDatabase dbHandle;
        /// <summary>Дескриптор модуля записи данных в БД</summary>
        private DBTaskWriter dbWwriterHandle;
        /// <summary>Дескриптор модуля обработки файлов (получение хеш-сумм)</summary>
        private HashSumFileCollector hashSumFileCollectorHandle;

        #endregion

        #region Вложенные структуры/классы

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

        #endregion

        #region Иницилазиация/Реализация

        /// <summary>Конструктор класса</summary>
        /// <param name="path">Полный путь к каталогу для обработки файлов и записи в БД</param>
        /// <param name="dbConnData">Сведения о соединении с БД (кортеж: host, port, sid, user, password)</param>
        public MyTask(string path, Tuple<string, int, string, string, string> dbConnData)
        {
            this.path = path;
            Console.WriteLine("Инициализация модуля сохранения результатов в БД");
            try
            {
                dbHandle = new OracleDatabase(dbConnData.Item1, dbConnData.Item2, dbConnData.Item3, dbConnData.Item4, dbConnData.Item5);
                dbWwriterHandle = new DBTaskWriter(dbHandle, FileHashSumInfoSet);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка инициализации модуля сохранения результатов в БД");
                dbHandle.Dispose();
                throw new Exception(ex.Message);
            }
            
            Console.WriteLine("Инициализация модуля получения хеш-сумм");
            hashSumFileCollectorHandle = new HashSumFileCollector(dbWwriterHandle, FileHashSumInfoSet);       
        }

        #endregion

        #region Методы, реализующие функцию класса

        /// <summary>Метод, запускающий выполнение задачи: получение хеш-сумм файлов и запись результатов в БД</summary>
        public void run()
        {
            int currentTick = System.Environment.TickCount;

            Console.WriteLine("Запуск модуля получения хеш-сумм");
            hashSumFileCollectorHandle.start();
            Console.WriteLine("Запуск модуля сохранения результатов в БД");
            dbWwriterHandle.start();

            try
            {
                DirectoryHelper dirHelper = new DirectoryHelper(path);

                foreach (string[] files in dirHelper)
                {
                    hashSumFileCollectorHandle.addFileToProcess(files);
                }

                hashSumFileCollectorHandle.requestStop();
                hashSumFileCollectorHandle.waitForTask();
                Console.WriteLine("Все файлы и папки обработаны, всего файлов обработано: {0}, ожидаем завершения работы с БД", hashSumFileCollectorHandle.ProcessedFilesCount);

                dbWwriterHandle.requestStop();
                dbWwriterHandle.waitForTask();
                
                Console.WriteLine("Работа с БД закончена, вставлено строк {0}", dbWwriterHandle.InsertedFilesCount);
                Console.WriteLine("Время выполнения задания (мс): {0}", System.Environment.TickCount - currentTick);
            }
            catch(Exception ex)
            {
                dbWwriterHandle.writeLogInfo(ex.Message);
                Console.WriteLine(ex.Message);
            }

            dbHandle.Dispose();
        }

        #endregion
    }
}
