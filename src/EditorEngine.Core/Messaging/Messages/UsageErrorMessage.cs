using System;
namespace EditorEngine.Core.Messaging.Messages
{
	public class UsageErrorMessage : Message
	{
		public string Feedback { get; private set; }
		
		public UsageErrorMessage(string feedback)
		{
			Feedback = feedback;
		}
	}
}

