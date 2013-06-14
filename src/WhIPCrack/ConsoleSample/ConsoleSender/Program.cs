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

        public Coords[] Coordinates;

        public struct Coords
        {
            public Double X;
            public Double Y;
        }

        public override string ToString()
        {
            return String.Format("A {0} B {1} C {2} D {3} Coords: {4}", A, B, C, D, CoordinatesToString());
        }

        string CoordinatesToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var coord in Coordinates)
            {
                builder.AppendFormat("Coord {0} {1}", coord.X, coord.Y);
            }

            return builder.ToString();
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

			Action<Message[], WriteBuffer> messageSerializer = (msg, buffer) =>
			{
                for (int msgIndex = 0; msgIndex < msg.Length; msgIndex++)
                {
                    buffer.Write(msg[msgIndex].A);
                    buffer.Write(msg[msgIndex].B);
                    buffer.Write(msg[msgIndex].C);
                    buffer.Write(msg[msgIndex].D);
                    buffer.WriteArray(msg[msgIndex].Coordinates);
                }
			};

			// assembly the sender, queue length is not currently implemented
			var sender = new Sender<Message>("speedTest", messageSerializer, 32, 1000);

			sender.MessageDispatched += (s, e) =>
			{
                Console.WriteLine(e.Message.ToString());

                //MessagesDispatched++;

                ////Console.WriteLine(e.Message);

                //if (MessagesDispatched % 800000 == 0)
                //{
                //    Console.WriteLine("{0:0,0} Messages sent", MessagesDispatched);
                //}
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
						Coordinates = new[] { new Message.Coords { X = messageValue, Y = messageValue }}
					};

					messageValue++;
				}

				sender.Send(messages);
				Thread.Sleep(10);
			}
		}

		static Int32 MessagesDispatched;

		static void Main(string[] args)
		{
			StartSender();
		}		
	}
}
	