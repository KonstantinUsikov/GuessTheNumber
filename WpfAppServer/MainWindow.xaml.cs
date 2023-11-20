using ClassLibraryMyProtocol;
using ClassLibraryMyProtocol.Client;
using ClassLibraryMyProtocol.Server;
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

namespace WpfAppServer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Server server;

		public MainWindow()
		{
			InitializeComponent();

			Dispatcher? dispatcher = Application.Current.Dispatcher;
			server = new Server(new WpfUiDispatcher(dispatcher));
			server.LogInformation += Server_LogInfo;
			server.Start();
		}

		private void Server_LogInfo(string text)
		{
			textBoxLogs.Text += text += "\n";
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			server.Stop();
		}
	}
}
