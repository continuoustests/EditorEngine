using System;
using NUnit.Framework;
using EditorEngine.Core.Bootstrapping;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Messaging;
using System.IO;
using System.Linq;
using EditorEngine.Core.Tests.Endpoints.Tcp;
using EditorEngine.Core.Tests.Messaging;
using System.Threading;
using EditorEngine.Core.Endpoints;

namespace EditorEngine.Core.Tests
{
	[TestFixture]
	public class BootstrapperTests
	{
		[Test]
		public void Should_set_up_messaging_and_start_services()
		{
			var guid = Guid.NewGuid().ToString();
			Bootstrapper.Initialize(guid);
			var consumer = new UsageFailureConsumer();
			Bootstrapper.Register<UsageErrorMessage>(consumer);
			var port = getPort(guid);
			var client = new Client();
			client.Connect(port);
			client.Send("goto FileThatDoesNotExist");
			Wait.ForTwoSecond().OrUntil(() => { return consumer.Message != null; });
			Bootstrapper.Shutdown();
			
			Assert.That(consumer.Message, Is.Not.Null);
		}
		
		private int getPort(string guid)
		{
			return Directory.GetFiles(Path.Combine(FS.GetTempDir(), "EditorEngine"))
				.Where(file => File.ReadAllLines(file)[0].Equals(guid))
				.Select(file => int.Parse(File.ReadAllLines(file)[1]))
				.First();
		}
	}
	
	class UsageFailureConsumer : IConsumerOf<UsageErrorMessage>
	{
		public UsageErrorMessage Message = null;
		
		public void Consume(UsageErrorMessage message)
		{
			Message = message;
		}
	}
}

