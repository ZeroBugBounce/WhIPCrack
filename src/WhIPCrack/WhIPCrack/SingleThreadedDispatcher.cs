using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace whIPCrack
{
	public class SingleThreadedDispatcher
	{
		Thread thread;
		Queue<Action> queue;
		readonly Object syncLock = new Object();
		ManualResetEventSlim startupWaitHandle;

		public SingleThreadedDispatcher() 
		{	
	
		}

		public bool ExecutingSingleThreaded
		{
			get { return Thread.CurrentThread == thread;}
		}

		public void Execute(Action action)
		{
			EnsureStarted();

			if (!ExecutingSingleThreaded)
			{
				Monitor.Enter(syncLock);
				queue.Enqueue(action);
				Monitor.Pulse(syncLock);
				Monitor.Exit(syncLock);
			}
		}

		public Int32 QueueLength
		{
			get
			{
				return queue.Count;
			}
		}

		void EnsureStarted()
		{
			if (thread == null)
			{
				startupWaitHandle = new ManualResetEventSlim(false);
				queue = new Queue<Action>(10000);
				thread = new Thread(_ => Process());
				thread.IsBackground = true;
				thread.Start();
				startupWaitHandle.Wait();
			}
		}

		void Process()
		{
			Monitor.Enter(syncLock);
			startupWaitHandle.Set();

			while (true)
			{
				Monitor.Wait(syncLock);
			processWork:
				var work = queue.Dequeue();
				Monitor.Exit(syncLock);

				work();

				Monitor.Enter(syncLock);
				if (queue.Count > 0)
				{
					goto processWork;
				}
			}
		}
	}
}
