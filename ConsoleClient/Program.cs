using ClassLibraryMyProtocol;
using ConsoleClient;

Console.Title = "Client";

Client client = new Client();
client.Connect("127.0.0.1", 8080);

AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
	client.Disconnect();
};

Console.WriteLine("Sending handshake packet..");
Thread.Sleep(5000);

MyPacketHandshake handshakePacket = new MyPacketHandshake
{
	MagicHandshakeNumber = TestHandshake.HandshakeNumber
};

MyPacket? packet = MyPacketSerializer.Serialize(MyPacketType.Handshake, handshakePacket);
if (packet != null)
{
	client.AddPacketToSendingQueue(packet.ToPacket());
}

Console.ReadLine();