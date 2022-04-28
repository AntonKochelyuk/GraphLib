using System;

namespace Graphs.Implementations
{
	public class DisposableAction : IDisposable
	{
		private readonly Action m_action;
		
		public DisposableAction(Action onDispose)
		{
			m_action = onDispose;
		}

		public void Dispose()
		{
			m_action?.Invoke();	
		}
	}
}