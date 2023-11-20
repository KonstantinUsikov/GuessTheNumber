namespace ClassLibraryMyProtocol
{
	/// <summary>
	/// Интерфейс для обновления элементов управления на форме.
	/// </summary>
	public interface IUiDispatcher
	{
		void Invoke(Action action);
	}
}
