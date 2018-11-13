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
