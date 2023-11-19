namespace ClassLibraryMyProtocol.MyPacketMain
{
    /// <summary>
    /// Атрибут, которым можно пометить поля пакета и указать их id.
    /// </summary>
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