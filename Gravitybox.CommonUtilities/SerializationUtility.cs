using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Gravitybox.CommonUtilities
{
    public class SerializationUtility
    {
        public static byte[] Serialize<T>(T data)
        {
            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, data);
                var xml = writer.ToString();
                return Encoding.UTF8.GetBytes(xml);
            }
        }

        public static T Deserialize<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            string xml = null;
            //if (data.Length > 3 && (data[0] == 31 && data[1] == 139 && data[2] == 8))
            //{
            //    xml = data;
            //}
            //else
            {
                xml = System.Text.Encoding.UTF8.GetString(data);
            }

            if (string.IsNullOrEmpty(xml))
                return default(T);

            try
            {
                using (var reader = new StringReader(xml))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static T Deserialize<T>(string xml)
        {
            return Deserialize<T>(System.Text.Encoding.UTF8.GetBytes(xml));
        }

    }
}
