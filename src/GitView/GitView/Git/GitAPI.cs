using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitView.Git
{
	public static class GitAPI
	{
		public static string Clone(string distantion) => $"clone {distantion}";

		public static string Fetch() => "fetch";

		//git add -A && git commit -m "Your Message"

		public static string Commit() => "commit -m --";
		public static string CommitAdd() => "add -A";

		public static string Push(string name) => $"push origin {name}";


		public static string CheckoutRemout(string name, string origin = "origin")
		{
			return $"checkout {origin}/{name}";
		}
		
		public static string CheckoutLocalB(string name)
		{
			return $"checkout -b {name}";
		}

		public static string CheckoutLocal(string name)
		{
			return $"checkout {name}";
		}

		public static string Branches()
		{
			return $"branch -r";
		}
	}
}
