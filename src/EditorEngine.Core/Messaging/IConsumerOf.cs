using System;
namespace EditorEngine.Core.Messaging
{
	public interface IConsumerOf<T> where T : Message
	{
		void Consume(T message);
	}
}

