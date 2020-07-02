# TCP_Comm

Simple async TCP communication library for sending and receiving strings and other serializable data.

The server triggers an Event if a message is arrived, and does no processing on the messages.
The messages can be acquired by the dequeue method.

The default port for sending and receiving TCP messages is 9001.
