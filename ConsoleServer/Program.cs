using ClassLibraryMyProtocol.Server;

Console.Title = "Server";

// Запускаем сервер
Server server = new Server("127.0.0.1", 8080);
server.Start();

// При завершении приложения выключаем сервер
AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
	server.Stop();
};

Console.ReadLine();