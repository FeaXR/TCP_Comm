# TCP_Comm

Simple async TCP communication library for sending and receiving strings and other serializable data.

The server triggers an Event if a message is arrived, and does no processing on the messages.
The messages can be acquired by the dequeue method.

The default port for sending and receiving TCP messages is 9001.

# Newest version
 * Version number: **0.4**
 * Release date: **2020. 08. 10.**

# What's New
Can run multiple instances on different ports parallel
Uses less memory than before
Cleanup and reformatting for better readability and understanding.
Added more comments for better understanding.

# Features
- Send and receive TCP messages with less code. 2 lines to set up server, 2 lines to send message.
- Always listening server with easily read output
- Send and receive any data with fully customizable message class
- Fully customize every aspect of the receiving and sending process
