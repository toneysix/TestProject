using System.Collections.Generic;
using System.Threading;

namespace Application.BusinessInterface
{
    public abstract class AbstractThreadTask<T>
    {
        private Thread thread;
        protected List<T> tasks;
        protected bool stopSignal;

        protected abstract void backgroundWorker(object _params);

        public virtual void waitForTask()
        {
            thread.Join();
        }

        protected virtual bool start()
        {
            if (thread != null && thread.IsAlive)
                return false;
            stopSignal = false;
            thread = new Thread(backgroundWorker);
            thread.Start();
            return true;
        }

        public virtual void requestStop()
        {
            stopSignal = true;
        }
    }
}
