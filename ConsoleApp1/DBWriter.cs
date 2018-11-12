using System;
using System.Collections.Generic;
using System.Threading;
using Application.BusinessInterface;
using Application.BusinessLogic;
using Application.DB;

namespace Application.Model
{
    public class DBWriter : AbstractThreadTask<MyTask.FileHashSumInfo>
    {      
        public uint InsertedFilesCount = 0;
        private OracleDatabase dbHandle;

        public DBWriter(OracleDatabase db, List<MyTask.FileHashSumInfo> fileHashSumInfoSet)
        {            
            dbHandle = db;
            tasks = fileHashSumInfoSet;
            initDB();
            start();
        }

        private void initDB()
        {
            int resultCode = dbHandle.executeNonQuery("create table FileInfo (FullPath VARCHAR2(260) PRIMARY KEY NOT NULL, MD5Hash VARCHAR2(100) NOT NULL)");
            if (resultCode != 0 && resultCode != 955)
                throw new Exception(String.Format("Ошибка при иницилазиации базы данных: невозможно создать таблицу, код ошибки ORA-{0}", resultCode));
        }

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

            stopSignal = false;
        }
    }
}
