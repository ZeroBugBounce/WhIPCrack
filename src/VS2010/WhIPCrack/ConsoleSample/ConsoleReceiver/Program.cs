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

			Func<Byte[], Message> messageDeserializer = buffer =>
			{
				var msg = new Message();

				msg.A = BitConverter.ToInt32(buffer, 0);
				msg.B = BitConverter.ToInt32(buffer, 4);
				msg.C = BitConverter.ToInt32(buffer, 8);
				msg.D = BitConverter.ToInt32(buffer, 12);

				return msg;
			};

			Stopwatch timer = new Stopwatch();
			timer.Start();
			var receiver = new Receiver<Message>("speedTest", messageDeserializer, 16, 1, i =>
			{
				MessagesReceived++;

				if (MessagesReceived % 10000 == 0)
				{
					Console.WriteLine("{0} messages in {1} sec {2}msg/sec | {3}ms per message", MessagesReceived, timer.Elapsed.TotalSeconds,
						((Double)MessagesReceived) / timer.Elapsed.TotalSeconds, timer.Elapsed.TotalMilliseconds / ((Double)MessagesReceived));
				}
			});
		}
	}
}
