using ClassLibraryMyProtocol.MyPacketMain;
using ClassLibraryMyProtocol.Packets;
using ClassLibraryMyProtocol.PacketTypes;
using System.Net.Sockets;

namespace ClassLibraryMyProtocol.Server
{
    /// <summary>
    /// Класс, представляющий подключенного клиента.
    /// </summary>
    public class ConnectedClient
	{
		/// <summary>
		/// Сокет клиента.
		/// </summary>
		public Socket ClientSocket { get; }

		/// <summary>
		/// Очередь для отправки пакетов.
		/// </summary>
		private readonly Queue<byte[]> PacketSendingQueue = new Queue<byte[]>();

		/// <summary>
		/// Событие, возникающее, когда клиент отключается
		/// </summary>
		public event Action<ConnectedClient>? Disconnected;

		/// <summary>
		/// Загаданное число.
		/// </summary>
		private int? GuessedNumber { get; set; } = null;


		/// <summary>
		/// Конструктор.
		/// </summary>
		/// <param name="clientSocket"> сокет клиента </param>
		public ConnectedClient(Socket clientSocket)
		{
			ClientSocket = clientSocket;

			// Запустить задачи для получения и отправки пакетов
			Task.Run(ListenIncomingPackets);
			Task.Run(SendPackets);
		}

		/// <summary>
		/// Ожидание входящих от клиента пакетов.
		/// </summary>
		private void ListenIncomingPackets()
		{
			// Слушаем пакеты, пока клиент не отключится
			while (true) 
			{
				// Получаем пакет от клиента
				byte[] buff = new byte[256];
				int c = ClientSocket.Receive(buff);
				// Если клиент отключился
				if (c == 0)
				{
					Disconnected?.Invoke(this);
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
		/// Выбор метода обработки пакета в зависимости от его типа.
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
				case MyPacketType.GuessTheNumber:
					ProcessGuessTheNumber(packet);
					break;
				case MyPacketType.Unknown:
					break;
			}
		}

		/// <summary>
		/// Обработка пакета рукопожатия, формирование ответа.
		/// </summary>
		/// <param name="packet"> пакет </param>
		private void ProcessHandshake(MyPacket packet)
		{
			Console.WriteLine("Recieved handshake packet.");

			// Приводим пакет к типу MyPacketHandshake
			MyPacketHandshake handshakeReceived = MyPacketSerializer.Deserialize<MyPacketHandshake>(packet);

			// Обрабатываем пакет, подготавливаем ответ
			MyPacketHandshake handshakeAnswer = new MyPacketHandshake();
			handshakeAnswer.MagicHandshakeNumber = handshakeReceived.MagicHandshakeNumber + 1;

			Console.WriteLine("Answering...");

			// Приводим пакет к MyPacket и отправляем его клиенту
			MyPacket? packetToSend = MyPacketSerializer.Serialize(MyPacketType.Handshake, handshakeAnswer);
			if (packetToSend != null)
			{
				AddPacketToSendingQueue(packetToSend.ToPacket());
			}
		}

		/// <summary>
		/// Обработка пакета игры "Угадай число", формирование ответа.
		/// </summary>
		/// <param name="packet"> пакет </param>
		private void ProcessGuessTheNumber(MyPacket packet)
		{
			Console.WriteLine("Recieved GuessTheNumber packet.");

			// Приводим пакет к типу GuessTheNumberPacket
			GuessTheNumberPacket guessTheNumberReceived = MyPacketSerializer.Deserialize<GuessTheNumberPacket>(packet);

			// Обрабатываем пакет, подготавливаем ответ
			GuessTheNumberPacket guessTheNumberAnswer;
			if (GuessedNumber == null)
			{
				Random random = new Random();
				GuessedNumber = random.Next(guessTheNumberReceived.Min, guessTheNumberReceived.Max + 1);
				guessTheNumberAnswer = new GuessTheNumberPacket
				{
					Min = 1,
					Max = 1,
					Guess = 1,
					LessOrMore = 1
				};
			}
			else
			{
				int lessOrMore;
				if (guessTheNumberReceived.Guess < GuessedNumber)
				{
					lessOrMore = -1;
				}
				else if (guessTheNumberReceived.Guess > GuessedNumber)
				{
					lessOrMore = 1;
				}
				else
				{
					lessOrMore = 0;
				}

				guessTheNumberAnswer = new GuessTheNumberPacket
				{
					Min = 0,
					Max = 0,
					Guess = 0,
					LessOrMore = lessOrMore
				};
			}

			Console.WriteLine("Answering...");

			// Приводим пакет к MyPacket и отправляем его клиенту
			MyPacket? packetToSend = MyPacketSerializer.Serialize(MyPacketType.GuessTheNumber, guessTheNumberAnswer);
			if (packetToSend != null)
			{
				AddPacketToSendingQueue(packetToSend.ToPacket());
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
					ClientSocket.Send(packet);
				}

				Thread.Sleep(100);
			}
		}
	}
}
