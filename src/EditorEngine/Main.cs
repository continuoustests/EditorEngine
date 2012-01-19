using System;
using EditorEngine.Core.Bootstrapping;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using System.Threading;

namespace EditorEngine
{
	class MainClass
	{
		private static bool _shutdown = false;
		
		public static void Main (string[] args)
		{
			if (args.Length != 1)
			{
				printUsages();
				return;
			}
			startApplication(args[0]);
		}
		
		private static void startApplication(string key)
		{
			try
			{
				Bootstrapper.Initialize(key);
				var shutdownConsumer = new ShutdownConsumer();
				Bootstrapper.Register<ShutdownMessage>(shutdownConsumer);
				shutdownConsumer.Shutdown += HandleShutdownConsumerShutdown;
				Console.WriteLine("Application running using key path {0}", key);
				while (!_shutdown)
					Thread.Sleep(100);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				Bootstrapper.Shutdown();
			}
		}

		static void HandleShutdownConsumerShutdown (object sender, EventArgs e)
		{
			_shutdown = true;
		}
		
		private static void printUsages()
		{
			Console.WriteLine("EditorEngine.exe {key}");
		}
	}
}

