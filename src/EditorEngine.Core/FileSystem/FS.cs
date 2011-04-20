using System;
using System.IO;
namespace EditorEngine.Core.FileSystem
{
	class FS : IFS
	{
		public bool FileExists(string file)
		{
			return File.Exists(file);
		}
		
		public bool DirectoryExists(string directory)
		{
			return Directory.Exists(directory);
		}
	}
}

