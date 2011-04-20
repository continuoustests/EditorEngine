using System;
namespace EditorEngine.Core.Editors
{
	public interface IPluginLoader
	{
		IEditor Load(string name);
	}
}

