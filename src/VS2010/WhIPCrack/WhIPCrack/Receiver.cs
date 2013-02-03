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
		Func<Byte[], TMessage> deserializer;
		Int32 msgLength;
		EventWaitHandle senderWaitHandle;
		EventWaitHandle receiverWaitHandle;
		Action<TMessage> messageCallback;

		public Receiver(string name, Func<Byte[], TMessage> messageDeserializer, Int32 messageLength, Int32 queueLength, Action<TMessage> onMessage)
		{
			Thread receiveThread = new Thread(_ =>
			{
				senderWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, name + "sender");
				senderWaitHandle.WaitOne();

				Name = name;
				receiveFile = MemoryMappedFile.CreateOrOpen(Name, messageLength * queueLength, MemoryMappedFileAccess.Read);
				msgLength = messageLength;
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
				Byte[] buffer = new Byte[msgLength];
				var b = accessor.ReadByte(0);

				var readCount = accessor.ReadArray<Byte>(0, buffer, 0, msgLength);
				messageCallback(deserializer(buffer));
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
