using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace EditorClient
{
	public class Arguments
	{
		public string Location { get; private set; }
		public string Command { get; private set; }
		public string Parameters { get; private set; }
		public bool Request { get; private set; }
		
		public static Arguments Parse(string[] args)
		{
			var hasLocation = false;
			var location = Environment.CurrentDirectory;
			if (args.Length > 1)
			{
				location = getLocation(args[0]);
				hasLocation = !location.Equals(Environment.CurrentDirectory);
			}
			bool request;
			var command = hasLocation ? args[1] : args[0];
			var parameters = getParameters(hasLocation, args, out request);
			return new Arguments(location, command, parameters, request);
		}
		
		private static string getLocation(string locationArgument)
		{
			if (File.Exists(locationArgument))
				return Path.GetDirectoryName(locationArgument);
			else if (Directory.Exists(locationArgument))
				return locationArgument;
			return Environment.CurrentDirectory;
		}
		
		private static string getParameters(bool hasLocation, string[] args, out bool request)
		{
			request = false;
			var sb = new StringBuilder();
			int start = hasLocation ? 2 : 1;
			for (int i = start; i < args.Length; i++)
			{
				if (args[i] == "--request")
					request = true;
				else
					sb.AppendFormat("{0} ", args[i]);
			}
			return sb.ToString();
		}
		
		public Arguments(string location, string command, string parameters, bool request)
		{
			Location = location;
			Command = command;
			Parameters = parameters;
			Request = request;
		}
	}
}

