using System;
using NUnit.Framework;
using EditorEngine.Core.Messaging;
using System.Threading;
namespace EditorEngine.Core.Tests.Messaging
{
	[TestFixture]
	public class MessageDispatcherTests
	{
		private MessageDispatcher _dispatcher;
		private Fake_ConumserOfMessage _consumer;
		private Fake_Message _message;
		
		[SetUp]
		public void Setup()
		{
			_dispatcher = new MessageDispatcher();
			_consumer = new Fake_ConumserOfMessage();
			_message = new Fake_Message();
		}
		
		[Test]
		public void When_registering_a_consumer_of_message_it_should_recieve_message()
		{
			_dispatcher.Register(_consumer);
			_dispatcher.Publish(_message);
			Wait.ForTwoSecond().OrUntil(() => { return _consumer.HasConsumedMessage; });
			_consumer.Consumed(_message);
		}
		
		[Test]
		public void When_registering_a_consumer_factory_the_created_consumer_should_recieve_the_published_message()
		{
			_dispatcher.Register(() => { return _consumer; });
			_dispatcher.Publish(_message);
			Wait.ForTwoSecond().OrUntil(() => { return _consumer.HasConsumedMessage; });
			_consumer.Consumed(_message);			
		}
		
		[Test]
		public void When_registering_a_consumer_locator_the_created_consumer_should_recieve_the_published_message()
		{
			_dispatcher.Register((t) =>
			                     	{
										if (t.Equals(typeof(IConsumerOf<Fake_Message>)))
											return new IConsumerOf<Fake_Message>[] { _consumer };
										return new object[] {};
									});
			_dispatcher.Publish(_message);
			Wait.ForTwoSecond().OrUntil(() => { return _consumer.HasConsumedMessage; });
			_consumer.Consumed(_message);			
		}
	}
	
	class Fake_ConumserOfMessage : IConsumerOf<Fake_Message>
	{
		private Fake_Message _consumedMessage = null;
		
		public bool HasConsumedMessage { get { return _consumedMessage != null; } }
		
		public void Consume(Fake_Message message)
		{
			_consumedMessage = message;
		}
		
		public void Consumed(Fake_Message message)
		{
			Assert.That(message, Is.SameAs(_consumedMessage));
		}
	}
	
	class Fake_Message : Message
	{
	}
	
	class Wait
	{
		public static Wait ForTwoSecond()
		{
			return new Wait(DateTime.Now.AddSeconds(2));
		}
		
		private DateTime _timeout;
		
		public Wait(DateTime timeout)
		{
			_timeout = timeout;
		}
		
		public void OrUntil(Func<bool> completion)
		{
			while (DateTime.Now < _timeout)
			{
				Thread.Sleep(10);
				if (completion.Invoke())
					return;
			}
		}
	}
}

