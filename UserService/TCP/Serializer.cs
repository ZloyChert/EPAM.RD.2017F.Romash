using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace UserService.TCP
{
    public static class Serializer
    {
        public static byte[] Serialize(MessageUsers anySerializableObject)
        {
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, anySerializableObject);
                return memoryStream.ToArray();
            }
        }

        public static MessageUsers DeserializeUsers(byte[] message)
        {
            using (var memoryStream = new MemoryStream(message))
                return (MessageUsers)(new BinaryFormatter()).Deserialize(memoryStream);
        }
    }
}
