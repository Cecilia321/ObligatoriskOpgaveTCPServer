// See https://aka.ms/new-console-template for more information
using ObligatoriskOpgaveTCPServer.Servere;

JsonServer s = new JsonServer();
s.Start();
Console.Read();
