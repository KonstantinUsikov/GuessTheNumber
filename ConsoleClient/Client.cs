using System.Net.Sockets;
using System.Net;
using ClassLibraryMyProtocol;

namespace ConsoleClient
{
	public class Client
	{
		/// <summary>
		/// Сокет.
		/// </summary>
		private Socket ClientSocket;

		/// <summary>
		/// Очередь для отправки пакетов.
		/// </summary>
		private readonly Queue<byte[]> PacketSendingQueue = new Queue<byte[]>();

		/// <summary>
		/// Подключен ли клиент к серверу.
		/// </summary>
		public bool Connected;


		/// <summary>
		/// Конструктор.
		/// </summary>
		public Client()
        {
			IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
			ClientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		}


		/// <summary>
		/// Подключиться к серверу.
		/// </summary>
		/// <param name="ip"> IP адрес сервера </param>
		/// <param name="port"> порт сервера </param>
        public void Connect(string ip, int port)
		{
			if (!Connected)
			{
				// Подключаемся к серверу
				ClientSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
				Connected = true;

				// Запускаем задачи для получения и отправки пакетов
				Task.Run(RecievePackets);
				Task.Run(SendPackets);
			}
		}

		/// <summary>
		/// Отключиться от сервера.
		/// </summary>
		public void Disconnect(bool hasServerClosed = false)
		{
            if (Connected)
            {
				if (hasServerClosed)
				{
                    Console.WriteLine("Server has closed");
                }

				// Закрываем соединение
				ClientSocket.Shutdown(SocketShutdown.Both);
				ClientSocket.Close();
			
				Connected = false;
			}
		}

		/// <summary>
		/// Ожидание входящих от сервера пакетов.
		/// </summary>
		private void RecievePackets()
		{
			while (true)
			{
				// Получаем пакет от сервера
				byte[] buff = new byte[256];
				try
				{
					ClientSocket.Receive(buff);
				}
				catch
				{
					// Если сервер отключился
					Disconnect(true);
					break;
				}

				// Получаем байты до конца пакета
				byte[] trimBuff = buff.TakeWhile((b, i) =>
				{
					return !(b == 0xFF && buff[i + 1] == 0);
				}).Concat(new byte[] { 0xFF, 0 }).ToArray();

				// Создаем объект пакета и обрабатываем его
				MyPacket? packet = MyPacket.Create(trimBuff);
				if (packet != null)
				{
					ProcessIncomingPacket(packet);
				}
			}
		}

		/// <summary>
		///  Выбор метода обработки пакета в зависимости от его типа.
		/// </summary>
		/// <param name="packet"> пакет </param>
		private void ProcessIncomingPacket(MyPacket packet)
		{
			// Получаем тип пакета
			MyPacketType type = MyPacketTypeManager.ConvertFromByteToType(packet.PacketType, packet.PacketSubtype);

			// В зависимости от типа вызываем нужный метод обработки
			switch (type)
			{
				case MyPacketType.Handshake:
					ProcessHandshake(packet);
					break;
				case MyPacketType.Unknown:
					break;
			}
		}

		/// <summary>
		/// Обработка пакета рукопожатия.
		/// </summary>
		/// <param name="packet"> пакет </param>
		private static void ProcessHandshake(MyPacket packet)
		{
			// Приводим пакет к типу MyPacketHandshake
			MyPacketHandshake handshakeReceived = MyPacketSerializer.Deserialize<MyPacketHandshake>(packet);

			// Обрабатываем результат
			if (handshakeReceived.MagicHandshakeNumber == TestHandshake.HandshakeNumber + 1)
			{
				Console.WriteLine("Handshake successful!");
			}
		}

		/// <summary>
		/// Добавление пакета в очередь отправки.
		/// </summary>
		/// <param name="packet"> пакет </param>
		public void AddPacketToSendingQueue(byte[] packet)
		{
			// Максимальная длина пакета - 256 байт
			if (packet.Length <= 256)
			{
				PacketSendingQueue.Enqueue(packet);
			}
		}

		/// <summary>
		/// Отправка пакетов, находящихся в очереди.
		/// </summary>
		private void SendPackets()
		{
			while (true)
			{
				// Если очередь отправки не пуста
				if (PacketSendingQueue.Count != 0)
				{
					// Получить пакет из очереди и отправить его
					byte[] packet = PacketSendingQueue.Dequeue();
					try
					{
						ClientSocket.Send(packet);
					}
					catch
					{
						// Если сервер отключился
						Disconnect(true);
						break;
					}
				}

				Thread.Sleep(100);
			}
		}
	}
}
