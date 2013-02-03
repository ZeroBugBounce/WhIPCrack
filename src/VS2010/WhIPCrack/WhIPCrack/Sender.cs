using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace whIPCrack
{
	public class Sender<TMessage> : IDisposable
	{
		MemoryMappedFile sendFile;
		MemoryMappedViewAccessor accessor;
		Func<TMessage, Byte[]> serializer;
		Int32 msgLength;
		SingleThreadedDispatcher sendDispatcher;

		EventWaitHandle senderWaitHandle;
		EventWaitHandle receiverWaitHandle;

		public event EventHandler<MessageDispatchEventArgs<TMessage>> MessageDispatched;

		public Sender(string name, Func<TMessage, Byte[]> messageSerializer, Int32 messageLength, Int32 queueLength, Double messagesPerSecondMax)
		{
			Name = name;
			serializer = messageSerializer;
			msgLength = messageLength;
			sendDispatcher = new SingleThreadedDispatcher(messagesPerSecondMax);

			sendDispatcher.Execute(() =>
			{
				sendFile = MemoryMappedFile.CreateOrOpen(Name, messageLength * queueLength, MemoryMappedFileAccess.ReadWrite);
				accessor = sendFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);
				senderWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, name + "sender");
				receiverWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, name + "receiver");

				senderWaitHandle.Set();
			});
		}

		public Sender(string name, Func<TMessage, Byte[]> messageSerializer, Int32 messageLength, Int32 queueLength) 
			: this(name, messageSerializer, messageLength, queueLength, Double.MaxValue)
		{
	
		}

		public string Name { get; private set; }

		public void Send(TMessage message)
		{
			sendDispatcher.Execute(() =>
			{
				receiverWaitHandle.WaitOne();
				var buffer = serializer(message);
				accessor.WriteArray(0, buffer, 0, msgLength);
				accessor.Flush();
				senderWaitHandle.Set();

				if (MessageDispatched != null)
				{
					MessageDispatched(this, new MessageDispatchEventArgs<TMessage>(ref message));
				}
			});
		}

		public void Dispose()
		{
			accessor.Dispose();
			sendFile.Dispose();

			GC.SuppressFinalize(this);
		}
	}

	public class MessageDispatchEventArgs<TMessage> : EventArgs
	{
		public TMessage Message { get; internal set; }

		public MessageDispatchEventArgs(ref TMessage message)
		{
			Message = message;
		}
	}
}
