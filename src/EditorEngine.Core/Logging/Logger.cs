using System;

namespace EditorEngine.Core.Logging
{
	public static class Logger
	{
		private static bool _enabled = false;

		public static void Enable()
		{
			_enabled = true;
		}

		public static void Write(string message)
		{
			if (!_enabled) return;
			Console.WriteLine(message);
		}
		
		public static void Write(string message, params object[] args)
		{
			if (!_enabled) return;
			Console.WriteLine(message, args);
		}
		
		public static void Write(Exception ex)
		{
			if (!_enabled) return;
			Console.WriteLine(ex.ToString());
		}
	}
}
