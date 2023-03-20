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
using static System.Windows.Forms.AxHost;

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

		public async Task Push(string name, string url)
		{ 
			var handler = new WrapperGit();
			var urlw = System.IO.Path.GetFileNameWithoutExtension(url);
			handler.SetDirect(Environment.CurrentDirectory + "\\" + urlw);

			Log.Message(await handler.StartAsync(GitAPI.CommitAdd()));

			Log.Message(handler.Start(GitAPI.Commit()));

			var list = new List<string>();
			await handler.StartProcessing(GitAPI.Push(name), (object obj)=>list.Add(obj.ToString()));

			foreach(var item in list)
			{
				Log.Message($"{nameof(Push)}: {item}");
			}
		}

		public void OnCallback(object state)
		{
			Log.Message($"{nameof(Push)}: {state}");
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
