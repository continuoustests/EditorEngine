using System;
using System.IO;

namespace EditorEngine
{
	public static class Logger
	{
		private static EditorEngine.Core.Logging.ILogger _logger = null;

		public static void Enable()
		{
			_logger = new EditorEngine.Core.Logging.ConsoleLogger();
		}

		public static void Enable(string filename)
		{
			_logger = new EditorEngine.Core.Logging.FileLogger(filename);
		}

		public static void Write(string message)
		{
			if (_logger == null) return;
			_logger.Write(message);
		}
		
		public static void Write(string message, params object[] args)
		{
			if (_logger == null) return;
			_logger.Write(message, args);
		}
		
		public static void Write(Exception ex)
		{
			if (_logger == null) return;
			_logger.Write(ex.ToString());
		}
	}
}

namespace EditorEngine.Core.Logging
{
	interface ILogger
	{
		void Write(string message);
		void Write(string message, params object[] args);
		void Write(Exception ex);
	}

	class ConsoleLogger : ILogger
	{
		public void Write(string message)
		{
			Console.WriteLine(message);
		}

		public void Write(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}

		public void Write(Exception ex)
		{
			Console.WriteLine(getException(ex));
		}

		private string getException(Exception ex)
		{
			var message = ex.ToString();
			if (ex.InnerException != null)
				message += Environment.NewLine + getException(ex.InnerException);
			return message;
		}
	}

	class FileLogger : ILogger
	{
		private string _file;
		private object _padlock = new object();

		public FileLogger(string filePath)
		{
			_file = filePath;
		}

		public void Write(string message)
		{
			write(message);
		}

		public void Write(string message, params object[] args)
		{
			write(string.Format(message, args));
		}

		public void Write(Exception ex)
		{
			write(getException(ex));	
		}

		private string getException(Exception ex)
		{
			var message = ex.ToString();
			if (ex.InnerException != null)
				message += Environment.NewLine + getException(ex.InnerException);
			return message;
		}

		private void write(string message)
		{
			lock (_padlock) {
				using (var writer = new StreamWriter(_file, true))
				{
					writer.WriteLine(message);	
				}
			}
		}
	}
}
