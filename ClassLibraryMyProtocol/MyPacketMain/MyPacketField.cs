namespace ClassLibraryMyProtocol.MyPacketMain
{
    /// <summary>
    /// Класс, представляет собой поле пакета.
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
