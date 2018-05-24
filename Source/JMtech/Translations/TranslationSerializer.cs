using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JMtech.Translations
{
	public class TranslationSerializer
	{
		public string serialize<T>(T obj)
		{
			object obj2 = obj;
			FieldInfo[] fields = obj2.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				string name = fieldInfo.Name;
				string text = fieldInfo.GetValue(obj).ToString();
				this.Result = string.Concat(new string[]
				{
					this.Result,
					name,
					"=",
					text,
					"\n"
				});
			}
			return this.Result;
		}

		public T revert<T>(string txt)
		{
			string[] array = Regex.Split(txt, "\n");
			T t = Activator.CreateInstance<T>();
			Type type = t.GetType();
			foreach (string text in array)
			{
				if (text.Contains("="))
				{
					int num = text.IndexOf("=");
					string name = text.Substring(0, num);
					string value = text.Substring(num + 1, text.Length - num - 1);
					FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field != null)
					{
						field.SetValue(t, value);
					}
				}
			}
			return t;
		}

		public string Result;
	}
}
