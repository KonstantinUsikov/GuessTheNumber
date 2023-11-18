namespace ClassLibraryMyProtocol
{
	/// <summary>
	/// Класс "Поле пакета".
	/// </summary>
	public class MyPacketField
	{
		/// <summary>
		/// Идентификатор поля.
		/// </summary>
		public byte FieldID { get; set; }

		/// <summary>
		/// Размер поля.
		/// </summary>
		public byte FieldSize { get; set; }

		/// <summary>
		/// Содержимое поля.
		/// </summary>
		public byte[] Contents { get; set; } = new byte[0];
	}
}
