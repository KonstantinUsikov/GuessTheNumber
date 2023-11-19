using ConsoleServer;

Console.Title = "Server";
Server server = new Server();
server.Start();

AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
	server.Stop();
};

Console.ReadLine();