using System;
using System.ServiceModel;

namespace EditorEngine.Core.Endpoints
{
	public interface ICommandEndpoint
	{
		void Run(string cmd);
	}
}

