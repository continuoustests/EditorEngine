using System;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Editors
{
	public interface IFileWriter
	{
		void Inject(EditorInjectMessage message);
		void Remove(EditorRemoveMessage msg);
	}
}
