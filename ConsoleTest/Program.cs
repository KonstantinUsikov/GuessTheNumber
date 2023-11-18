using ClassLibraryMyProtocol;

MyPacket packet = new MyPacket(1, 0);
packet.SetValue(0, 123);
packet.SetValue(1, 123.1F);
packet.SetValue(2, 123.12D);
packet.SetValue(3, false);

byte[] packetBytes = packet.ToPacket();
MyPacket parsedPacket = new MyPacket(packetBytes);

Console.WriteLine($"int: {parsedPacket.GetValue<int>(0)}\n" +
				  $"float: {parsedPacket.GetValue<float>(1)}\n" +
				  $"double: {parsedPacket.GetValue<double>(2)}\n" +
				  $"bool: {parsedPacket.GetValue<bool>(3)}");