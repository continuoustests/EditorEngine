using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine
{
	public class ShutdownConsumer : IConsumerOf<ShutdownMessage>
	{
		public event EventHandler Shutdown;
		
		public void Consume (ShutdownMessage message)
		{
			if (Shutdown != null)
				Shutdown(this, new EventArgs());
		}
	}
}

