using System;
using System.Diagnostics;
using System.Threading;
using whIPCrack;

namespace ConsoleReceiver
{
	struct Message
	{
		public Int32 A;
		public Int32 B;
		public Int32 C;
		public Int32 D;

		public Coords Coordinates;

		public struct Coords
		{
			public Double X;
			public Double Y;
		}

		public override string ToString()
		{
			return String.Format("A {0} B {1} C {2} D {3} Coords: {4},{5}", A, B, C, D, Coordinates.X, Coordinates.Y);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			Thread receiver = new Thread(_ => StartReceiver());
			receiver.Start();
			Thread.Sleep(Timeout.Infinite);
		}

		static Int32 MessagesReceived;

		static void StartReceiver()
		{
			var fore = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("**********************************************");
			Console.WriteLine("                    Receiver");
			Console.WriteLine("**********************************************");
			Console.ForegroundColor = fore;

			Func<ReadBuffer, Message> messageDeserializer = buffer =>
			{
				return buffer.Read<Message>();
			};

			Stopwatch timer = new Stopwatch();
			timer.Start();
			var receiver = new Receiver<Message>(name: "speedTest", messageDeserializer: messageDeserializer, messageLength: 32, maxQueuedMessages: 200, 
				onMessage: msg =>
			{
				MessagesReceived++;

				//Console.WriteLine(msg);

				if (MessagesReceived % 800000 == 0)
				{
					Console.WriteLine("{0:###,#} messages in {1:0.00} sec {2:###,0.00}msg/sec | {3:0}ns per message", MessagesReceived, timer.Elapsed.TotalSeconds,
						((Double)MessagesReceived) / timer.Elapsed.TotalSeconds, (timer.Elapsed.TotalMilliseconds * 1000000) / ((Double)MessagesReceived));
				}
			});
		}
	}
}
