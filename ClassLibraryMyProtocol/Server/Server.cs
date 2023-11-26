using System.Net.Sockets;
using System.Net;
using ClassLibraryMyProtocol.Client;

namespace ClassLibraryMyProtocol.Server
{
	/// <summary>
	/// Класс "Сервер".
	/// </summary>
	public class Server
	{
		/// <summary>
		/// Сокет сервера.
		/// </summary>
		private readonly Socket ServerSocket;

		/// <summary>
		/// Список подключенных клиентов.
		/// </summary>
		private readonly List<ConnectedClient> Clients = new List<ConnectedClient>();

		/// <summary>
		/// Ожидает ли сервер подключений.
		/// </summary>
		public bool Listening;

		/// <summary>
		/// Отправить информацию на форму.
		/// </summary>
		public event Action<string>? LogInformation;

		/// <summary>
		/// Диспетчер, позволяющий взаимодействовать с элементами из формы в другом потоке.
		/// </summary>
		private readonly IUiDispatcher? uiDispatcher;


		private string ip;
		private int port;

		/// <summary>
		/// Конструктор.
		/// </summary>
		public Server(string ip, int port, IUiDispatcher? uiDispatcher = null)
		{
			this.ip = ip;
			this.port = port;
			// Создаем сокет сервера
			this.uiDispatcher = uiDispatcher;
			IPAddress ipAddress = IPAddress.Parse(ip);
			ServerSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		}


		/// <summary>
		/// Запустить сервер.
		/// </summary>
		public void Start()
		{
			if (!Listening)
			{
				// Cвязываем сокет с локальным адресом и портом
				ServerSocket.Bind(new IPEndPoint(IPAddress.Any, port));

				// Переводим сокет в режим прослушивания входящих соединений
				ServerSocket.Listen(5);
				Listening = true;

				// Принимаем клиентов
				Task.Run(AcceptClients);
			}
		}

		/// <summary>
		/// Остановить сервер.
		/// </summary>
		public void Stop()
		{
			if (Listening)
			{
				// Закрываем соединение
				ServerSocket.Close();
				Listening = false;
			}
		}

		/// <summary>
		/// Ожидать подключений к серверу.
		/// </summary>
		private void AcceptClients()
		{
			while (true)
			{
				if (!Listening)
				{
					return;
				}

				// Получаем сокет подключившегося клиента
				Socket clientSocket;
				try
				{
					clientSocket = ServerSocket.Accept();
				}
				catch 
				{ 
					return; 
				}

				Console.WriteLine($"[!] Accepted client from {clientSocket.RemoteEndPoint}");
				uiDispatcher?.Invoke(() => LogInformation?.Invoke($"[!] Подключился клиент {clientSocket.RemoteEndPoint}"));

				// Добавляем клиента в список
				ConnectedClient client = new ConnectedClient(clientSocket, LogInformation, uiDispatcher);
				client.Disconnected += OnClientDisconnected;
				Clients.Add(client);
			}
		}

		/// <summary>
		/// Метод, срабатывающий при событии Disconnected клиента.
		/// </summary>
		/// <param name="client"> клиент </param>
		private void OnClientDisconnected(ConnectedClient client)
		{
			// Удаляем клиента из списка
			Clients.Remove(client);

			Console.WriteLine($"[!] Client {client.ClientSocket.RemoteEndPoint} disconnected");
			uiDispatcher?.Invoke(() => LogInformation?.Invoke($"[!] Клиент {client.ClientSocket.RemoteEndPoint} отключился"));
		}
	}
}
