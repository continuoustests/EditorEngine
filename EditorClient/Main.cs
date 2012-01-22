using System;
namespace EditorClient
{
	public class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				printUsage();
				return;
			}
			var arguments = Arguments.Parse(args);
			var instance = new EngineLocator().GetInstance(arguments.Location);
			if (instance == null)
			{
				Console.WriteLine("Could not find running engine handling key path {0}", arguments.Location);
				return;
			}
			var command = string.Format("{0} {1}", arguments.Command, arguments.Parameters);
			if (arguments.Request)
			{
				Console.WriteLine("Requested: {0}", command);
				instance.Request(command);
			}
			else
			{
				Console.WriteLine("Sent command: {0}", command);
				instance.Send(command);
			}
		}
		
		private static void printUsage()
		{
			Console.WriteLine("EditorClient.exe {command} {parameters[]}");
			Console.WriteLine("EditorClient.exe {working location} {command} {parameters[]}");
		}
	}
}

