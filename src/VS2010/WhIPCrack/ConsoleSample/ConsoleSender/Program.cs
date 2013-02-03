using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using whIPCrack;
using System.Diagnostics;

namespace ConsoleSender
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
		static Random Rng = new Random();

		static void StartSender()
		{
			var fore = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("**********************************************");
			Console.WriteLine("                    Sender");
			Console.WriteLine("**********************************************");
			Console.ForegroundColor = fore;

			Thread sender = new Thread(_ => RepeatingMessageSender());
			sender.Start();
			Thread.Sleep(Timeout.Infinite);
		}

		static void RepeatingMessageSender()
		{
			Func<Message, Byte[]> messageSerializer = msg =>
			{
				var buffer = new byte[16];

				BitConverter.GetBytes(msg.A).CopyTo(buffer, 0);
				BitConverter.GetBytes(msg.B).CopyTo(buffer, 4);
				BitConverter.GetBytes(msg.C).CopyTo(buffer, 8);
				BitConverter.GetBytes(msg.D).CopyTo(buffer, 12);

				return buffer;
			};

			// assembly the sender, queue length is not currently implemented
			var sender = new Sender<Message>("speedTest", messageSerializer, 16, 1, 100);

			sender.MessageDispatched += (s, e) =>
			{
				MessagesDispatched++;

				if (MessagesDispatched % 10000 == 0)
				{
					Console.WriteLine("{0:0,0} Messages sent", MessagesDispatched);
				}
			};

			while (true)
			{
				sender.Send(new Message { A = Rng.Next(), B = Rng.Next(), C = Rng.Next(), D = Rng.Next() });
			}
		}

		static Int32 MessagesDispatched;
		static Int32 MessagesReceived;

		//static void StartSender()
		//{
		//    var sender = new Sender<SampleMsg1>("sampleMsg1", msg =>
		//    {
		//        var buffer = new Byte[5];

		//        if (BitConverter.IsLittleEndian)
		//        {
		//            buffer[0] = (byte)(msg.Id);
		//            buffer[1] = (byte)(msg.Id >> 8);
		//            buffer[2] = (byte)(msg.Id >> 16);
		//            buffer[3] = (byte)(msg.Id >> 24);
		//            buffer[4] = msg.IsActive ? (byte)1 : (byte)0;
		//        }

		//        return buffer;
		//    }, 5, 100);

		//    while (true)
		//    {
		//        Console.WriteLine("press Enter key to send");
		//        Console.ReadLine();
		//        var msg = GenerateRandoSampleMsg();

		//        Console.WriteLine("{0} {1}", msg.Id, msg.IsActive);
		//        sender.Send(msg);
		//    }
		//}

		static void Main(string[] args)
		{
			StartSender();
		}		
	}
}
	