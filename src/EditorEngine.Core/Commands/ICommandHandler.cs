using System;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine.Core
{
	public interface ICommandHandler
	{
		string ID { get; }
		void Execute(CommandMessage message);
	}
}

