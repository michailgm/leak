﻿using System;
using System.Threading;

namespace Leak.Completion
{
    public class CompletionThread : CompletionWorker, IDisposable
    {
        private Thread thread;
        private bool completed;
        private IntPtr port;

        public void Start()
        {
            port = CompletionInterop.CreateIoCompletionPort(new IntPtr(-1), IntPtr.Zero, 0, 0);
            thread = new Thread(Execute);

            thread.Start();
        }

        private unsafe void Execute()
        {
            while (completed == false)
            {
                uint bytesProcessed;
                uint completionKey;
                NativeOverlapped* native;

                bool result = CompletionInterop.GetQueuedCompletionStatus(
                    port,
                    out bytesProcessed,
                    out completionKey,
                    &native, 1000);

                if (native != null)
                {
                    Overlapped overlapped = Overlapped.Unpack(native);
                    CompletionCallback callback = overlapped.AsyncResult as CompletionCallback;

                    if (result)
                    {
                        callback?.Complete(native, (int)bytesProcessed);
                    }
                    else
                    {
                        callback?.Fail(native);
                    }

                    Overlapped.Free(native);
                }
            }
        }

        public void Add(IntPtr handle)
        {
            CompletionInterop.CreateIoCompletionPort(handle, port, (uint)handle.ToInt32(), 0);
        }

        public void Dispose()
        {
            completed = true;
            thread?.Join();
            thread = null;
        }
    }
}