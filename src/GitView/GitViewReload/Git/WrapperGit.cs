using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

using System.Runtime.InteropServices;
using System.Threading;

namespace GitView.Git
{
	public class WrapperGit : IDisposable
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AttachConsole(uint dwProcessId);


		private Process Process { get; }

		public ProcessStartInfo StartInfo { get; }

		public WrapperGit()
		{ 
			Process = new Process();
			StartInfo = Process.StartInfo = new ProcessStartInfo();
			StartInfo.UseShellExecute = false;
			StartInfo.RedirectStandardOutput = true;
			StartInfo.RedirectStandardError = true;
			StartInfo.CreateNoWindow = true;
			StartInfo.FileName = "git";

			App.Current.Exit += OnExit;
		}

		private void OnExit(object sender, ExitEventArgs e)
		{
			if(!Process.HasExited)
			{
				Process.Kill();
			}
			Process.Close();
			Process.Dispose();
		}

		public void SetDirect(string path)
		{ 
			StartInfo.WorkingDirectory = path;
		}

		public async Task StartProcessing(string command, ContextCallback asyncCallback)
		{
			SetArgs(command);
			await Task.Run(() =>
			{
				Process.Start();

				while(!Process.StandardOutput.EndOfStream || !Process.StandardError.EndOfStream)
				{
					var data1 = Process.StandardOutput.ReadLine();
					var data2 = Process.StandardError.ReadLine();
					if(string.IsNullOrEmpty(data1))
					{
						asyncCallback.Invoke(data2);
					}
					else if(string.IsNullOrEmpty(data2))
					{
						asyncCallback.Invoke(data1);
					}
					
				}

				Process.WaitForExit();
			});
		}

		public async Task<string> StartAsync(string command, bool check = false)
		{
			SetArgs(command);
			if(check)
			{
				StartInfo.CreateNoWindow = false;
				StartInfo.RedirectStandardOutput = false;
				StartInfo.RedirectStandardError = false;
			}
			try
			{
				var d = await Task.Run(() => Start(command, check)).ConfigureAwait(false);
				if(!check)
				{
					return d;
				}
				else
				{
					return "Смотрите данные в консоли";
				}
			}
			catch (Exception exc)
			{
				return exc.Message;
			}
			finally
			{
				if(check)
				{
					StartInfo.CreateNoWindow = true;
					StartInfo.RedirectStandardOutput = true;
					StartInfo.RedirectStandardError = true;
				}
			}
		}

		public string Start(string command, string name)
		{
			SetArgs(string.Empty);
			var cache = StartInfo.FileName;

			StartInfo.FileName = name;

			var resp = Start();

			StartInfo.FileName = cache;

			return resp;
		}

		public string Start(string command, bool check = false)
		{
			SetArgs(command);
			return Start(check);
		}

		public async Task StartAsync()
		{
			await Task.Run(() => Start());
		}

		public void SetArgs(string args)
		{ 
			StartInfo.Arguments = args;
		}

		public bool IsExeption { get; private set; }

		public string Start(bool check = false)
		{
			Process.Start();
			Process.WaitForExit();

			if(!check)
			{
				var message = Process.StandardOutput.ReadToEnd();
				if(string.IsNullOrEmpty(message))
				{
					IsExeption = true;
					message = Process.StandardError.ReadToEnd();
				}
				else
				{
					IsExeption = false;
				}

				return message + Process.StandardError.ReadToEnd();
			}
			else
			{
				return string.Empty;
			}
		}

		public string GetError()
		{ 
			return Process.StandardError.ReadToEnd();
		}

		public void Dispose()
		{
			Process.Close();
			Process.Dispose();
		}

		internal Task StartAsync(object diff)
		{
			throw new NotImplementedException();
		}
	}
}
