using System.Reflection;

namespace ClassLibraryMyProtocol
{
	/// <summary>
	/// Класс для сериализации/десериализации пакетов.
	/// </summary>
	public static class MyPacketSerializer
	{
		/// <summary>
		/// Получить поля, которые будут участвовать в процессе сериализации (имеют атрибут MyFieldAttribute).
		/// </summary>
		/// <param name="type"> тип пакета </param>
		/// <returns> список кортежей (значение поля, идентификатор) </returns>
		private static List<(FieldInfo, byte)> GetFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.Where(field => field.GetCustomAttribute<MyFieldAttribute>() != null)
				.Select(field => (field, field.GetCustomAttribute<MyFieldAttribute>()!.FieldID))
				.ToList();

		}

		/// <summary>
		/// Сериализация из класса с атрибутами в класс MyPacket.
		/// </summary>
		/// <param name="type"> тип пакета </param>
		/// <param name="subtype"> подтип пакета </param>
		/// <param name="obj"> класс пакета с атрибутами </param>
		/// <returns> пакет типа MyPacket </returns>
		public static MyPacket Serialize(byte type, byte subtype, object obj)
		{
			// Получаем список (поле, идентификатор) из объекта класса с атрибутами
			List<(FieldInfo, byte)> fields = GetFields(obj.GetType());

			// Добавляем поля из полученного списка в пакет MyPacket
			MyPacket packet = new MyPacket(type, subtype);
			foreach ((FieldInfo, byte) field in fields)
			{
				packet.SetValue(field.Item2, field.Item1.GetValue(obj));
			}

			return packet;
		}

		/// <summary>
		/// Десериализация из класса MyPacket в класс с атрибутами.
		/// </summary>
		/// <typeparam name="T"> тип класса с атрибутами </typeparam>
		/// <param name="packet"> пакет типа MyPacket </param>
		/// <returns> пакет в виде класса с атрибутами </returns>
		public static T Deserialize<T>(MyPacket packet)
		{
			// Создаем объект класса с атрибутами
			T instance = Activator.CreateInstance<T>();

			// Получаем поля и атрибуты класса
			List<(FieldInfo, byte)> fields = GetFields(typeof(T));

			// Проходимся по всем полям класса с атрибутами
			foreach ((FieldInfo, byte) tuple in fields)
			{
				FieldInfo field = tuple.Item1;
				byte packetFieldId = tuple.Item2;

				// Если в MyPacket элемента с id из атрибута, берем следующее поле
				if (packet.GetField(packetFieldId) == null)
				{
					continue;
				}

				// Получаем значение из MyPacket по id
				var value = typeof(MyPacket).GetMethod("GetValue")?
					.MakeGenericMethod(field.FieldType)
					.Invoke(packet, new object[] { packetFieldId });

				// Если не нашли значение с таким id, берем следующее поле
				if (value == null)
				{
					continue;
				}

				// Кладем в объект класса с атрибутами значение
				field.SetValue(instance, value);
			}

			return instance;
		}
	}
}
