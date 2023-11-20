using System;
using System.Windows.Threading;
using ClassLibraryMyProtocol;

namespace WpfLibrary
{
	/// <summary>
	/// Диспетчер для изменения элементов управления на форме.
	/// </summary>
	public class WpfUiDispatcher : IUiDispatcher
	{
		private readonly Dispatcher dispatcher;

		public WpfUiDispatcher(Dispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}

		public void Invoke(Action action)
		{
			dispatcher.Invoke(action);
		}
	}
}
