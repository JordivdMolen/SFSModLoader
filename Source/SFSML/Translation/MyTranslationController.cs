using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SFSML.Translation
{
	public class MyTranslationController<T>
	{
		public MyTranslationController(MyMod host) : this(host.getDataPath(), "translations")
		{
		}

		public MyTranslationController(string purePath) : this(purePath, "translations")
		{
		}

		public MyTranslationController(MyMod host, string name) : this(host.getDataPath(), name)
		{
		}

		public MyTranslationController(string purePath, string name)
		{
			this.translationPath = Path.Combine(purePath, name + ".translation");
			bool flag = !File.Exists(this.translationPath);
			if (flag)
			{
				this.translatable = Activator.CreateInstance<T>();
				File.WriteAllText(this.translationPath, JsonUtility.ToJson(this.translatable, true));
			}
			this.translatable = JsonUtility.FromJson<T>(File.ReadAllText(this.translationPath));
		}

		public string autoFormat(string translation, params object[] formatations)
		{
			bool flag = translation.StartsWith("@");
			T t;
			if (flag)
			{
				translation = translation.Substring(1);
				t = this.translatable;
				foreach (FieldInfo fieldInfo in t.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					bool flag2 = fieldInfo.Name.Equals(translation);
					if (flag2)
					{
						translation = fieldInfo.GetValue(this.translatable).ToString();
					}
				}
			}
			t = this.translatable;
			foreach (FieldInfo fieldInfo2 in t.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				bool flag3 = translation.Contains("%" + fieldInfo2.Name + "%");
				if (flag3)
				{
					translation = translation.Replace("%" + fieldInfo2.Name + "%", fieldInfo2.GetValue(this.translatable).ToString());
				}
			}
			for (int k = 0; k < formatations.Length; k++)
			{
				translation = translation.Replace("{" + k.ToString() + "}", formatations[k].ToString());
			}
			return translation;
		}

		public readonly string translationPath;

		public readonly T translatable;
	}
}
