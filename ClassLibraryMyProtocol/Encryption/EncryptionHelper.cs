using System.Security.Cryptography;
using System.Text;

namespace ClassLibraryMyProtocol.Encryption
{
    /// <summary>
    /// Класс, позволяет зашифровывать и расшифровывать массивы байт.
    /// </summary>
    public static class EncryptionHelper
    {
        /// <summary>
        /// Зашифровать массив байт.
        /// </summary>
        /// <param name="data"> данные для шифрования </param>
        /// <param name="passPhrase"> кодовое слово </param>
        /// <returns> результат шифрования </returns>
        public static byte[] Encrypt(byte[] data, string passPhrase)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = GenerateKey(passPhrase, aesAlg.KeySize / 8);
                aesAlg.GenerateIV();

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(data, 0, data.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                    }

                    byte[] encrypted = new byte[aesAlg.IV.Length + msEncrypt.ToArray().Length];
                    Array.Copy(aesAlg.IV, encrypted, aesAlg.IV.Length);
                    Array.Copy(msEncrypt.ToArray(), 0, encrypted, aesAlg.IV.Length, msEncrypt.ToArray().Length);

                    return encrypted;
                }
            }
        }

        /// <summary>
        /// Расшифровать массив байт.
        /// </summary>
        /// <param name="data"> данные для расшифровки </param>
        /// <param name="passPhrase"> кодовое слово </param>
        /// <returns> результат расшифровки </returns>
        public static byte[] Decrypt(byte[] data, string passPhrase)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = GenerateKey(passPhrase, aesAlg.KeySize / 8);

                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Array.Copy(data, iv, iv.Length);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                        {
                            csDecrypt.Write(data, iv.Length, data.Length - iv.Length);
                            csDecrypt.FlushFinalBlock();
                        }
                    }

                    return msDecrypt.ToArray();
                }
            }
        }

        /// <summary>
        /// Генерация ключа.
        /// </summary>
        /// <param name="passPhrase"> кодовое слово </param>
        /// <param name="keySize"> размер ключа </param>
        /// <returns> ключ </returns>
        private static byte[] GenerateKey(string passPhrase, int keySize)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passPhraseBytes = Encoding.UTF8.GetBytes(passPhrase);
                return sha256.ComputeHash(passPhraseBytes).Take(keySize).ToArray();
            }
        }
    }
}
