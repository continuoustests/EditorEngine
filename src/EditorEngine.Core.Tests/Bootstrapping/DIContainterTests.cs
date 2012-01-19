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

			Assert.That(container.GetServices()[0], Is.InstanceOf<CommandEndpoint>());
		}
	}
}

