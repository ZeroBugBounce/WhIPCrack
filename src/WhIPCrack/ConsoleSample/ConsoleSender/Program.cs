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
			Int32 messageValue = 0;

			Action<Message, WriteBuffer> messageSerializer = (msg, buffer) =>
			{
				buffer.Write<Message>(msg);
			};

			// assembly the sender, queue length is not currently implemented
			var sender = new Sender<Message>("speedTest", messageSerializer, 32, 200);

			sender.MaxPredispatchQueueLength = 100000;

			sender.MessageDispatched += (s, e) =>
			{
				MessagesDispatched++;

				//Console.WriteLine(e.Message);

				if (MessagesDispatched % 800000 == 0)
				{
					Console.WriteLine("{0:0,0} Messages sent", MessagesDispatched);
				}
			};

			while (true)
			{
				Message[] messages = new Message[1];

				for (Int32 index = 0; index < messages.Length; index++)
				{
					messages[index] = new Message
					{
						A = messageValue,
						B = messageValue,
						C = messageValue,
						D = messageValue,
						Coordinates = new Message.Coords { X = messageValue, Y = messageValue }
					};

					messageValue++;
				}

				sender.Send(messages);
				//Thread.Sleep(1000);
			}
		}

		static Int32 MessagesDispatched;
		//static Int32 MessagesReceived;

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
	