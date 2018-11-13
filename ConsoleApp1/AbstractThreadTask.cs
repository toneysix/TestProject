using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Пространство имён бизнес интерфейса
/// </summary>
namespace Application.BusinessInterface
{
    /// <summary>
    /// Абстрактный класс, описывающий выполнение задачи в новом потоке с общим связанным членом, через который происходит взаимодействие между другими потоками
    /// </summary>
    /// <typeparam name="T">Тип связанного члена</typeparam>
    public abstract class AbstractThreadTask<T>
    {
        /// <summary>Дескриптор потока</summary>
        private Thread thread;
        /// <summary>Ссылка на связанный член класса, через который происходит взаимодействие между другими задачами</summary>
        protected T tasks;
        /// <summary>Флаг, сигнализирующий о мануальном запросе на остановку задачного потока</summary>
        protected bool stopSignal;

        /// <summary>Процедура, выполняющаяся в новом поток и решающая поставленную задачу</summary>
        /// <param name="_params">Передаваемые параметры</param>
        protected abstract void backgroundWorker(object _params);

        /// <summary>Метод, ожидающий завершение задачного потока</summary>
        public void waitForTask()
        {
            thread.Join();
        }

        /// <summary>Метод, запускающий задачный поток</summary>
        /// <returns>Возвращает true, если задачный поток был успешно запущен и false, если поток находится в состоянии работы</returns>
        public bool start()
        {
            if (thread != null && thread.IsAlive)
                return false;
            stopSignal = false;
            thread = new Thread(backgroundWorker);
            thread.Start();
            return true;
        }

        /// <summary>Метод, запрашивающий остановку задачного потока</summary>
        /// <remarks>Метод не гарантирует незамедлительной остановки потока и зависит от реализации поточной процедуры backgroundWorker в конкретной задачи</remarks>
        public void requestStop()
        {
            stopSignal = true;
        }
    }
}
