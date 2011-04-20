using System;
namespace EditorEngine.Core.FileSystem
{
	public interface IFS
	{
		bool FileExists(string file);
		bool DirectoryExists(string directory);
	}
}

