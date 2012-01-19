using System;
using EditorEngine.Core.Messaging;
using System.Linq;
namespace EditorEngine.Core.Bootstrapping
{
	public static class Bootstrapper
	{
		private static DIContainer _container;
		private static string _key;
		
		public static void Initialize(string key)
		{
			_key = key;
			_container = new DIContainer();
			_container.Initalize();
			startServices();
		}
		
		public static void Shutdown()
		{
			stopServices();
		}

		public static void Register<T>(IConsumerOf<T> consumer) where T : Message
		{
			_container.Register<T>(consumer);
		}
		
		private static void startServices()
		{
			_container.GetServices().ToList()
				.ForEach(service => service.Start(_key));
		}
		
		private static void stopServices()
		{
			_container.GetServices().ToList()
				.ForEach(service => service.Stop());
		}
	}
}

