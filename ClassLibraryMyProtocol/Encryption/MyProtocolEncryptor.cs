namespace ClassLibraryMyProtocol.Encryption
{
    /// <summary>
    /// Класс, предоставляет методы для шифрования и расшифрования массивов байт.
    /// </summary>
    public static class MyProtocolEncryptor
    {
        /// <summary>
        /// Ключ шифрования.
        /// </summary>
        private static string Key { get; } = "2e985f930853919313c96d001cb5701f";

        /// <summary>
        /// Зашифровать массив байт.
        /// </summary>
        /// <param name="data"> данные </param>
        /// <returns> результат шифрования </returns>
        public static byte[] Encrypt(byte[] data)
        {
            return EncryptionHelper.Encrypt(data, Key);
        }

        /// <summary>
        /// Расшифровать массив байт.
        /// </summary>
        /// <param name="data"> данные </param>
        /// <returns> результат расшифрования </returns>
        public static byte[] Decrypt(byte[] data)
        {
            return EncryptionHelper.Decrypt(data, Key);
        }
    }
}
