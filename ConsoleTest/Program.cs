using ClassLibraryMyProtocol;

MyPacket packet = MyPacket.Create(1, 0);
packet.SetValue(0, 123);
packet.SetValue(1, 123.1F);
packet.SetValue(2, 123.12D);
packet.SetValue(3, false);

byte[] packetBytes = packet.ToPacket();
MyPacket? parsedPacket = MyPacket.Create(packetBytes);
if (parsedPacket != null)
{
	Console.WriteLine($"int: {parsedPacket.GetValue<int>(0)}\n" +
				  $"float: {parsedPacket.GetValue<float>(1)}\n" +
				  $"double: {parsedPacket.GetValue<double>(2)}\n" +
				  $"bool: {parsedPacket.GetValue<bool>(3)}\n");
}



TestPacket testPacket = new TestPacket
{
	IntValue = 12345,
	DoubleValue = 123.45D,
	BooleanValue = true
};

MyPacket packetSer = MyPacketSerializer.Serialize(0, 0, testPacket);
TestPacket packetDes = MyPacketSerializer.Deserialize<TestPacket>(packetSer);

Console.WriteLine($"IntValue = {packetDes.IntValue}\n" +
					$"DoubleValue = {packetDes.DoubleValue}\n" +
					$"BooleanValue = {packetDes.BooleanValue}\n");



MyPacket pack = MyPacket.Create(0, 0);
pack.SetValue(1, 12345);
pack.SetValue(2, 2345);
Console.WriteLine("pack.Encrypted: " + pack.Encrypted);

MyPacket? packetEncrypted = pack.Encrypt();
byte[] encr = new byte[0];
if (packetEncrypted != null)
{
	Console.WriteLine("packetEncrypted.Encrypted: " + packetEncrypted.Encrypted);
	encr = packetEncrypted.ToPacket();
}

MyPacket? packetDecrypted = MyPacket.Create(encr);
if (packetDecrypted != null)
{
	Console.WriteLine("packetDecrypted.Encrypted: " + packetDecrypted.Encrypted);
	Console.WriteLine(packetDecrypted.GetValue<int>(1));
	Console.WriteLine(packetDecrypted.GetValue<int>(2));
}
