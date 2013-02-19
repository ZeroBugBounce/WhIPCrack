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
		WriteBuffer buffer;
		Action<TMessage[], WriteBuffer> serializer;

		Int32 msgLength;
		Int32 maxPredispatchQueueLength = -1;
		AutoResetEvent predispatchQueueLengthWaitHandle = new AutoResetEvent(false);
		EventWaitHandle senderWaitHandle;
		EventWaitHandle receiverWaitHandle;

		public event EventHandler<MessageDispatchEventArgs<TMessage>> MessageDispatched;

		public Sender(string name, Action<TMessage[], WriteBuffer> messageSerializer, Int32 messageLength, Int32 maxQueuedMessages)
		{
			Name = name;
			serializer = messageSerializer;
			msgLength = messageLength;

			sendFile = MemoryMappedFile.CreateOrOpen(Name, Constants.HeaderSize + (messageLength * maxQueuedMessages), MemoryMappedFileAccess.ReadWrite);
			accessor = sendFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);
			buffer = new WriteBuffer(accessor);

			senderWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, name + "sender");
			receiverWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, name + "receiver");

			senderWaitHandle.Set();
		}

		public string Name { get; private set; }

		public void Send(TMessage message)
		{
			receiverWaitHandle.WaitOne();
			Int32 writeLength = Constants.HeaderSize + msgLength;
			var buffer = new WriteBuffer(accessor);
			buffer.Write(1); // header	
			serializer(new[] {message}, buffer);
			accessor.Flush();
			senderWaitHandle.Set();

			if (MessageDispatched != null)
			{
				MessageDispatched(this, new MessageDispatchEventArgs<TMessage>(ref message));
			}
		}

		public void Send(IEnumerable<TMessage> messages)
		{
			TMessage[] messageArray = messages.ToArray();
			receiverWaitHandle.WaitOne();
			Int32 writeLength = Constants.HeaderSize + msgLength;
			var buffer = new WriteBuffer(accessor);
			buffer.Write(messages.Count()); // header	

			serializer(messageArray, buffer);

			accessor.Flush();
			senderWaitHandle.Set();

			if (MessageDispatched != null)
			{
				TMessage[] msgbuffer = messages.ToArray();
				for (Int32 index = 0; index < msgbuffer.Length; index++)
				{
					MessageDispatched(this, new MessageDispatchEventArgs<TMessage>(ref msgbuffer[index]));
				}
			}
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
