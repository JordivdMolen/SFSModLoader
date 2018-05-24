using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ServerTransferProgram
{
	public class Util
	{
		public static byte[] ToByteArray<T>(T obj)
		{
			if (obj == null)
			{
				return null;
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream, obj);
				result = memoryStream.ToArray();
			}
			return result;
		}

		public static T FromByteArray<T>(byte[] data)
		{
			if (data == null)
			{
				return default(T);
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			T result;
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				object obj = binaryFormatter.Deserialize(memoryStream);
				result = (T)((object)obj);
			}
			return result;
		}

		public static string Base64Encode(string plainText)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(bytes);
		}

		public static string Base64Decode(string base64EncodedData)
		{
			byte[] bytes = Convert.FromBase64String(base64EncodedData);
			return Encoding.UTF8.GetString(bytes);
		}
	}
}
