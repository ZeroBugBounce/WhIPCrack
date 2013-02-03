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

		TimeSpan throttleCalculateInterval = TimeSpan.FromMilliseconds(500);
		Double messagesPerSecondThrottle = Double.MaxValue;
		Stopwatch throttleTimer = new Stopwatch();
		Int32 messagesReceivedInThrottleWindow = 0;

		public SingleThreadedDispatcher() 
		{		
		}

		public SingleThreadedDispatcher(Double messagesPerSecondMax)
		{
			messagesPerSecondThrottle = messagesPerSecondMax;
		}

		public bool ExecutingSingleThreaded
		{
			get { return Thread.CurrentThread == thread;}
		}

		public void Execute(Action action)
		{
			EnsureStarted();

			//Int32 throttleWait = ThrottleWait();
			//if (throttleWait > 0)
			//{
			//    Thread.Sleep(throttleWait);
			//}

			if (!ExecutingSingleThreaded)
			{
				Monitor.Enter(syncLock);
				messagesReceivedInThrottleWindow++;
				queue.Enqueue(action);
				Monitor.Pulse(syncLock);
				Monitor.Exit(syncLock);
			}
		}

		Int32 ThrottleWait()
		{
			if (throttleTimer.Elapsed > throttleCalculateInterval)
			{
				throttleTimer.Stop();
				var messagesPerSecond = ((Double)messagesReceivedInThrottleWindow) / throttleTimer.Elapsed.TotalSeconds;

				messagesReceivedInThrottleWindow = 0;
				throttleTimer.Restart();

				if (messagesPerSecond > messagesPerSecondThrottle)
				{
					 return (Int32)(1000d * (messagesPerSecond / messagesPerSecondThrottle));
				}
			}

			return 0;
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
				throttleTimer.Start();
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
