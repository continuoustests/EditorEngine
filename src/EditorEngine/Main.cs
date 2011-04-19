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
				Bootstrapper.Register<IConsumerOf<ShutdownMessage>, ShutdownConsumer>();
				var shutdownConsumer = getShutdownConsumer();
				shutdownConsumer.Shutdown += HandleShutdownConsumerShutdown;
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
		
		private static ShutdownConsumer getShutdownConsumer()
		{
			var consumers = Bootstrapper.ResolveAll<IConsumerOf<ShutdownMessage>>();
			return (ShutdownConsumer) consumers[consumers.Length - 1];
		}
		
		private static void printUsages()
		{
			Console.WriteLine("EditorEngine.exe {key}");
		}
	}
}

