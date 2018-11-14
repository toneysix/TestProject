using System;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Пространство имён вспомогательных классов и методов
/// </summary>
namespace Application.Utils
{
    /// <summary>
    /// Класс по работе с директориями, реализуйющий перебор всех файлов в указанном каталоге, включая подкаталоги
    /// </summary>
    public class DirectoryHelper : IEnumerable, IEnumerator
    {
        #region Поля класса

        /// <summary>Текущий индекс перебираемого каталога</summary>
        private int currentFolderIndex;
        /// <summary>Текущие файлы, извлеченные из текущего перебираемого каталога/summary>
        private List<string> currentFiles;
        /// <summary>Список директорий для перебора файлов</summary>
        private List<string> currentFolders = new List<string>();

        #endregion

        #region Инициализация, реализация

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="folderPath">Полный путь к папке для выборки всех файлов</param>
        public DirectoryHelper(string folderPath)
        {
            if(Directory.Exists(folderPath))
            {
                currentFolders.Add(folderPath);
                currentFolderIndex = 0;
                currentFiles = new List<string>();
                return;
            }

            throw new Exception("Указанная директория не существует");
        }

        #endregion

        #region Методы, реализующие функционал класса (IEnumerable, IEnumerator)

        /// <summary>
        /// Получает перечислитель (реализация метода IEnumerable)
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return this;
        }

        /// <summary>
        /// Сдвигает итерацию перечислителя на следующуий каталог (реализация метода IEnumerator)
        /// </summary>
        public bool MoveNext()
        {
            currentFiles.Clear();
     
            if (currentFolders.Count <= currentFolderIndex)
            {          
                Reset();
                return false;
            }

            currentFiles.AddRange(Directory.GetFiles(currentFolders[currentFolderIndex]));   
            currentFolders.AddRange(Directory.GetDirectories(currentFolders[currentFolderIndex]));
            currentFolderIndex++;
            if (currentFiles.Count == 0)
                MoveNext();

            return true;
        }

        /// <summary>
        /// Сбрасывает итерацию файлов каталогов (реализация метода IEnumerator)
        /// </summary>
        public void Reset()
        {
            currentFolders.RemoveRange(1, currentFolders.Count - 1);
            currentFolderIndex = 0;
            currentFiles.Clear();
        }

        /// <summary>
        /// Получает список файлов текущего перебираемого каталога (реализация метода IEnumerator)
        /// </summary>
        public object Current
        {
            get
            {
                return currentFiles.ToArray();
            }
        }

        #endregion
    }

    /// <summary>
    /// Класс, реализующий получение хеш-сумм (с файлов)
    /// </summary>
    public class MD5Hash
    {
        #region Поля класса

        /// <summary>Экземпляр класса MD5 криптографической библиотеки .NET с хеш-алгоритмами</summary>
        private MD5 _MD5;

        #endregion

        #region Вложенные классы и структуры

        /// <summary>
        /// Структура, хранящая информацию о MD5 хеш-сумме
        /// </summary>
        public struct MD5HashSum
        {
            /// <summary>
            /// Хеш-сумма
            /// </summary>
            public byte[] HashSum { get; private set; }

            /// <summary>
            /// Конструктор структуры
            /// </summary>
            /// <param name="hash">Хеш-сумма</param>
            public MD5HashSum(byte[] hash)
            {
                HashSum = hash;
            }

            /// <summary>
            /// Преобразует байтовый массив хеш-суммы в строку с символами верхнего регистра
            /// </summary>
            public string toString()
            {
                return BitConverter.ToString(HashSum).Replace("-", "").ToUpperInvariant();
            }
        }

        #endregion

        #region Инициализация, реализация

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public MD5Hash()
        {
            _MD5 = MD5.Create();
        }

        #endregion

        #region Методы, реализующие функционал класса

        /// <summary>
        /// Метод, реализующий вычисление MD5 хеш-суммы указанного файла
        /// </summary>
        /// <param name="filename">Полный путь к файлу</param>
        /// <returns>Возвращает байтовую последовательность хеш-суммы</returns>   
        public MD5HashSum fileHashSum(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                return new MD5HashSum(_MD5.ComputeHash(stream));
            }
        }

        #endregion
    }
}
