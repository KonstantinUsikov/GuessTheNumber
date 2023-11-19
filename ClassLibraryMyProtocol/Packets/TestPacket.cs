using ClassLibraryMyProtocol.MyPacketMain;

namespace ClassLibraryMyProtocol.Packets
{
    /// <summary>
    /// Пакет для тестирования.
    /// </summary>
    public class TestPacket
    {
        [MyField(0)]
        public int IntValue;

        [MyField(1)]
        public double DoubleValue;

        [MyField(2)]
        public bool BooleanValue;
    }
}
