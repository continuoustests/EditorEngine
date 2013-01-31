using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace EditorEngine.Core.Arguments
{
	public class ArgumentParser
	{
		public static List<KeyValuePair<string,string>> Parse(string[] arguments) {
			var list = new List<KeyValuePair<string,string>>();
			foreach (var arg in arguments) {
				var argList = Parse(arg);
				if (argList != null)
					list.AddRange(argList);
			}
			return list;
		}

		public static List<KeyValuePair<string,string>> Parse(string argument)
		{
			var list = new List<KeyValuePair<string,string>>();
			var insideQuote = false;
			var word = "";
			var name = "";
			var previous = ' ';
			foreach (var c in argument)
			{
				if (!insideQuote && c == '"')
				{
					insideQuote = true;
					continue;
				}

				if (insideQuote)
				{
					if (c == '"' && previous != '\\')
					{
						insideQuote = false;
						continue;
					}
					word += c.ToString();
					previous = c;
					continue;
				}

				if (c == '=')
				{
					name = word.Trim();
					word = "";
					continue;
				}

				if (c == ' ')
				{
					list.Add(new KeyValuePair<string,string>(name, word.Trim().Replace("\\\"", "\"")));
					word = "";
					continue;
				}
				word += c.ToString();
				previous = c;
			}
			if (name.Length > 0 && word.Length > 0)
				list.Add(new KeyValuePair<string,string>(name, word.Trim().Replace("\\\"", "\"")));
			if (list.Count == 0)
				return null;
			return list;
		}
	}
}