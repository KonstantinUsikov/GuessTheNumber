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

namespace WpfAppClient
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private bool textBoxMinValid = true;
		private bool textBoxMaxValid = true;
		private bool gameStarted = false;

		private void textBoxMin_TextChanged(object sender, TextChangedEventArgs e)
		{
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
			buttonGuess.IsEnabled = true;

		}
	}
}
