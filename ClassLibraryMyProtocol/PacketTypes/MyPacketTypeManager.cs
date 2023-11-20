namespace ClassLibraryMyProtocol.PacketTypes
{
    /// <summary>
    /// Класс для преобразования значения перечисления MyPacketType в байты заголовков и наоборот.
    /// </summary>
    public static class MyPacketTypeManager
    {
        /// <summary>
        /// Словарь, по значению перечисления MyPacketType позволяет получить тип и подтип пакета (byte, byte)
        /// </summary>
        private static readonly Dictionary<MyPacketType, (byte, byte)> FromTypeToByteDictionary =
            new Dictionary<MyPacketType, (byte, byte)>();

        /// <summary>
        /// Словарь, по типу и подтипу пакета (byte, byte) позволяет получить значение перечисления MyPacketType
        /// </summary>
        private static readonly Dictionary<(byte, byte), MyPacketType> FromByteToTypeDictionary =
            new Dictionary<(byte, byte), MyPacketType>();


        /// <summary>
        /// Статический конструктор, регистрирует типы пакетов.
        /// </summary>
        static MyPacketTypeManager()
        {
            RegisterType(MyPacketType.Handshake, 1, 0);
			RegisterType(MyPacketType.GuessTheNumber, 2, 0);
		}

        /// <summary>
        /// Регистрация типа пакета.
        /// </summary>
        /// <param name="packetType"> тип пакета из перечисления MyPacketType </param>
        /// <param name="type"> тип пакета </param>
        /// <param name="subtype"> подтип пакета </param>
        public static void RegisterType(MyPacketType packetType, byte type, byte subtype)
        {
            FromTypeToByteDictionary[packetType] = (type, subtype);
            FromByteToTypeDictionary[(type, subtype)] = packetType;
        }


        /// <summary>
        /// Получить тип и подтип пакета по MyPacketType.
        /// </summary>
        /// <param name="packetType"> значение перечисления MyPacketType </param>
        /// <returns> тип и подтип пакета </returns>
        public static (byte, byte)? ConvertFromTypeToByte(MyPacketType packetType)
        {
            if (!FromTypeToByteDictionary.ContainsKey(packetType))
            {
                return null;
            }

            return FromTypeToByteDictionary[packetType];
        }

        /// <summary>
        /// Получить MyPacketType по типу и подтипу пакета.
        /// </summary>
        /// <param name="type"> тип пакета </param>
        /// <param name="subtype"> подтип пакета </param>
        /// <returns> значение перечисления MyPacketType </returns>
        public static MyPacketType ConvertFromByteToType(byte type, byte subtype)
        {
            if (!FromByteToTypeDictionary.ContainsKey((type, subtype)))
            {
                return MyPacketType.Unknown;
            }

            return FromByteToTypeDictionary[(type, subtype)];
        }
    }
}
