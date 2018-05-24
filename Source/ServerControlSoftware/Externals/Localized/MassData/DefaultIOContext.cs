using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ServerControlSoftware.Externals.Localized.MassData
{
	public class DefaultIOContext
	{
		private string Relative()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		private string Relative(string path)
		{
			return Path.Combine(this.Relative(), path);
		}

		public T FetchFromFile<T>(string location)
		{
			string text = this.Relative(location);
			if (!File.Exists(text))
			{
				T t = Activator.CreateInstance<T>();
				File.WriteAllText(text, JsonConvert.SerializeObject(t));
				return t;
			}
			string text2 = File.ReadAllText(text);
			return JsonConvert.DeserializeObject<T>(text);
		}

		public void FetchFromFile<T>(string location, out T inputObject)
		{
			string path = this.Relative(location);
			if (!File.Exists(path))
			{
				T t = Activator.CreateInstance<T>();
				File.WriteAllText(path, JsonConvert.SerializeObject(t));
				inputObject = t;
				return;
			}
			string value = File.ReadAllText(path);
			inputObject = JsonConvert.DeserializeObject<T>(value);
		}

		public void StoreToFile(string location, object obj)
		{
			File.WriteAllText(this.Relative(location), JsonConvert.SerializeObject(obj, Formatting.Indented));
		}

		public int DirectoryCheck(string dir)
		{
			string path = this.Relative(dir);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
				return 1;
			}
			return 0;
		}

		public int DirectoryCheck(params string[] dir)
		{
			int num = 0;
			foreach (string dir2 in dir)
			{
				num += this.DirectoryCheck(dir2);
			}
			return num;
		}
	}
}
