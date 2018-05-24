using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace JMtech.Translations
{
	public abstract class Translation
	{
		public void autoExec()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (FieldInfo fieldInfo in base.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				dictionary.Add(fieldInfo.Name, fieldInfo.GetValue(this).ToString());
			}
			foreach (FieldInfo fieldInfo2 in base.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				string text = fieldInfo2.GetValue(this).ToString();
				foreach (string text2 in dictionary.Keys)
				{
					text = text.Replace("%R:" + text2 + "%", dictionary[text2]);
				}
				fieldInfo2.SetValue(this, text);
			}
		}

		public abstract void Init();

		public static void loadTranslations<T>()
		{
			string text = Path.Combine(Application.persistentDataPath, "Translations");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			foreach (string path in Directory.GetFiles(text))
			{
				string fileName = Path.GetFileName(path);
				string extension = Path.GetExtension(fileName);
				if (!(extension != ".trans"))
				{
					string value = File.ReadAllText(path);
					T t = JsonConvert.DeserializeObject<T>(value);
					SystemLanguage key = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), Path.GetFileNameWithoutExtension(fileName));
					if (!key.Equals(null))
					{
						Translation.translations.Add(key, (Translation)((object)t));
						((Translation)((object)t)).Init();
					}
				}
			}
			if (Translation.translations.ContainsKey(Application.systemLanguage))
			{
				Translation.mainTranslation = Translation.translations[Application.systemLanguage];
			}
			else if (Translation.translations.ContainsKey(SystemLanguage.English))
			{
				Translation.mainTranslation = Translation.translations[SystemLanguage.English];
			}
			else
			{
				Translation.mainTranslation = (Translation)((object)Activator.CreateInstance<T>());
				string contents = JsonConvert.SerializeObject((T)((object)Translation.mainTranslation), Formatting.Indented);
				File.WriteAllText(Path.Combine(text, SystemLanguage.English.ToString() + ".trans"), contents);
				Debug.Log("Stored to " + text);
			}
		}

		public static void saveTranslation<T>()
		{
		}

		public List<string> getTranslatables()
		{
			List<string> list = new List<string>();
			foreach (FieldInfo fieldInfo in base.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (fieldInfo.FieldType == typeof(string) || fieldInfo.FieldType == typeof(string))
				{
					list.Add(fieldInfo.Name.ToString());
				}
			}
			return list;
		}

		public Dictionary<string, string> getTranslations()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (FieldInfo fieldInfo in base.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (fieldInfo.FieldType == typeof(string) || fieldInfo.FieldType == typeof(string))
				{
					dictionary.Add(fieldInfo.Name.ToString(), fieldInfo.GetValue(this).ToString());
				}
			}
			return dictionary;
		}

		public static Dictionary<SystemLanguage, Translation> translations = new Dictionary<SystemLanguage, Translation>();

		public static Translation mainTranslation;
	}
}
