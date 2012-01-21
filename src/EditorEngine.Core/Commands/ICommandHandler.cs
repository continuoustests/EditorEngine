using System;
namespace EditorEngine.Core
{
	public interface ICommandHandler
	{
		string ID { get; }
		void Execute(string[] arguments);
	}
}

