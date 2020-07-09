# TCP_Comm

Simple async TCP communication library for sending and receiving strings and other serializable data.

The server triggers an Event if a message is arrived, and does no processing on the messages.
The messages can be acquired by the dequeue method.

The default port for sending and receiving TCP messages is 9001.

# Version 0.2
Cleanup and reformatting for better readability and understanding.

# What's New
Introduced new file, Classes.cs to store the classes needed for the server and client instead of storing it in the Client.cs file.
Server now can be started to receie one message or keep on receiving every message

# Features
- Send and receive TCP messages with less code. 2 lines to set up server, 1 line to send message.
- Always listening server with easily read output
- Send and receive any serializable data
- Fully customize every aspect of the receiving and sending process
