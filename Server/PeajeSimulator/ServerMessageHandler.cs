﻿using Common;
using Common.Message.Server;
using Common.Messages;
using Common.Messages.Client;

using RabbitMqService.Abstractions;
using RabbitMqService.Queues;

namespace SocketServer
{
    public class ServerMessageHandler
    {
        private readonly IMessageSender _messageSender;
        public ServerMessageHandler(IMessageSender messageSender)
        {
            _messageSender = messageSender;
        }
        public void SendToRabbitQueue<T>(IBaseMessage message, ResponseToMachine toRespond) where T : ClientRequestNumMessage
        {
            var msg = (T)message;
            var msgString = msg.Message.ToString();
            _messageSender.PublishAsync<Reconocimiento, string>(msgString);
            Console.WriteLine($"Message sent to RabbitMQ: {msgString}");
        }
       
        public static void HandleString<T>(T message, ResponseToMachine toRespond) where T : StringMessage
        {
            Console.WriteLine(message.Message);
        }
    }
}