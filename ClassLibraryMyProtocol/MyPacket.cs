using System.Runtime.InteropServices;

namespace ClassLibraryMyProtocol
{
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
		#endregion

		#region Конструкторы
		/// <summary>
		/// Конструктор пакета.
		/// </summary>
		/// <param name="packetType"> тип пакета </param>
		/// <param name="packetSubtype"> подтип пакета </param>
		public MyPacket(byte packetType, byte packetSubtype) 
		{
			PacketType = packetType;
			PacketSubtype = packetSubtype;
		}

		/// <summary>
		/// Конструктор пакета.
		/// </summary>
		/// <param name="packet"> представление пакета в виде масива байт </param>
		public MyPacket(byte[] packet)
		{
			if (TryParse(packet, out MyPacket myPacket))
			{
				PacketType = myPacket.PacketType;
				PacketSubtype = myPacket.PacketSubtype;
				Fields = myPacket.Fields;
			}
		}

		/// <summary>
		/// Попробовать перевести пакет из массива байт в класс.
		/// </summary>
		/// <param name="packet"> пакет (массив байт) </param>
		/// <param name="result"> объект класса MyPacket </param>
		/// <returns> true - если удалось, false - если нет </returns>
		private bool TryParse(byte[] packet, out MyPacket result)
		{
			// Проверка размера пакета
			if (packet.Length < 7)
			{
				result = new MyPacket(0, 0);
				return false;
			}

			// Проверка байтов начала пакета
			if (packet[0] != 0xAF || packet[1] != 0xAA || packet[2] != 0xAF)
			{
				result = new MyPacket(0, 0);
				return false;
			}

			// Проверка байтов окончания пакета
			int mIndex = packet.Length - 1;
			if (packet[mIndex - 1] != 0xFF || packet[mIndex] != 0x00)
			{
				result = new MyPacket(0, 0);
				return false;
			}

			// Получаем тип и подтип пакета
			byte type = packet[3];
			byte subtype = packet[4];

			MyPacket myPacket = new MyPacket(type, subtype);

			// Получаем поля пакета
			byte[] fields = packet.Skip(5).ToArray();
			while (true)
			{
				// Если длина поля равна 2, то это признак окончания пакета
				if (fields.Length == 2)
				{
					result = myPacket;
					return true;
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

		#region Публичные методы
		/// <summary>
		/// Попробовать записать структуру в пакет.
		/// </summary>
		/// <typeparam name="T"> тип структуры </typeparam>
		/// <param name="id"> идентификатор </param>
		/// <param name="structure"> структура </param>
		/// <returns> true - если удалось, false - если нет </returns>
		public bool TrySetValue<T>(byte id, T structure) where T : struct
		{
			// Если поле с таким id уже есть, выйти из метода
			if (TryGetField(id, out MyPacketField field))
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
		/// Попробовать получить значение поля по идентификатору.
		/// </summary>
		/// <typeparam name="T"> тип значения </typeparam>
		/// <param name="id"> идентификатор </param>
		/// <param name="result"> значение поля </param>
		/// <returns> true - если удалось, false - если нет </returns>
		public bool TryGetValue<T>(byte id, out T result) where T : struct
		{
			// Пытаемся получить поле по id
			MyPacketField field;
			if (!TryGetField(id, out field))
			{
				result = default(T);
				return false;
			}

			// Проверяем, что размер структуры T совпадаем с размером поля пакета
			int neededSize = Marshal.SizeOf(typeof(T));
			if (field.FieldSize != neededSize)
			{
				result = default(T);
				return false;
			}

			// Парсим поле и возвращаем результат
			result = ByteArrayToStructure<T>(field.Contents);
			return true;
		}

		/// <summary>
		/// Приведение класса к пакету (массиву байт).
		/// </summary>
		/// <returns> массив байт </returns>
		public byte[] ToPacket()
		{
			MemoryStream packet = new MemoryStream();

			// Записываем байты начала пакета
			packet.Write(new byte[] { 0xAF, 0xAA, 0xAF, PacketType, PacketSubtype }, 0, 5);

			IEnumerable<MyPacketField> fields = Fields.OrderBy(field => field.FieldID);
			foreach (MyPacketField field in fields)
			{
				// Запивысаем идентификатор и размер поля
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
		private byte[] StructureToByteArray<T>(T structure) where T : struct
		{
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

		/// <summary>
		/// Попробовать получить поле пакета по идентификатору.
		/// </summary>
		/// <param name="id"> идентификатор поля </param>
		/// <param name="result"> поле пакета </param>
		/// <returns> true - если удалось, false - если нет </returns>
		private bool TryGetField(byte id, out MyPacketField result)
		{
			// В цикле ищем поле с нужным id
			foreach (MyPacketField field in Fields)
			{
				if (field.FieldID == id)
				{
					result = field;
					return true;
				}
			}

			result = new MyPacketField();
			return false;
		}
		#endregion
	}
}
