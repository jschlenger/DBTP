using System.Xml.Serialization;

namespace DBT_API.Util
{
    public static class CopyHelper
    {
        public static T DeepCopyXML<T>(T input)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new XmlSerializer(input.GetType());
                serializer.Serialize(stream, input);
                stream.Position = 0;
                return (T)serializer.Deserialize(stream);
            }
        }
    }
}
