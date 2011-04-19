using System;
using EditorEngine.Core.Messaging;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
namespace EditorEngine.Core.Tests.Messaging
{
	class Fake_MessageDispatcher : IMessageDispatcher
	{
		private List<Message> _messages = new List<Message>();
		
		public void Register<T> (IConsumerOf<T> consumer) where T : Message
		{
		}

		public void Register<T> (Func<IConsumerOf<T>> consumerFactory) where T : Message
		{
		}

		public void Register (Func<Type, IEnumerable<object>> consumerLocator)
		{
		}

		public void Publish<T> (T message) where T : Message
		{
			_messages.Add(message);
		}
		
		public T GetPublishedMessage<T>() where T : Message
		{
			return (T) _messages.Where(x => x.GetType().Equals(typeof(T))).FirstOrDefault();
		}
		
		public void Published<T>() where T : Message
		{
			Assert.That(_messages.Exists(x => x.GetType().Equals(typeof(T))), Is.True, String.Format("Expected a message of type {0} to be published", typeof(T)));
		}
		
		public void DidNotPublish<T>() where T : Message
		{
			Assert.That(_messages.Exists(x => x.GetType().Equals(typeof(T))), Is.False, String.Format("Published a message of type {0} when it was not supposed to", typeof(T)));
		}
		
		public void Published<T>(T message) where T : Message
		{
			Assert.That(_messages.Exists(x => areSame(x, message)), Is.True, String.Format("The specified message was not published ({0})", message.ToString()));
		}
		
		public void DidNotPublish<T>(T message) where T : Message
		{
			Assert.That(_messages.Exists(x => areSame(x, message)), Is.False, String.Format("The specified message not published when it was not supposed to ({0})", message.ToString()));
		}
		
		private bool areSame(object obj1, object obj2)
		{
			return obj1 == obj2;
		}
	}
}

