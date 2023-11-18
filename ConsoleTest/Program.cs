using ClassLibraryMyProtocol;

MyPacket packet = new MyPacket(1, 0);
packet.TrySetValue(0, 123);
packet.TrySetValue(1, 123.1F);
packet.TrySetValue(2, 123.12D);
packet.TrySetValue(3, false);

byte[] packetBytes = packet.ToPacket();
MyPacket parsedPacket = new MyPacket(packetBytes);

parsedPacket.TryGetValue(0, out int resI);
parsedPacket.TryGetValue(1, out float resD);
parsedPacket.TryGetValue(2, out double resF);
parsedPacket.TryGetValue(3, out bool resB);

Console.WriteLine($"int: {resI}\n" +
				  $"float: {resD}\n" +
				  $"double: {resF}\n" +
				  $"bool: {resB}");
