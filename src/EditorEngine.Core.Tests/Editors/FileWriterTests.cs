using System;
using System.IO;
using NUnit.Framework;
using EditorEngine.Core.Editors;
using EditorEngine.Core.Arguments;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Tests.Editors
{
	[TestFixture]
	public class FileWriterTests
	{
		private FileWriter _writer;
		private string _file;
		private string _content;
		
		[SetUp]
		public void Setup()
		{
			_writer = new FileWriter();
			_file = Path.GetTempFileName();
			_content = Path.GetTempFileName();
			File.WriteAllText(_file, getOriginal());
		}

		[TearDown]
		public void Teardown()
		{
			File.Delete(_file);
			File.Delete(_content);
		}
		
		[Test]
		public void When_asked_to_insert_a_piece_of_text_it_will_insert_it()
		{
			File.WriteAllText(_content, "(inserted text here)");
			var message = new EditorInsertMessage(
				"(inserted text here)",
				new GoTo()
					{
						File = _file,
						Line = 2,
						Column = 17
					},
				null);
			_writer.Insert(message);
			Assert.That(
				File.ReadAllText(_file),
				Is.EqualTo(
					"some text is already" + Environment.NewLine +
					"in here but will (inserted text here)be modified" + Environment.NewLine +
					"by these lovelly commands"));
		}
		
		[Test]
		public void When_asked_to_insert_a_piece_of_text_after_the_end_of_the_file_it_wont_be_inserted()
		{
			File.WriteAllText(_content, "(inserted text here)");
			var message = new EditorInsertMessage(
				"(inserted text here)",
				new GoTo()
					{
						File = _file,
						Line = 4,
						Column = 17
					},
				null);
			_writer.Insert(message);
			Assert.That(
				File.ReadAllText(_file),
				Is.EqualTo(
					"some text is already" + Environment.NewLine +
					"in here but will be modified" + Environment.NewLine +
					"by these lovelly commands"));
		}
		
		[Test]
		public void When_asked_to_delete_code_in_a_line_it_will_delete_it()
		{
			var message = new EditorRemoveMessage(
				new GoTo()
					{
						File = _file,
						Line = 2,
						Column = 17
					},
				new GoTo()
					{
						Line = 2,
						Column = 21
					});
			_writer.Remove(message);
			Assert.That(
				File.ReadAllText(_file),
				Is.EqualTo(
					"some text is already" + Environment.NewLine +
					"in here but will odified" + Environment.NewLine +
					"by these lovelly commands"));
		}
		
		[Test]
		public void When_asked_to_delete_code_in_multiple_line_it_will_delete_it()
		{
			var message = new EditorRemoveMessage(
				new GoTo()
					{
						File = _file,
						Line = 1,
						Column = 10
					},
				new GoTo()
					{
						Line = 2,
						Column = 21
					});
			_writer.Remove(message);
			Assert.That(
				File.ReadAllText(_file),
				Is.EqualTo(
					"some text odified" + Environment.NewLine +
					"by these lovelly commands"));
		}

		private string getOriginal()
		{
			return "some text is already" + Environment.NewLine +
				   "in here but will be modified" + Environment.NewLine +
				   "by these lovelly commands";
		}
	}
}
