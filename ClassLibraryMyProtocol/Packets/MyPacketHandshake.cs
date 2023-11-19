using ClassLibraryMyProtocol.MyPacketMain;

namespace ClassLibraryMyProtocol.Packets
{
    /// <summary>
    /// Класс, представляющий пакет для рукопожатия клиента и сервера
    /// </summary>
    public class MyPacketHandshake
    {
        /// <summary>
        /// Число, которое будет передаваться при рукопожатии.
        /// </summary>
        public static int HandshakeNumber = 12;

        [MyField(1)]
        public int MagicHandshakeNumber;
    }
}
