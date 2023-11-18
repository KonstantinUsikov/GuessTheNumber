namespace ClassLibraryMyProtocol
{
	[AttributeUsage(AttributeTargets.Field)]
	public class MyFieldAttribute : Attribute
	{
		public byte FieldID { get; }

		public MyFieldAttribute(byte fieldId)
		{
			FieldID = fieldId;
		}
	}
}