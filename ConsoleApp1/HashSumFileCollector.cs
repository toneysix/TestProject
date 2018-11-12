using Application.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using Application.BusinessInterface;

/// <summary>
/// Пространство имён бизнес логики
/// </summary>
namespace Application.BusinessLogic
{
    /// <summary>
    /// Класс, производящий расчёт хеш-сумм файлов во всех директориях указанной директории
    /// </summary>
    public class HashSumFileCollector : AbstractThreadTask<List<MyTask.FileHashSumInfo>>
    {
        #region Поля класса

        /// <summary>Указывает какое количество файлов должно быть обработано для передачи результатов их обработки в БД на запись</summary>
        private uint portionProcFile;
        /// <summary>Путь к директории для обработки файлов</summary>
        private string path;
        /// <summary>Количество обработанных файлов</summary>
        public int ProcessedFilesCount = 0;

        #endregion

        #region Инициализация/Реализация

        /// <summary>Конструктор класса</summary>
        /// <param name="path">Полный путь к директории</param>
        /// <param name="fileHashSumInfoSet">Ссылка на список, в который будут помещаться данные об обработанных файлах</param>
        /// <param name="portionProcFile">Необязательный параметр, указывающий какое количество файлов должно быть обработано для передачи результатов их обработки в БД на запись</param>
        public HashSumFileCollector(string path, List<MyTask.FileHashSumInfo> fileHashSumInfoSet, uint portionProcFile = 10)
        {         
            if (Directory.Exists(path) || File.Exists(path))
            {
                this.path = path;   
                this.portionProcFile = portionProcFile;
                tasks = fileHashSumInfoSet;
                start();
            }
            else
                throw new Exception("Файл или директория не существует");
        }

        #endregion

        #region Методы, реализующие функционал класса

        /// <summary>Метод, осуществляющий передачу списка обработанных файлов на запись в БД</summary>
        /// <param name="from">Список обработанных файлов для передачи на запись в БД</param>
        /// <remarks>Предполагается, что передача в БД списка осуществляется через общую сущность</remarks>
        private void writeIntoDb(List<MyTask.FileHashSumInfo> from)
        {
            if (from.Count == 0)
                return;

            lock (tasks)
            {
                foreach (MyTask.FileHashSumInfo fileHashSumInfo in from)
                {
                    tasks.Add(fileHashSumInfo);
                }
            }
        }

        /// <summary>Процедура, обрабатывающая файлы из указанной директории в новом потоке</summary>
        /// <param name="_params">Передаваемые параметры</param>
        protected override void backgroundWorker(object _params)
        {
            MD5Hash md5Hash = new MD5Hash();
            string hash;
            List<MyTask.FileHashSumInfo> tempFileHashSumInfoSet = new List<MyTask.FileHashSumInfo>();

            try
            {
                DirectoryHelper dirHelper = new DirectoryHelper(path);
                foreach(string[] files in dirHelper)
                {
                    foreach(string file in files)
                    {
                        hash = md5Hash.fileHashSum(file).toString();
                        tempFileHashSumInfoSet.Add(new MyTask.FileHashSumInfo() { Path = file, Hash = hash });
                        ProcessedFilesCount++;

                        if (portionProcFile == tempFileHashSumInfoSet.Count)
                        {
                            writeIntoDb(tempFileHashSumInfoSet);
                            tempFileHashSumInfoSet.Clear();
                        }
                    }
                }     
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            writeIntoDb(tempFileHashSumInfoSet);
        }

        #endregion
    }
}
