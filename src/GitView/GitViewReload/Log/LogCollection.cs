using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static GitView.Log.LogCollection;

namespace GitView.Log
{
	public class LogCollection : IEnumerable<LogElement>, INotifyPropertyChanged
	{
		public sealed class LogElement
		{
			public LogElement(string text, SolidColorBrush brush = null)
			{ 
				Text = $"({DateTime.Now}): {text}";

				if(brush != null)
				{ 
					Color = brush;
				}
			}

			public string Text { get; }
			public SolidColorBrush Color { get; } = Brushes.Gray;
		}

		public ObservableCollection<LogElement> Rows { get; } = new ObservableCollection<LogElement>();

		public LogCollection() 
		{ 
			
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }  

		public void MessageError(string data)
		{ 
			Rows.Add(new LogElement(data, Brushes.Red));
			NotifyPropertyChanged(nameof(Rows));
		}

		public void Message(string data)
		{ 
			Rows.Add(new LogElement(data));
			NotifyPropertyChanged(nameof(Rows));
		}

		public void MessageCompleted(string data)
		{ 
			Rows.Add(new LogElement(data, Brushes.Green));
			NotifyPropertyChanged(nameof(Rows));
		}

		public IEnumerator<LogElement> GetEnumerator()
		{
			return Rows.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Rows.GetEnumerator();
		}
	}
}
