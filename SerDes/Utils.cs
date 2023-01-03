using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SerDes
{
    public static class Utils
    {
        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;

            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        public static T ByteArrayToObject<T>(byte[] arrBytes) => (T) ByteArrayToObject(arrBytes);

        // Convert a byte array to an Object
        public static object ByteArrayToObject(byte[] arrBytes)
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);

            return obj;
        }
    }

    public static class Extensions
    {
        public static byte[] ToByteArray(this object obj) => Utils.ObjectToByteArray(obj);
    }
}