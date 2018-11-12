using Application.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using Application.BusinessInterface;

namespace Application.BusinessLogic
{
    public class HashSumFileCollector : AbstractThreadTask<MyTask.FileHashSumInfo>
    {
        private uint portionProcFile;
        private string path;
        public int ProcessedFilesCount = 0;

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
    }
}
