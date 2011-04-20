using System;
using NUnit.Framework;
using EditorEngine.Core.Bootstrapping;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Endpoints.Tcp;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.FileSystem;
using EditorEngine.Core.Editors;
namespace EditorEngine.Core.Tests.Bootstrapping
{
	[TestFixture]
	public class DIContainterTests
	{
		[Test]
		public void Should_register_classes()
		{
			var container =  new DIContainer();
			container.Initalize();
			
			Assert.That(container.Resolve<IMessageDispatcher>(), Is.InstanceOf<IMessageDispatcher>());
			Assert.That(container.Resolve<ICommandEndpoint>(), Is.InstanceOf<ICommandEndpoint>());
			Assert.That(container.Resolve<IFS>(), Is.InstanceOf<IFS>());
			Assert.That(container.Resolve<IPluginLoader>(), Is.InstanceOf<IPluginLoader>());
			
			Assert.That(container.ResolveAll<IService>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<ICommandHandler>().Length, Is.EqualTo(2));
			Assert.That(container.ResolveAll<IConsumerOf<CommandMessage>>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<IConsumerOf<EditorGoToMessage>>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<IConsumerOf<EditorLoadMessage>>().Length, Is.EqualTo(1));
		}
	}
}

