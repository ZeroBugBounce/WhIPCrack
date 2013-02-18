WhIPCrack
=========

Inter Process Communication via a very simple, thin layer over the .NET 4.0+ memory mapped file implementation.

### Example: ###
    
    var sender = new Sender<Message>(name: "speedTest", messageSerializer: messageSerializer, messageLength: 32, maxQueuedMessages: 200);

The maxQueuedMessages parameter means the maximum number of messages that are going to exist in the memory mapped file at once.  The more messages you send at once, the greater overall throughput you are going to get. 

messageSerializer is a delegate that you provide to manually serialize your message.  If your message is a simple struct with only primative types (including other structs of primative types) you can simply serialize the message like so:

    Action<Message, WriteBuffer> messageSerializer = (msg, buffer) =>
    {
         buffer.Write<Message>(msg);
    };

On the receiving side:

    var receiver = new Receiver<Message>(name: "speedTest", messageDeserializer: messageDeserializer, messageLength: 32, maxQueuedMessages: 200, 
                                        onMessage: msg => { /* whatever you wish to do when the message arrives* */});

Deserializing can be as mindless as serialization:

    Func<ReadBuffer, Message> messageDeserializer = buffer =>
    {
        return buffer.Read<Message>();
    };

And that's it - just start sending messages from the sender side and they will appear on the receiver side. 

### A few notes on use and operation: ###

1. It's possible to start and stop the sender and receiver processes at any point and messages will continue to be sent while both are up and running.
2. Bad things (or at least undefined things) will happen if you try to set up more than one sender or receiver with the same name on one machine.
3. Running a Sender or Receiver in an IIS process (and possibly other scenarios) requires the name to be prefixed with Global\ as in @"Global\name".
4. If you only send 1 message using the Send(TMessage message) method, more relative time will be spent waiting in synchronization and throughput will be vastly below optimal.  Using the Send(IEnumerable<TMessage> messages) with multiple messages will be more efficient.