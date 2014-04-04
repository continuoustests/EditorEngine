using System;
using NUnit.Framework;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Tests.Arguments
{
	[TestFixture]
	public class ArgumentParserTests
	{

        [Test]
        public void Can_parse_settings()
        {
            var args = ArgumentParser.Parse("key=value");
            Assert.That(args.Count, Is.EqualTo(1));
            Assert.That(args[0].Key, Is.EqualTo("key"));
            Assert.That(args[0].Value, Is.EqualTo("value"));
        }

		[Test]
		public void Can_parse_settings_with_spaces()
		{
            var args = ArgumentParser.Parse("\"--editor.sublime.executable=C:\\Program Files\\Sublime Text 2\\sublime_text.exe\"");
            Assert.That(args.Count, Is.EqualTo(1));
            Assert.That(args[0].Key, Is.EqualTo("--editor.sublime.executable"));
            Assert.That(args[0].Value, Is.EqualTo("C:\\Program Files\\Sublime Text 2\\sublime_text.exe"));
		}


        [Test]
        public void Can_parse_setting_of_mixed_types()
        {
            var args = ArgumentParser.Parse("key=value key2=value2 \"key.3=another key\" key4=value4");
            Assert.That(args.Count, Is.EqualTo(4));
            Assert.That(args[0].Key, Is.EqualTo("key"));
            Assert.That(args[0].Value, Is.EqualTo("value"));
            Assert.That(args[1].Key, Is.EqualTo("key2"));
            Assert.That(args[1].Value, Is.EqualTo("value2"));
            Assert.That(args[2].Key, Is.EqualTo("key.3"));
            Assert.That(args[2].Value, Is.EqualTo("another key"));
            Assert.That(args[3].Key, Is.EqualTo("key4"));
            Assert.That(args[3].Value, Is.EqualTo("value4"));
        }
	}
}