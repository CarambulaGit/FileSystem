using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SerDes
{
    public static class Utils
    {
        public const int CharsForByte = Constants.BitesInByte;
        private const string EmptyByte = "\0\0\0\0\0\0\0\0";

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

        public static T ByteArrayToObject<T>(byte[] arrBytes) => (T) ByteArrayToObject(arrBytes);

        public static string ByteArrayToStr(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var singleByte in bytes)
            {
                sb.Append(Convert.ToString(singleByte, 2).PadLeft(CharsForByte, '0'));
            }

            return sb.ToString();
        }

        public static byte[] BinaryCharsArrayToByteArray(char[] c)
        {
            var result = new byte[c.Length / CharsForByte];
            for (int i = 0, b = 0; i < c.Length; i += CharsForByte, b++)
            {
                var str = $"{c[i]}{c[i + 1]}{c[i + 2]}{c[i + 3]}{c[i + 4]}{c[i + 5]}{c[i + 6]}{c[i + 7]}";
                if (str.Equals(EmptyByte)) break;
                result[b] = Convert.ToByte(str, 2);
            }

            return result;
        }
    }

    public static class Extensions
    {
        public static byte[] ToByteArray(this object obj) => Utils.ObjectToByteArray(obj);
        public static T To<T>(this byte[] byteArray) => Utils.ByteArrayToObject<T>(byteArray);
        public static string ByteArrayToBinaryStr(this byte[] bytes) => Utils.ByteArrayToStr(bytes);
        public static byte[] BinaryCharsArrayToByteArray(this char[] c) => Utils.BinaryCharsArrayToByteArray(c);
    }
}