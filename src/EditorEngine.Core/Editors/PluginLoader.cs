using System;
using EditorEngine.Core.FileSystem;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using System.Linq;
using EditorEngine.Core.Endpoints;

namespace EditorEngine.Core.Editors
{
	class PluginLoader : IPluginLoader
	{
		private ICommandEndpoint _publisher;
		
		public PluginLoader(ICommandEndpoint publisher)
		{
			_publisher = publisher;
		}
		
		public IEditor Load(string name)
		{
			try
			{
				var file = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugins"), string.Format("{0}.dll", name));
				if (!File.Exists(file))
					return null;
				var assembly = AssemblyDefinition.ReadAssembly(file);
				var pluginType = assembly.MainModule.Types
					.Where(type => type.Interfaces.ToList().Exists(i => i.FullName.Equals(typeof(IEditor).FullName)))
					.FirstOrDefault();
				if (pluginType == null)
					return null;
				var editor = (IEditor) Activator.CreateInstanceFrom(file, pluginType.ToString()).Unwrap();
				editor.Publisher = _publisher;
				return editor;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return null;
			}
		}
	}
}

