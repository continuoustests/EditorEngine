using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
namespace EditorEngine.Core.Messaging
{
	public class MessageDispatcher : IMessageDispatcher
	{
		private List<KeyValuePair<Type, object>> _consumers = new List<KeyValuePair<Type, object>>();
		private List<KeyValuePair<Type, object>> _consumerFactories = new List<KeyValuePair<Type, object>>();
		private List<Func<Type, IEnumerable<object>>> _consumerLocators = new List<Func<Type, IEnumerable<object>>>();
		
		public void Register<T>(IConsumerOf<T> consumer) where T : Message
		{
			_consumers.Add(new KeyValuePair<Type, object>(typeof(IConsumerOf<T>), consumer));
		}

		public void Register<T>(Func<IConsumerOf<T>> consumerFactory) where T : Message
		{
			_consumerFactories.Add(new KeyValuePair<Type, object>(typeof(IConsumerOf<T>), consumerFactory));
		}
		
		public void Register(Func<Type, IEnumerable<object>> consumerLocator)
		{
			_consumerLocators.Add(consumerLocator);
		}

		public void Publish<T>(T message) where T : Message
		{
			publishToRegisteredConsumers(message);
			publishToConsumerFactories(message);
			publishToConsumerLocators(message);
		}
		
		private void publishToRegisteredConsumers<T>(T message) where T : Message
		{
			_consumers.Where(consumer => consumer.Key.Equals(typeof(IConsumerOf<T>))).ToList()
				.ForEach(consumer => publish(((IConsumerOf<T>)consumer.Value), message));
		}
		
		private void publishToConsumerFactories<T>(T message) where T : Message
		{
			_consumerFactories.Where(factory => factory.Key.Equals(typeof(IConsumerOf<T>))).ToList()
				.ForEach(factory => publish(((Func<IConsumerOf<T>>)factory.Value).Invoke(), message));
		}
		
		private void publishToConsumerLocators<T>(T message) where T : Message
		{
			_consumerLocators.ForEach(locator => locator.Invoke(typeof(IConsumerOf<T>)).ToList()
				.ForEach(consumer => publish(((IConsumerOf<T>)consumer), message)));
		}
		
		private void publish<T>(IConsumerOf<T> consumer, T message) where T : Message
		{
			Logger.Write("Dispatching to " + consumer.GetType().ToString() + " " + message.GetType().ToString());
			ThreadPool.QueueUserWorkItem((state) => { consumer.Consume(message); }); 
		}
	}
}

