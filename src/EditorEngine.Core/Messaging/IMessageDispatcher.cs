using System;
using System.Collections.Generic;

namespace EditorEngine.Core.Messaging
{
	public interface IMessageDispatcher
	{
		void Register<T>(IConsumerOf<T> consumer) where T : Message;
		void Register<T>(Func<IConsumerOf<T>> consumerFactory) where T : Message;
		void Register(Func<Type, IEnumerable<object>> consumerLocator);
		
		void Publish<T>(T message) where T : Message;
	}
}

