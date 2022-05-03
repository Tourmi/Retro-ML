using System;
using System.Collections.Generic;
using System.Threading;

namespace SMW_ML.Utils
{
    internal static class Exceptions
    {
        private static Queue<Exception> exceptions = new Queue<Exception>();
        private static Semaphore semaphore = new Semaphore(0, 1);

        public static void QueueException(Exception ex)
        {
            lock (exceptions) exceptions.Enqueue(ex);

            semaphore.Release();
        }

        public static Exception ConsumeException()
        {
            semaphore.WaitOne();

            lock (exceptions) return exceptions.Dequeue();
        }
    }
}
