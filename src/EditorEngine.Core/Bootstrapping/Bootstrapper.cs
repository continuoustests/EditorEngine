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
			setupMessaging();
			startServices();
		}
		
		public static void Shutdown()
		{
			stopServices();
		}
		
		public static void Register<T,Y>()
		{
			_container.Register<T,Y>();
		}
		
		public static T Resolve<T>()
		{
			return _container.Resolve<T>();
		}
		
		public static T[] ResolveAll<T>()
		{
			return _container.ResolveAll<T>();
		}
		
		private static void setupMessaging()
		{
			var dispatcher = _container.Resolve<IMessageDispatcher>();
			dispatcher.Register((type) => { return _container.ResolveAll(type); });
		}
		
		private static void startServices()
		{
			_container.ResolveAll<IService>().ToList()
				.ForEach(service => service.Start(_key));
		}
		
		private static void stopServices()
		{
			_container.ResolveAll<IService>().ToList()
				.ForEach(service => service.Stop());
		}
	}
}

