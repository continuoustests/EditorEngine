using System;
using System.Linq;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Commands;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Commands.Handlers;
using System.Collections.Generic;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using EditorEngine.Core.Editors;
using EditorEngine.Core.FileSystem;

namespace EditorEngine.Core.Bootstrapping
{
	class DIContainer
	{
		private WindsorContainer _container;
		
		public void Initalize()
		{
			_container = new WindsorContainer();
            _container.Kernel.Resolver.AddSubResolver(new ArrayResolver(_container.Kernel));
            _container
				.Register(Component.For<IMessageDispatcher>().ImplementedBy<MessageDispatcher>().LifeStyle.Singleton)
				.Register(Component.For<IFS>().ImplementedBy<FS>())
				.Register(Component.For<IPluginLoader>().ImplementedBy<PluginLoader>())
				// Services
				.Register(Component.For<ICommandEndpoint>().Forward<IService>().ImplementedBy<CommandEndpoint>().LifeStyle.Singleton)
				// Message consumers
				.Register(Component.For<IConsumerOf<CommandMessage>>().ImplementedBy<CommandDispatcher>().LifeStyle.Singleton)
				.Register(Component.For<IConsumerOf<EditorLoadMessage>>()
							  .Forward<IConsumerOf<EditorGoToMessage>>()
					          .Forward<IConsumerOf<EditorSetFocusMessage>>()
					          .ImplementedBy<EditorDispatcher>().LifeStyle.Singleton)
				// Command handlers
				.Register(Component.For<ICommandHandler>().ImplementedBy<GoToHandler>())
				.Register(Component.For<ICommandHandler>().ImplementedBy<LoadEditorHandler>())
				.Register(Component.For<ICommandHandler>().ImplementedBy<SetFocusHandler>());
		}
		
		internal void Register<T,Y>()
		{
			_container.Register(Component.For<T>().ImplementedBy<Y>().LifeStyle.Singleton);
		}
		
		public T Resolve<T>()
		{
			return _container.Resolve<T>();
		}
		
		public T[] ResolveAll<T>()
		{
			return _container.ResolveAll<T>();
		}
		
		public IEnumerable<object> ResolveAll(Type type)
		{
			return _container.ResolveAll(type).OfType<object>();
		}
	}
}

