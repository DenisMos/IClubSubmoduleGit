using GitView.Git;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using SProperties = GitViewReload.Properties;

namespace GitView
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowsViewModel _model;

		private bool Initialize = false;

		private System.Threading.Timer _timer;

		SynchronizationContext _synchronizationContext;

		NotifyIcon nIcon = new NotifyIcon();

		public MainWindow()
		{
			nIcon.Icon = SProperties.Resources.non;
			nIcon.Visible = true;
			nIcon.Text = $"Нет подключения. ";

			this.Closing += OnClosedAction;

			var contextMenu = new System.Windows.Forms.ContextMenu();
			var menuItem1 = new System.Windows.Forms.MenuItem();
			menuItem1.Text = "Синхронизировать";
			var menuItem2 = new System.Windows.Forms.MenuItem();
			menuItem2.Text = "Отправить";
			var menuItem4 = new System.Windows.Forms.MenuItem();
			menuItem4.Text = "Визуализация";
			var menuItem3 = new System.Windows.Forms.MenuItem();
			menuItem3.Text = "Выйти";
			contextMenu.MenuItems.Add(menuItem1);
			contextMenu.MenuItems.Add(menuItem2);
			contextMenu.MenuItems.Add(menuItem4);
			contextMenu.MenuItems.Add(menuItem3);

			menuItem4.Click += OnMenu4Click;
			menuItem3.Click += OnMenu3Click;
			menuItem2.Click += OnMenu2Click;
			menuItem1.Click += OnMenu1Click;

			nIcon.ContextMenu = contextMenu;

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

			_synchronizationContext = SynchronizationContext.Current;
			_timer = new System.Threading.Timer(OnTimer, null, 0, 30000);

			Send.IsEnabled = false;
		}

		private void OnMenu4Click(object sender, EventArgs e)
		{
			Vizualization(sender, null);
		}

		private void OnMenu1Click(object sender, EventArgs e)
		{
			CheckDiff(null);
		}

		private void OnMenu2Click(object sender, EventArgs e)
		{
			Push(this, null);
		}

		private void OnMenu3Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void OnClosedAction(object sender, CancelEventArgs e)
		{
			nIcon.Visible = false;
		}

		private async void OnTimer(object state)
		{
			if(Initialize)
			{
				_synchronizationContext.Post(CheckDiff, state);
			}
			else
			{
				nIcon.Icon = SProperties.Resources.non;
			}
		}

		private void Vizualization(object sender, RoutedEventArgs e)
		{
			var wrap = new WrapperGit();
			var url = System.IO.Path.GetFileNameWithoutExtension(_url.Text);
			wrap.SetDirect(Environment.CurrentDirectory + "\\" + url);

			wrap.Start("show --pretty=\"\" --name-only head", "gitk");
		}

		private async void Connect(object sender, RoutedEventArgs e)
		{
			try
			{
				C.IsEnabled = false;

				var satus = await _model.Connect(_name.Text, _pass.Text, _url.Text);

				System.IO.File.WriteAllText("cfg.txt", _url.Text);


				Initialize = Send.IsEnabled = satus;
				CheckDiff(null);
			}
			catch(Exception exc)
			{
				_model.Log.MessageError(exc.Message);
			}
			finally
			{
				C.IsEnabled = true;
			}
		}

		private async void Sync(object sender, RoutedEventArgs e)
		{
			CheckDiff(null);
		}

		public async void CheckDiff(object sender)
		{
			if(!Initialize)
				return;

			try
			{
				var wrapper = new WrapperGit();

				var url = System.IO.Path.GetFileNameWithoutExtension(_url.Text);

				wrapper.SetDirect(Environment.CurrentDirectory + "\\" + url);

				_model.Items.Clear();

				var count = 0;

				var diff = await wrapper.StartAsync(GitAPI.Untracked);
				count += AddRows("Новые объекты:", diff, Brushes.MediumPurple);

				var unstage = await wrapper.StartAsync(GitAPI.Unstage);
				count += AddRows("Изменённые объекты:", unstage, Brushes.DarkCyan);

				switch(count)
				{
					case 0:
						nIcon.Icon = SProperties.Resources.title;
						break;
					case 1:
						nIcon.Icon = SProperties.Resources.mod11;
						this.Icon = ConvertBTS(SProperties.Resources.mod1p);
						break;
					case 2:
						nIcon.Icon = SProperties.Resources.mod2;
						break;
					case 3:
						nIcon.Icon = SProperties.Resources.mod3;
						break;
					case 4:
						nIcon.Icon = SProperties.Resources.mod4;
						break;
					case 5:
						nIcon.Icon = SProperties.Resources.mod5_;
						break;
					default:
					{
						if(count > 100)
						{
							nIcon.Icon = SProperties.Resources._100_;
							ConvertBTS(SProperties.Resources._100_p);

						}
						else if(count > 10)
						{
							nIcon.Icon = SProperties.Resources._10_;
							ConvertBTS(SProperties.Resources._10_p);

						}
						else if(count > 5)
						{
							nIcon.Icon = SProperties.Resources.mod5_;
							ConvertBTS(SProperties.Resources.mod5p_);

						}
					}
					break;
				}
				if(count == 0)
				{
					nIcon.Icon = SProperties.Resources.title;
				}
				nIcon.Text = $"Подключён: {_name.Text}, изменений: {count}";
			}
			catch(Exception exc)
			{
				_model.Log.MessageError(exc.Message);
			}
		}

		public BitmapImage ConvertBTS(Bitmap src)
		{
			MemoryStream ms = new MemoryStream();
			((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
			BitmapImage image = new BitmapImage();
			image.BeginInit();
			ms.Seek(0, SeekOrigin.Begin);
			image.StreamSource = ms;
			image.EndInit();
			return image;
		}

		private int AddRows(string group_name, string data, SolidColorBrush brush)
		{
			var rows = data.Split('\n');
			if(rows.Length > 0)
			{
				_model.Items.Add(new Log.LogCollection.LogElement(group_name, brush));
			}
			int count = 0;
			foreach(var row in rows)
			{
				if(!string.IsNullOrEmpty(row))
				{
					count++;
					_model.Items.Add(new Log.LogCollection.LogElement(row, brush));
				}
			}

			return count;
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{

		}

		private async void Push(object sender, RoutedEventArgs e)
		{
			try
			{
				Send.IsEnabled = false;

				await _model.Push(_name.Text);

				CheckDiff(null);
			}
			catch(Exception exc)
			{
				_model.Log.MessageError(exc.Message);
			}
			finally
			{
				Send.IsEnabled = true;
			}
		}

		private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var text = ((TextBlock)sender).Text;
		}
	}
}
