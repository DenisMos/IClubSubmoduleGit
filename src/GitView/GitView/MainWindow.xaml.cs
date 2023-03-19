using GitView.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace GitView
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowsViewModel _model;

		public MainWindow()
		{
			DataContext = _model = new MainWindowsViewModel();

			InitializeComponent();

			if(System.IO.File.Exists("cfg.txt"))
			{
				var file = System.IO.File.ReadAllText("cfg.txt");

				try
				{
					var rows = file.Split(' ');
					_url.Text = rows[0];
				}
				catch
				(Exception ex)
				{

				}
			}

			Send.IsEnabled = false;
		}

		private void Connect(object sender, RoutedEventArgs e)
		{
			try
			{
				var satus = _model.Connect(_name.Text, _pass.Text, _url.Text);

				System.IO.File.WriteAllText("cfg.txt", _url.Text);

				Send.IsEnabled = satus;
			}
			catch(Exception exc)
			{
				_model.Log.MessageError(exc.Message);
			}
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			var wrapper = new WrapperGit();

			var url = System.IO.Path.GetFileNameWithoutExtension(_url.Text);

			wrapper.SetDirect(Environment.CurrentDirectory + "\\" + url);

		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{

		}

		private void Push(object sender, RoutedEventArgs e)
		{
			try
			{
				_model.Push(_name.Text);
			}
			catch(Exception exc)
			{
				_model.Log.MessageError(exc.Message);
			}
		}
	}
}
