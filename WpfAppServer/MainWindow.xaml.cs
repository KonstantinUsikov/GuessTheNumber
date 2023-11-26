using ClassLibraryMyProtocol;
using ClassLibraryMyProtocol.Client;
using ClassLibraryMyProtocol.Server;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json;
using System.IO;

namespace WpfAppServer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Server? server;

		public MainWindow()
		{
			InitializeComponent();

			ServerSettings? settings = ReadJsonFile();
			if (settings == null || settings.Ip == null)
			{
				MessageBox.Show("Не могу запустить сервер");
				Close();
			}
			else
			{
				try
				{
					Dispatcher? dispatcher = Application.Current.Dispatcher;
					server = new Server(settings.Ip, settings.Port, new WpfUiDispatcher(dispatcher));
					server.LogInformation += Server_LogInfo;
					server.Start();
				}
				catch (Exception)
				{
					MessageBox.Show("Не могу запустить сервер");
					Close();
				}
			}	
		}

		private ServerSettings? ReadJsonFile()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "JSON Files|*.json";

			bool? result = openFileDialog.ShowDialog();
			if (result == false)
			{
				// Пользователь отменил выбор файла
				Close();
			}

			// Получаем путь к выбранному файлу
			string selectedFilePath = openFileDialog.FileName;

			// Читаем содержимое файла
			string json = File.ReadAllText(selectedFilePath);

			// Десериализуем JSON в объект
			ServerSettings? settings = JsonConvert.DeserializeObject<ServerSettings>(json);

			return settings;
		}

		private void Server_LogInfo(string text)
		{
			textBoxLogs.Text += text += "\n";
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			server?.Stop();
		}
	}
}
