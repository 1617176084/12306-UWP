using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using System.Runtime.Serialization.Json;
 

namespace huoche
{
    public class JsonHelper<T>
    {
        public static T Convert(Stream stream)
        {
            try
            {
                return (T)(Deserialize(stream, typeof(T)));

            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        public static object Deserialize(Stream streamObject, Type serializedObjectType)
        {
            if ((serializedObjectType == null) || (streamObject == null))
            {
                return null;
            }
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(serializedObjectType);

            return serializer.ReadObject(streamObject);
        }

        public static object Deserialize(string json, Type serializedObjectType)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(serializedObjectType);
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return serializer.ReadObject(ms);
            }

        }

        public static string Serialize(object objForSerialization)
        {
            string str = string.Empty;
            if (objForSerialization == null)
            {
                return str;
            }
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(objForSerialization.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, objForSerialization);
                byte[] buffer = new byte[stream.Length];
                stream.Position = 0L;
                stream.Read(buffer, 0, buffer.Length);
                return buffer.ToString();
            }
        }

        public static void Serialize(Stream streamObject, object objForSerialization)
        {
            if ((objForSerialization != null) && (streamObject != null))
            {
                new System.Runtime.Serialization.Json.DataContractJsonSerializer(objForSerialization.GetType()).WriteObject(streamObject, objForSerialization);
            }
        }
    }

}
