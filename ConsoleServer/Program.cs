using ClassLibraryMyProtocol.Server;

Console.Title = "Server";

// Запускаем сервер
Server server = new Server();
server.Start();

// При завершении приложения выключаем сервер
AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
	server.Stop();
};

Console.ReadLine();