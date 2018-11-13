using Application.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Application.BusinessInterface;
using Application.Model;

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

        /// <summary>Дескриптор БД</summary>
        private DBTaskWriter dBWriterHandle;
        /// <summary>Указывает какое количество файлов должно быть обработано для передачи результатов их обработки в БД на запись</summary>
        private readonly uint portionProcFile;
        /// <summary>Количество обработанных файлов</summary>
        public int ProcessedFilesCount = 0;
        /// <summary>Список файлов, для которых требуется произвести обработку</summary>
        private List<string> filesToProcess;

        #endregion

        #region Инициализация/Реализация

        /// <summary>Конструктор класса</summary>
        /// <param name="dBWriterHandle">Дескриптор БД для логирования ошибок</param>
        /// <param name="fileHashSumInfoSet">Ссылка на список, в который будут помещаться данные об обработанных файлах</param>
        /// <param name="portionProcFile">Необязательный параметр, указывающий какое количество файлов должно быть обработано для передачи результатов их обработки в БД на запись</param>
        public HashSumFileCollector(DBTaskWriter dBWriterHandle, List<MyTask.FileHashSumInfo> fileHashSumInfoSet, uint portionProcFile = 10)
        {
           this.dBWriterHandle = dBWriterHandle;
           this.portionProcFile = portionProcFile;
           filesToProcess = new List<string>();
           tasks = fileHashSumInfoSet;
        }

        #endregion

        #region Методы, реализующие функционал класса

        /// <summary>Метод, осуществляющий постановку на обработку файла</summary>
        /// <param name="filePath">Полный путь к файлу для его обработки</param>
        public void addFileToProcess(string filePath)
        {
            lock (filesToProcess)
            {
                filesToProcess.Add(filePath);
            }
        }

        /// <summary>Метод, осуществляющий постановку на обработку файлов</summary>
        /// <param name="filePath">Список файлов с полными путями для обработки</param>
        public void addFileToProcess(List<string> filesPath)
        {
            lock (filesToProcess)
            {
                filesToProcess.AddRange(filesPath);
            }
        }

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
            string hash = "";
            List<MyTask.FileHashSumInfo> tempFileHashSumInfoSet = new List<MyTask.FileHashSumInfo>();
            List<string> tempFilesToProcess = new List<string>();
            int filesCountToProcess = 0;

            while ((filesCountToProcess = filesToProcess.Count) > 0 || !stopSignal)
            {
                if(filesCountToProcess > 0)
                {
                    lock(filesToProcess)
                    {
                        tempFilesToProcess = new List<string>(filesToProcess.AsReadOnly());
                        filesToProcess.Clear();
                    }

                    foreach (string file in tempFilesToProcess)
                    {
                        try
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
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            dBWriterHandle.writeLogInfo(ex.Message);
                        }
                    }
                }          
                
                Thread.Sleep(150);
            }
 
            writeIntoDb(tempFileHashSumInfoSet);
        }

        #endregion
    }
}
