using System;
using EditorEngine.Core.Editors;
using Rhino.Mocks;
using EditorEngine.Core.Messaging.Messages;
using NUnit.Framework;

namespace EditorEngine.Core.Tests.Editors
{
	[TestFixture]
	public class EditorDispatcherTests
	{
		private EditorDispatcher _dispatcher;
		private IEditor _editor;
		
		[SetUp]
		public void Setup()
		{
			_editor = MockRepository.GenerateMock<IEditor>();
			_dispatcher = new EditorDispatcher();
			_dispatcher.SetEditor(_editor);
		}
		
		[Test]
		public void Should_initialize_editor()
		{
			_dispatcher.Consume(new EditorGoToMessage("MyFile", 10, 10));
			_editor.AssertWasCalled(x => x.GoTo("MyFile", 10, 10));
		}
	}
}

