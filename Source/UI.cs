using System;
using System.IO;
using JMtech.Translations;
using Newtonsoft.Json;
using UnityEngine;

public class UI
{
	private static void reload()
	{
		string path = Path.Combine(Application.persistentDataPath, "Translations");
		Translation translation = (Translation)Activator.CreateInstance<SFST>();
		string contents = JsonConvert.SerializeObject((SFST)translation, Formatting.Indented);
		string text = Path.Combine(path, SystemLanguage.English.ToString() + ".trans");
		File.WriteAllText(text, contents);
		Debug.Log("Refreshed translations from script [{0}]".FormatStr(text));
	}
}
