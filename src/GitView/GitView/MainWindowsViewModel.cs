using GitView.Git;
using GitView.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GitView
{
	public class MainWindowsViewModel : INotifyPropertyChanged
	{
		public BitmapImage Image { get; private set; }

		public ObservableCollection<string> Items { get; }

		public LogCollection Log { get; } = new LogCollection();

		public MainWindowsViewModel() 
		{
			var wathc = Stopwatch.StartNew();
			wathc.Start();

			Items = new ObservableCollection<string>()
			{ 
				"Data"
			};

			wathc.Stop();

			Log.MessageCompleted($"Сервис запущен ({wathc.Elapsed})");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Push(string name)
		{ 
			var handler = new WrapperGit();
			handler.SetDirect(Environment.CurrentDirectory + "\\" + "IClubRepositoryForUnity");

			handler.Start(GitAPI.CommitAdd());

			Log.Message(handler.Start(GitAPI.Commit()));

			var message = handler.Start(GitAPI.Push(name));
			if(handler.IsExeption)
			{
				Log.MessageError(message);
			}
			else
			{
				Log.MessageCompleted(message);
			}
		}

		public bool Connect(string name, string pass, string url)
		{
			Log.MessageCompleted($"Подключение к {url}");

			var handler = new WrapperGit();
			handler.Start(GitAPI.Clone(url));
			Log.Message($"Клонирование репозитория");
			var urlw = System.IO.Path.GetFileNameWithoutExtension(url);

			handler.SetDirect(Environment.CurrentDirectory + "\\" + urlw);

			if(handler.IsExeption)
			{
				Log.Message($"Обновление ветки");

				handler.Start(GitAPI.Fetch());
				handler.Start();
			}

			handler.SetArgs(GitAPI.CheckoutRemout(name));
			Log.Message(handler.Start());

			if(!handler.IsExeption)
			{
				handler.SetArgs(GitAPI.CheckoutLocalB(name));
				Log.Message(handler.Start());

				if(handler.IsExeption) {
					handler.SetArgs($"checkout {name}");
					Log.Message(handler.Start());

					handler.Start("pull");
				}
			}
			else
			{ 
				handler.SetArgs(GitAPI.CheckoutLocal(name));
				var log = handler.Start();

				if(handler.IsExeption)
				{
					Log.MessageError($"Неправильные данные: {log}");
					return false;
				}
				else
				{ 
					handler.Start("pull");
					Log.MessageCompleted("Подключение установлено");
				}
			}

			var path = @"IClubRepositoryForUnity/Title.jpg";

			if(File.Exists(path))
			{
				Image = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Image"));

			return true;
		}
	}
}
