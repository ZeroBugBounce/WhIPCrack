using System;
using System.Diagnostics;
using System.Text;
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
		static void Main(string[] args)
		{
			Thread receiver = new Thread(_ => StartReceiver());
			receiver.Start();
			Thread.Sleep(Timeout.Infinite);
		}

		static void StartReceiver()
		{
			int messagesreceived = 0;
			var fore = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("**********************************************");
			Console.WriteLine("                    Receiver");
			Console.WriteLine("**********************************************");
			Console.ForegroundColor = fore;

			Func<ReadBuffer, Message> messageDeserializer = buffer =>
			{
                var message = new Message
                {
                    A = buffer.Read<Int32>(),
                    B = buffer.Read<Int32>(),
                    C = buffer.Read<Int32>(),
                    D = buffer.Read<Int32>(),
                    Coordinates = buffer.ReadArray<Message.Coords>()
                };

                return message;

                //buffer.Write(msg[msgIndex].A);
                //buffer.Write(msg[msgIndex].B);
                //buffer.Write(msg[msgIndex].C);
                //buffer.Write(msg[msgIndex].D);
                //buffer.WriteArray(msg[msgIndex].Coordinates);
			};

			Stopwatch timer = new Stopwatch();
			timer.Start();
			var receiver = new Receiver<Message>(name: "speedTest", messageDeserializer: messageDeserializer, messageLength: 32, maxQueuedMessages: 1000, 
				onMessage: msg =>
			{
				//Console.WriteLine(msg.ToString());

				messagesreceived++;

				if (messagesreceived % 50000 == 0)
				{
					Console.WriteLine("{0:###,#} messages in {1:0.00} sec {2:###,0.00}msg/sec | {3:0}ns per message", messagesreceived, timer.Elapsed.TotalMilliseconds,
						((double)messagesreceived) / timer.Elapsed.TotalSeconds, (timer.Elapsed.TotalMilliseconds * 1000000) / ((double)messagesreceived));
				}
			});
		}
	}
}
