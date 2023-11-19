using ClassLibraryMyProtocol;
using ConsoleClient;

Console.Title = "Client";

// Подключаемся к серверу
Client client = new Client();
client.Connect("127.0.0.1", 8080);

// При завержении приложения отключаемся от сервера
AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
	client.Disconnect();
};

Console.WriteLine("Sending handshake packet..");
Thread.Sleep(5000);

// Создаем пакет для отправки
MyPacketHandshake handshakePacket = new MyPacketHandshake
{
	MagicHandshakeNumber = MyPacketHandshake.HandshakeNumber
};

// Приводим пакет к MyPacket
MyPacket? packet = MyPacketSerializer.Serialize(MyPacketType.Handshake, handshakePacket);
if (packet != null)
{
	// Отправляем пакет серверу
	client.AddPacketToSendingQueue(packet.ToPacket());
}

Console.ReadLine();