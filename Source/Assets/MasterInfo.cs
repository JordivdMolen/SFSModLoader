using System;
using System.Diagnostics;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets
{
	public class MasterInfo
	{
		public static MasterInfo Fetch()
		{
			WWW www = new WWW("https://pastebin.com/raw/pZi4airN");
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!www.isDone)
			{
				if (stopwatch.ElapsedMilliseconds >= 1000L)
				{
					return null;
				}
			}
			string text = www.text;
			return JsonConvert.DeserializeObject<MasterInfo>(text);
		}

		public string REDIRECT;

		public VersionInfo Version;
	}
}
