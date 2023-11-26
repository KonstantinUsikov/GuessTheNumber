using ClassLibraryMyProtocol;
using ClassLibraryMyProtocol.Client;
using ClassLibraryMyProtocol.MyPacketMain;
using ClassLibraryMyProtocol.Packets;
using ClassLibraryMyProtocol.PacketTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfLibrary;

namespace WpfAppClient
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Client client;
		private bool textBoxMinValid = true;
		private bool textBoxMaxValid = true;
		private bool gameStarted = false;
		private int guessCount = 0;

		public MainWindow()
		{
			InitializeComponent();
			Dispatcher? dispatcher = Application.Current.Dispatcher;
			client = new Client(new WpfUiDispatcher(dispatcher));
			client.GuessIsLessThanNumber += AppendTextBoxLessThan;
			client.GuessIsMoreThanNumber += AppendTextBoxMoreThan;
			client.GuessIsEqualToNumber += PlayerHasWon;
			client.ServerHasDisconnected += ServerHasDisconnected;
		}

		private void buttonConnect_Click(object sender, RoutedEventArgs e)
		{
			string input = textBoxIp.Text;

			// Делим строку на части
			string[] parts = input.Split(':');
			if (parts.Length != 2)
			{
				MessageBox.Show("Неверный формат данных.");
				return;
			}

			// Получаем данные из строк
			string ip = parts[0];
			if (!int.TryParse(parts[1], out int port))
			{
				MessageBox.Show("Не удалось привести порт к числу.");
				return;
			}

			// Пробуем подключиться к серверу
			try
			{
				client.Connect(ip, port);
				textBoxMin.IsEnabled = true;
				textBoxMax.IsEnabled = true;
				buttonStart.IsEnabled = true;
				textBoxIp.IsEnabled = false;
				buttonConnect.IsEnabled = false;
			}
			catch (Exception)
			{
				MessageBox.Show("Не удалось подключиться к серверу");
			}
		}

		private void textBoxMin_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (textBoxMin == null)
			{
				return;
			}
			textBoxMinValid = int.TryParse(textBoxMin.Text, out int result);
			if (buttonStart != null)
			{
				if (textBoxMinValid && textBoxMaxValid && 
					int.Parse(textBoxMin.Text) < int.Parse(textBoxMax.Text) &&
					!gameStarted)
				{
					buttonStart.IsEnabled = true;
				}
				else
				{
					buttonStart.IsEnabled = false;
				}
			}
		}

		private void textBoxMax_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (textBoxMax == null)
			{
				return;
			}
			textBoxMaxValid = int.TryParse(textBoxMax.Text, out int result);
			if (buttonStart != null)
			{
				if (textBoxMinValid && textBoxMaxValid &&
					int.Parse(textBoxMin.Text) < int.Parse(textBoxMax.Text) &&
					!gameStarted)
				{
					buttonStart.IsEnabled = true;
				}
				else
				{
					buttonStart.IsEnabled = false;
				}
			}
		}

		private void buttonStart_Click(object sender, RoutedEventArgs e)
		{
			gameStarted = true;
			buttonStart.IsEnabled = false;
			textBoxMin.IsEnabled = false;
			textBoxMax.IsEnabled = false;
			textBoxGuess.IsEnabled = true;

			GuessTheNumberPacket guessTheNumberPacket = new GuessTheNumberPacket
			{
				Min = int.Parse(textBoxMin.Text),
				Max = int.Parse(textBoxMax.Text),
				NewGame = 1
			};

			// Приводим пакет к MyPacket
			MyPacket? packet = MyPacketSerializer.Serialize(MyPacketType.GuessTheNumber, guessTheNumberPacket);
			if (packet != null)
			{
				// Отправляем пакет серверу
				client.AddPacketToSendingQueue(packet.ToPacket());
			}
		}

		private void buttonGuess_Click(object sender, RoutedEventArgs e)
		{
			guessCount++;
			GuessTheNumberPacket guessTheNumberPacket = new GuessTheNumberPacket
			{
				Guess = int.Parse(textBoxGuess.Text)
			};

			// Приводим пакет к MyPacket
			MyPacket? packet = MyPacketSerializer.Serialize(MyPacketType.GuessTheNumber, guessTheNumberPacket);
			if (packet != null)
			{
				// Отправляем пакет серверу
				client.AddPacketToSendingQueue(packet.ToPacket());
			}
		}

		private void AppendTextBoxLessThan(string text)
		{
			textBoxLessThan.Text += text + "\n";
		}

		private void AppendTextBoxMoreThan(string text)
		{
			textBoxMoreThan.Text += text + "\n";
		}

		private void PlayerHasWon()
		{
			MessageBox.Show($"Вы выиграли! Количество попыток: {guessCount}");
			//client.Disconnect();
			//var dispatcher = Application.Current.Dispatcher;
			//client = new Client(new WpfUiDispatcher(dispatcher));
			//client.GuessIsLessThanNumber += AppendTextBoxLessThan;
			//client.GuessIsMoreThanNumber += AppendTextBoxMoreThan;
			//client.GuessIsEqualToNumber += PlayerHasWon;
			//client.ServerHasDisconnected += ServerHasDisconnected;
			//client.Connect("127.0.0.1", 8080);

			gameStarted = false;
			buttonStart.IsEnabled = true;
			textBoxMin.IsEnabled = true;
			textBoxMax.IsEnabled = true;
			textBoxGuess.IsEnabled = false;
			buttonGuess.IsEnabled = false;
			textBoxLessThan.Text = "";
			textBoxMoreThan.Text = "";
			textBoxGuess.Text = "";
			guessCount = 0;
		}

		private void ServerHasDisconnected()
		{
			MessageBox.Show("Сервер отключился!");
			client.Disconnect();

			textBoxIp.IsEnabled = true;
			buttonConnect.IsEnabled = true;
			textBoxMin.IsEnabled = false;
			textBoxMax.IsEnabled = false;
			buttonStart.IsEnabled = false;
			textBoxGuess.IsEnabled = false;
			buttonGuess.IsEnabled = false;
			textBoxLessThan.Text = "";
			textBoxMoreThan.Text = "";
			textBoxGuess.Text = "";
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			client.Disconnect();
		}

		private void textBoxGuess_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (int.TryParse(textBoxGuess.Text, out int result))
			{
				if (result <= int.Parse(textBoxMax.Text) && result >= int.Parse(textBoxMin.Text))
				{
					buttonGuess.IsEnabled = true;
				}
				else
				{
					buttonGuess.IsEnabled = false;
				}
			}
			else
			{
				buttonGuess.IsEnabled = false;
			}
		}
	}
}
