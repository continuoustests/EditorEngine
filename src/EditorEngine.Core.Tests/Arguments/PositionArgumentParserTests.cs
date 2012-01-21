using System;
using System.Reflection;
using EditorEngine.Core.Arguments;
using NUnit.Framework;

namespace EditorEngine.Core.Tests.Arguments
{
	[TestFixture]
	public class PositionArgumentParserTests
	{
		private PositionArgumentParser _parser;
		
		[SetUp]
		public void Setup()
		{
			_parser = new PositionArgumentParser();
		}
		
		[Test]
		public void When_invalid_argument_it_returns_null()
		{
			Assert.That(_parser.Parse("invalidfile"), Is.EqualTo(null));
		}
		
		[Test]
		public void When_given_a_file_with_no_column_or_line_it_retusn_file()
		{
			var file = getFile();
			var goTo = _parser.Parse(file);
			Assert.That(goTo.File, Is.EqualTo(file));
			Assert.That(goTo.Line, Is.EqualTo(0));
			Assert.That(goTo.Column, Is.EqualTo(0));
		}
		
		[Test]
		public void When_given_a_file_with_a_line_with_no_column()
		{
			var file = getFile();
			var goTo = _parser.Parse(file + "|34");
			Assert.That(goTo.File, Is.EqualTo(file));
			Assert.That(goTo.Line, Is.EqualTo(34));
			Assert.That(goTo.Column, Is.EqualTo(0));
		}
		
		[Test]
		public void When_given_a_file_with_a_line_and_column()
		{
			var file = getFile();
			var goTo = _parser.Parse(file + "|34|2");
			Assert.That(goTo.File, Is.EqualTo(file));
			Assert.That(goTo.Line, Is.EqualTo(34));
			Assert.That(goTo.Column, Is.EqualTo(2));
		}

		private string getFile()
		{
			return Assembly.GetExecutingAssembly().Location;
		}
	}
}
