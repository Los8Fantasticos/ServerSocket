using Common.MessageHandlers;
using RabbitMqService.RabbitMq;
using Common.Messages.Client;
using SocketServer;
using RabbitMqService.Abstractions;
using RabbitMqService.Queues;

MessageConfig msgConf = new MessageConfig();
msgConf.Initialize();
MessageHandle.Initialize(msgConf);

MessageManagerSettings settings = new MessageManagerSettings();
settings.ConnectionString = "localhost";
settings.ExchangeName = "MessagingBroker";

QueueSettings queueSettings = new QueueSettings();
queueSettings.Add<Reconocimiento>();

MessageManager messageSender = new MessageManager(settings, queueSettings);

ServerMessageHandler serverHandler = new ServerMessageHandler(messageSender);

MessageHandle.RegisterMessageHandler<ClientRequestNumMessage>(serverHandler.SendToRabbitQueue<ClientRequestNumMessage>);

Server server = new Server(5000);
Console.ReadLine();
Console.WriteLine("\n Press any key to continue...");
Console.ReadKey();