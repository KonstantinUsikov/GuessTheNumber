using System.Runtime.InteropServices;

namespace ClassLibraryMyProtocol
{
	/// <summary>
	/// Класс "Пакет", предоставляет методы для добавления и чтения значений, шифрования и расшифрования пакета.
	/// </summary>
	public class MyPacket
	{
		#region Свойства
		/// <summary>
		/// Тип пакета.
		/// </summary>
		public byte PacketType { get; private set; }

		/// <summary>
		/// Подтип пакета.
		/// </summary>
		public byte PacketSubtype { get; private set; }

		/// <summary>
		/// Список полей пакета.
		/// </summary>
		public List<MyPacketField> Fields { get; set; } = new List<MyPacketField>();

		/// <summary>
		/// Зашифрован ли пакет.
		/// </summary>
		public bool Encrypted { get; set; }
		#endregion

		#region Фабричные методы
		/// <summary>
		/// Запрещаем использование конструктора.
		/// </summary>
		private MyPacket() { }

		/// <summary>
		/// Фабричный метод для создания пакета.
		/// </summary>
		/// <param name="packetType"> тип пакета </param>
		/// <param name="packetSubtype"> подтип пакета </param>
		/// <returns> пакет </returns>
		public static MyPacket Create(byte packetType, byte packetSubtype)
		{
			return new MyPacket
			{
				PacketType = packetType,
				PacketSubtype = packetSubtype
			};
		}

		/// <summary>
		/// Фабричный метод для создания пакета, если пакет зашифрован автоматически его расшифровывает.
		/// </summary>
		/// <param name="packet"> представление пакета в виде масива байт </param>
		/// <param name="markAsEncrypted"> был ли расшифрован пакет </param>
		/// <returns> пакет </returns>
		public static MyPacket? Create(byte[] packet, bool markAsEncrypted = false)
		{
			// Проверка размера пакета
			if (packet.Length < 7)
			{
				return null;
			}

			// Проверка байтов начала пакета
			bool encrypted;
			if (packet[0] == 0xAF && packet[1] == 0xAA && packet[2] == 0xAF)
			{
				encrypted = false;
			}
			else if (packet[0] == 0x95 && packet[1] == 0xAA && packet[2] == 0xFF)
			{
				encrypted = true;
			}
			else
			{
				return null;
			}

			// Проверка байтов окончания пакета
			int mIndex = packet.Length - 1;
			if (packet[mIndex - 1] != 0xFF || packet[mIndex] != 0x00)
			{
				return null;
			}

			// Получаем тип и подтип пакета
			byte type = packet[3];
			byte subtype = packet[4];

			MyPacket myPacket = MyPacket.Create(type, subtype);

			// Получаем поля пакета
			byte[] fields = packet.Skip(5).ToArray();
			while (true)
			{
				// Если длина поля равна 2, то это признак окончания пакета
				if (fields.Length == 2)
				{
					// Если пакет зашифрован, то расшифровать его
					return encrypted ? DecryptPacket(myPacket) : myPacket;
				}

				// Получаем идентификатор и размер пакета
				byte id = fields[0];
				byte size = fields[1];

				// Добавляем поле в список
				if (size != 0)
				{
					byte[] contents = fields.Skip(2).Take(size).ToArray();
					myPacket.Fields.Add(new MyPacketField
					{
						FieldID = id,
						FieldSize = size,
						Contents = contents
					});
				}

				// Переходим к следующему полю
				fields = fields.Skip(2 + size).ToArray();
			}
		}
		#endregion

		#region Методы шифрования
		/// <summary>
		/// Зашифровать пакет.
		/// </summary>
		/// <returns> зашифрованный пакет </returns>
		public MyPacket? Encrypt()
		{
			return EncryptPacket(this);
		}

		/// <summary>
		/// Расшифровать пакет.
		/// </summary>
		/// <returns> расшифрованный пакет </returns>
		public MyPacket? Decrypt()
		{
			return DecryptPacket(this);
		}

		/// <summary>
		/// Зашифровать пакет.
		/// </summary>
		/// <param name="packet"> пакет для шифрования </param>
		/// <returns> зашифрованный пакет </returns>
		private static MyPacket? EncryptPacket(MyPacket packet)
		{
			// Получаем пакет в байтах
			byte[] rawBytes = packet.ToPacket();
			// Шифруем этот пакет
			byte[] encrypted = MyProtocolEncryptor.Encrypt(rawBytes);

			// Создаем новый пакет
			MyPacket myPacket = Create(0, 0);
			// Записываем в него зашифрованные данные, обязательно с id = 0
			myPacket.SetValueRaw(0, encrypted);
			// Помечаем пакет как зашифрованный
			myPacket.Encrypted = true;

			return myPacket;
		}

		/// <summary>
		/// Расшифровать пакет.
		/// </summary>
		/// <param name="packet"> пакет для расшифрования </param>
		/// <returns> расшифрованный пакет </returns>
		private static MyPacket? DecryptPacket(MyPacket packet)
		{
			// Зашифрованные данные должны быть в поле с id = 0
			if (packet.GetField(0) == null)
			{
				return null; 
			}

			// Получаем зашифрованный пакет
			byte[]? rawData = packet.GetValueRaw(0); 
			if (rawData == null)
			{
				return null;
			}

			// Расшифровываем пакет
			byte[] decrypted = MyProtocolEncryptor.Decrypt(rawData);

			// Создаем новый пакет при помощи расшифрованных данных
			return Create(decrypted, true);
		}
		#endregion

		#region Публичные методы
		/// <summary>
		/// Записать структуру в пакет.
		/// </summary>
		/// <typeparam name="T"> тип структуры </typeparam>
		/// <param name="id"> идентификатор </param>
		/// <param name="structure"> структура </param>
		/// <returns> true - если удалось, false - если нет </returns>
		public bool SetValue<T>(byte id, T structure)
		{
			// Проверяем, является ли тип T значимым
			if (structure == null || !structure.GetType().IsValueType)
			{
				return false;
			}

			// Если поле с таким id уже есть, выйти из метода
			MyPacketField? field = GetField(id);
			if (field != null)
			{
				return false;
			}

			// Переводим структуру в массив байт
			byte[] bytes = StructureToByteArray(structure);
			if (bytes.Length > byte.MaxValue)
			{
				return false;
			}

			// Добавляем поле в пакет
			Fields.Add(new MyPacketField
			{
				FieldID = id,
				FieldSize = (byte)bytes.Length,
				Contents = bytes
			});
			return true;
		}

		/// <summary>
		/// Получить значение поля по идентификатору.
		/// </summary>
		/// <typeparam name="T"> тип значения </typeparam>
		/// <param name="id"> идентификатор </param>
		/// <returns> значение поля </returns>
		public T? GetValue<T>(byte id) where T : struct
		{
			// Пытаемся получить поле по id
			MyPacketField? field = GetField(id);
			if (field == null)
			{
				return null;
			}

			// Проверяем, что размер структуры T совпадаем с размером поля пакета
			int neededSize = Marshal.SizeOf(typeof(T));
			if (field.FieldSize != neededSize)
			{
				return null;
			}

			// Парсим поле и возвращаем результат
			return ByteArrayToStructure<T>(field.Contents);
		}

		/// <summary>
		/// Записать поле в пакет.
		/// </summary>
		/// <param name="id"> идентификатор </param>
		/// <param name="rawData"> массив байт </param>
		/// <returns> true - если удалось, false - если нет </returns>
		public bool SetValueRaw(byte id, byte[] rawData)
		{
			// Если поле с таким id уже есть, выйти из метода
			MyPacketField? field = GetField(id);
			if (field != null)
			{
				return false;
			}

			// Проверка размера массива
			if (rawData.Length > byte.MaxValue)
			{
				return false;
			}

			// Добавляем поле в пакет
			Fields.Add(new MyPacketField
			{
				FieldID = id,
				FieldSize = (byte)rawData.Length,
				Contents = rawData
			});
			return true;
		}

		/// <summary>
		/// Получить поле по идентификатору.
		/// </summary>
		/// <param name="id"> идентификатор </param>
		/// <returns> поле (массив байт) </returns>
		public byte[]? GetValueRaw(byte id)
		{
			// Пытаемся получить поле по id
			MyPacketField? field = GetField(id);
			if (field == null)
			{
				return null;
			}

			return field.Contents;
		}

		/// <summary>
		/// Получить поле пакета по идентификатору.
		/// </summary>
		/// <param name="id"> идентификатор поля </param>
		/// <returns> поле пакета </returns>
		public MyPacketField? GetField(byte id)
		{
			// В цикле ищем поле с нужным id
			foreach (MyPacketField field in Fields)
			{
				if (field.FieldID == id)
				{
					return field;
				}
			}

			return null;
		}

		/// <summary>
		/// Приведение класса к пакету (массиву байт).
		/// </summary>
		/// <returns> массив байт </returns>
		public byte[] ToPacket()
		{
			MemoryStream packet = new MemoryStream();

			// Записываем байты начала пакета
			packet.Write(
				Encrypted
					? new byte[] { 0x95, 0xAA, 0xFF, PacketType, PacketSubtype }
					: new byte[] { 0xAF, 0xAA, 0xAF, PacketType, PacketSubtype }, 0, 5);

			IEnumerable<MyPacketField> fields = Fields.OrderBy(field => field.FieldID);
			foreach (MyPacketField field in fields)
			{
				// Записываем идентификатор и размер поля
				packet.Write(new[] { field.FieldID, field.FieldSize }, 0, 2);
				// Записываем содержимое поля
				packet.Write(field.Contents, 0, field.Contents.Length);
			}

			// Записываем байты окончания пакета
			packet.Write(new byte[] { 0xFF, 0x00 }, 0, 2);

			return packet.ToArray();
		}
		#endregion

		#region Приватные методы
		/// <summary>
		/// Перевод структуры в массив байт.
		/// </summary>
		/// <typeparam name="T"> тип структуры </typeparam>
		/// <param name="structure"> структура </param>
		/// <returns> массив байт </returns>
		private byte[] StructureToByteArray<T>(T structure)
		{
			// Проверяем на null
			if (structure == null)
			{
				return new byte[0];
			}

			// Получаем размер объекта
			int rawsize = Marshal.SizeOf(structure);

			// Cоздаем массив, в который будет записана вся информация
			byte[] rawdata = new byte[rawsize];

			// Получаем дескриптор на наш массив и записываем в него объект
			GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
			Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
			handle.Free();

			return rawdata;
		}

		/// <summary>
		/// Перевод массива байт в структуру.
		/// </summary>
		/// <typeparam name="T"> тип структуры </typeparam>
		/// <param name="bytes"> массив байт </param>
		/// <returns> структура </returns>
		private T ByteArrayToStructure<T>(byte[] bytes) where T : struct
		{
			// Получаем дескриптор на наш массив
			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

			// Получаем структуру из дескриптора
			object? obj = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			T structure = obj == null ? default(T) : (T)obj;
			handle.Free();

			return structure;
		}
		#endregion
	}
}
