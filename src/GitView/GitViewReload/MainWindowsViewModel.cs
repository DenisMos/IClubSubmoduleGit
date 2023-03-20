using GitView.Git;
using GitView.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static GitView.Log.LogCollection;

namespace GitView
{
	public class MainWindowsViewModel : INotifyPropertyChanged
	{
		public BitmapSource Image { get; private set; }

		public ObservableCollection<LogElement> Items { get; }

		public LogCollection Log { get; } = new LogCollection();

		public MainWindowsViewModel() 
		{
			
			var wathc = Stopwatch.StartNew();
			wathc.Start();

			Items = new ObservableCollection<LogElement>()
			{ 
			};

			wathc.Stop();

			Log.MessageCompleted($"Сервис запущен ({wathc.Elapsed})");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public async Task Push(string name)
		{ 
			var handler = new WrapperGit();
			handler.SetDirect(Environment.CurrentDirectory + "\\" + "IClubRepositoryForUnity");

			await handler.StartAsync(GitAPI.CommitAdd());

			Log.Message(handler.Start(GitAPI.Commit()));

			var message = await handler.StartAsync(GitAPI.Push(name), true);
			if(message.Contains("->"))
			{
				Log.MessageCompleted($"Ветка '{name}' обновлена");
			}
			else
			{
				Log.MessageError(message);
			}
		}

		public async Task<bool> Connect(string name, string pass, string url)
		{
			Log.MessageCompleted($"Подключение к {url}");

			var handler = new WrapperGit();

			Log.Message($"Клонирование репозитория");
			await handler.StartAsync(GitAPI.Clone(url), true);

			var urlw = System.IO.Path.GetFileNameWithoutExtension(url);

			handler.SetDirect(Environment.CurrentDirectory + "\\" + urlw);

			if(handler.IsExeption)
			{
				Log.Message($"Обновление ветки");

				await handler.StartAsync(GitAPI.Fetch(), true);
			}

			var responceCheck = await handler.StartAsync(GitAPI.CheckoutRemout(name));
			Log.Message(responceCheck);

			if(!handler.IsExeption)
			{
				handler.SetArgs(GitAPI.CheckoutLocalB(name));
				Log.Message(handler.Start());

				if(handler.IsExeption) {
					Log.Message(await handler.StartAsync($"checkout {name}"));
					await handler.StartAsync("pull");
				}
			}
			else
			{ 
				var log = await handler.StartAsync(GitAPI.CheckoutLocal(name));

				if(handler.IsExeption)
				{
					Log.MessageError($"Неправильные данные: {log}");
					return false;
				}
				else
				{ 
					await handler.StartAsync("pull");
					Log.MessageCompleted("Подключение установлено");
				}
			}

			var path = $@"{handler.StartInfo.WorkingDirectory}/Title.jpg";

			if(File.Exists(path))
			{
				Image = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Image"));

			return true;
		}
	}
}
