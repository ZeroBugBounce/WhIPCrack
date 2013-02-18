using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace whIPCrack
{
	public class Receiver<TMessage> : IDisposable
	{
		MemoryMappedFile receiveFile;
		MemoryMappedViewAccessor accessor;
		Func<ReadBuffer, TMessage> deserializer;
		Int32 msgLength;
		Int32 queueLength;
		EventWaitHandle senderWaitHandle;
		EventWaitHandle receiverWaitHandle;
		Action<TMessage> messageCallback;

		public Receiver(string name, Func<ReadBuffer, TMessage> messageDeserializer, Int32 messageLength, Int32 maxQueuedMessages, Action<TMessage> onMessage)
		{
			Thread receiveThread = new Thread(_ =>
			{
				senderWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, name + "sender");
				senderWaitHandle.WaitOne();

				Name = name;
				receiveFile = MemoryMappedFile.CreateOrOpen(Name, Constants.HeaderSize + (messageLength * maxQueuedMessages), MemoryMappedFileAccess.Read);
				msgLength = messageLength;
				queueLength = maxQueuedMessages;
				deserializer = messageDeserializer;
				accessor = receiveFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
				
				receiverWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, name + "receiver");
				messageCallback = onMessage;

				receiverWaitHandle.Set();
				Receive();
			});

			receiveThread.Start();
		}

		void Receive()
		{
			while (true)
			{
				senderWaitHandle.WaitOne();

				var buffer = new ReadBuffer(accessor);
				var messageCount = buffer.Read();

				while (--messageCount >= 0)
				{
					messageCallback(deserializer(buffer));
				}

				receiverWaitHandle.Set();
			}
		}

		public string Name { get; private set; }

		public void Dispose()
		{
			accessor.Dispose();
			receiveFile.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}
