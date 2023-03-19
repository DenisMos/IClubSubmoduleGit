using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitView.Git
{
	public class WrapperGit : IDisposable
	{
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
		}

		public void SetDirect(string path)
		{ 
			StartInfo.WorkingDirectory = path;
		}

		public string Start(string command)
		{
			SetArgs(command);
			return Start();
		}

		public void SetArgs(string args)
		{ 
			StartInfo.Arguments = args;
		}

		public bool IsExeption { get; private set; }

		public string Start()
		{
			Process.Start();

			Process.WaitForExit();

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

		public string GetError()
		{ 
			return Process.StandardError.ReadToEnd();
		}

		public void Dispose()
		{
			Process.Close();
			Process.Dispose();
		}
	}
}
