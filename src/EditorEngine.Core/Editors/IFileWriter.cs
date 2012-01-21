using System;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Editors
{
	public interface IFileWriter
	{
		void Insert(EditorInsertMessage message);
		void Remove(EditorRemoveMessage msg);
	}
}
