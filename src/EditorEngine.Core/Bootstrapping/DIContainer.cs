using System;
using System.Linq;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Commands;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Commands.Handlers;
using System.Collections.Generic;
using EditorEngine.Core.Editors;
using EditorEngine.Core.FileSystem;

namespace EditorEngine.Core.Bootstrapping
{
	class DIContainer
	{
		private CommandEndpoint _endpoint;
		private IMessageDispatcher _dispatcher;

		public void Initalize()
		{
            _dispatcher = new MessageDispatcher();
			_endpoint = new CommandEndpoint(_dispatcher);
			var dirtyFilesHandler = new GetDirtyFilesHandler(_dispatcher, _endpoint);
			var getCaretHandler = new GetCaretHandler(_dispatcher, _endpoint);
			var loadEditorHandler = new LoadEditorHandler(_dispatcher, _endpoint);
			
			var commandDispatcher = new CommandDispatcher(
				new ICommandHandler[]
					{
						new GoToHandler(_dispatcher),
						loadEditorHandler,
						new SetFocusHandler(_dispatcher),
						new InsertHandler(_dispatcher),
						new RemoveHandler(_dispatcher),
						new ReplaceHandler(_dispatcher),
						new RefactorHandler(_dispatcher),
						dirtyFilesHandler,
						new EditorCommandHandler(_dispatcher),
						getCaretHandler,
						new RequestUserSelectionHandler(_dispatcher),
						new RequestUserSelectionAtCaretHandler(_dispatcher),
						new RequestUserInputHandler(_dispatcher)
					});
			_dispatcher.Register<CommandMessage>(commandDispatcher);
			_dispatcher.Register<EditorDirtyFilesListMessage>(dirtyFilesHandler);
			_dispatcher.Register<EditorCaretMessage>(getCaretHandler);

			var editorDispatcher = new EditorDispatcher(new PluginLoader(_endpoint), _dispatcher, null);
			_dispatcher.Register<EditorLoadMessage>(editorDispatcher);
			_dispatcher.Register<EditorLoadedMessage>(loadEditorHandler);
			_dispatcher.Register<EditorGoToMessage>(editorDispatcher);
			_dispatcher.Register<EditorSetFocusMessage>(editorDispatcher);
			_dispatcher.Register<EditorInsertMessage>(editorDispatcher);
			_dispatcher.Register<EditorRemoveMessage>(editorDispatcher);
			_dispatcher.Register<EditorReplaceMessage>(editorDispatcher);
			_dispatcher.Register<EditorRefactorMessage>(editorDispatcher);
			_dispatcher.Register<EditorGetDirtyFilesMessage>(editorDispatcher);
			_dispatcher.Register<EditorCommandMessage>(editorDispatcher);
			_dispatcher.Register<EditorGetCaretMessage>(editorDispatcher);
			_dispatcher.Register<EditorRequestUserSelection>(editorDispatcher);
			_dispatcher.Register<EditorRequestUserSelectionAtCaret>(editorDispatcher);
			_dispatcher.Register<EditorRequestUserInput>(editorDispatcher);
		}

		public void Register<T>(IConsumerOf<T> consumer) where T : Message
		{
			_dispatcher.Register<T>(consumer);
		}

		public IService[] GetServices()
		{
			return new IService[] { _endpoint };
		}
	}
}

