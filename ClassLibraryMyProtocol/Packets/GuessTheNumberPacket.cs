using ClassLibraryMyProtocol.MyPacketMain;

namespace ClassLibraryMyProtocol.Packets
{
	/// <summary>
	/// Пакет для игры "Угадай число".
	/// </summary>
	public class GuessTheNumberPacket
	{
		[MyField(1)]
		public int Min;

		[MyField(2)]
		public int Max;

		[MyField(3)]
		public int Guess;

		[MyField(4)]
		public int LessOrMore;
	}
}
