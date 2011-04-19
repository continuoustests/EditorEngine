using System;
using System.ServiceModel;

namespace EditorEngine.Core.Endpoints
{
	interface ICommandEndpoint
	{
		void Run(string cmd);
	}
}

